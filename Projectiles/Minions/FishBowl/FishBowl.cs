using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.FishBowl
{
	public class FishBowlMinionBuff : MinionBuff
	{
		public FishBowlMinionBuff() : base(ProjectileType<FishBowlMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Flying Fishbowl");
			Description.SetDefault("A flying fishbowl will fight for you!");
		}
	}

	public class FishBowlMinionItem : MinionItem<FishBowlMinionBuff, FishBowlMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Fishbowl Staff");
			Tooltip.SetDefault("Summons a flying fishbowl to fight for you!\n"+
				"Most effective against flying enemies");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 14;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 2, 0);
			item.rare = ItemRarityID.White;
		}
	}

	/// <summary>
	/// uses ai[0] so owner can track it,
	/// localAI[0] so owner knows if it's hit a wall
	/// </summary>
	public class FishBowlFish : ModProjectile
	{

		public override string Texture => "Terraria/NPC_" + NPCID.Goldfish;
		const int TIME_TO_LIVE = 90;
		bool isFlopping = false;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
			Main.projFrames[projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.timeLeft = TIME_TO_LIVE;
			projectile.friendly = true;
			projectile.tileCollide = true;
			projectile.penetrate = -1;
			projectile.localNPCHitCooldown = 30;
		}

		public override void AI()
		{
			if(TIME_TO_LIVE - projectile.timeLeft > 10)
			{
				projectile.velocity.Y += 0.5f;
			}
			projectile.frameCounter++;
			if(isFlopping)
			{
				projectile.frame = 4 + (projectile.frameCounter / 8) % 2;
				projectile.rotation = 0;
			} else
			{
				projectile.frame = (projectile.frameCounter / 8) % 4;
				projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			isFlopping = true;
			projectile.friendly = false;
			if(oldVelocity.Y > 0 && projectile.velocity.Y == 0 || projectile.velocity.Y == -0.5f)
			{
				// let the bowl know it's time to go home
				projectile.localAI[0] = 1;
				projectile.velocity.X = 0;
			}
			return false;
		}

	}

	public class FishBowlMinion : HeadCirclingGroupAwareMinion
	{
		int lastFiredFrame = 0;
		int side = -1;
		internal override int BuffId => BuffType<FishBowlMinionBuff>();

		Projectile launchedFish = default;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying FishBowl");
			Main.projFrames[projectile.type] = 17;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			drawOriginOffsetX = -4;
			drawOriginOffsetY = -2;
			attackFrames = 30;
			frameSpeed = 15;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			projectile.rotation = 0.05f * projectile.velocity.X;
			if(launchedFish != default)
			{
				projectile.frame = 0;
			} else
			{
				base.Animate(4, maxFrame);
			}
			if(vectorToTarget is Vector2 target)
			{
				projectile.spriteDirection = Math.Sign(target.X);
			}
		}


		public override Vector2 IdleBehavior()
		{
			//check if we've got a fish in the air
			launchedFish = default;
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == Main.myPlayer && p.type == ProjectileType<FishBowlFish>() && p.ai[0] == projectile.whoAmI)
				{
					launchedFish = p;
					break;
				}
			}
			return base.IdleBehavior();
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(launchedFish != default)
			{
				DoFishCatchingMovement();
			} else
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(launchedFish != default)
			{
				DoFishCatchingMovement();
			} else
			{
				DoTargetedMovement(vectorToTargetPosition);
			}
		}

		private void DoTargetedMovement(Vector2 vectorToTargetPosition)
		{
			// prefer a different side based on minion index
			List<Projectile> minions = GetActiveMinions();
			if (side == -1 && minions.Count > 0)
			{
				side = minions.IndexOf(projectile) % 2;
			} else if (side == -1)
			{
				side = 0;
			}

			Vector2 targetTop = default;
			if(targetNPCIndex is int idx && Main.npc[idx].active)
			{
				targetTop = Main.npc[idx].Top - projectile.Center;
				targetTop += 2 * Main.npc[idx].velocity;
				Vector2 newTarget = side == 0 ? Main.npc[idx].BottomRight : Main.npc[idx].BottomLeft; 
				vectorToTargetPosition = newTarget - projectile.Center;
			}
			int targetBelow = 80;
			Vector2 vectorBelow = vectorToTargetPosition;
			// angle to travel downwards at
			// prefer to be below the enemy at a ~30 degree angle
			float losCheckAngle = MathHelper.Pi / 6 + 2 * MathHelper.Pi * side / 3;

			Vector2 losCheckVector = losCheckAngle.ToRotationVector2();
			projectile.friendly = false;

			// only check for exact position once close to target
			if (vectorToTargetPosition.LengthSquared() < 256 * 256)
			{
				for (int i = 16; i < targetBelow; i++)
				{
					vectorBelow = vectorToTargetPosition + i * losCheckVector;
					if (!Collision.CanHit(projectile.Center, 1, 1, projectile.Center + vectorBelow, 1, 1))
					{
						break;
					}
				}
			}
			DistanceFromGroup(ref vectorBelow);
			// this is a bit of a touchy launch condition, may want to revisit
			if (Main.myPlayer == player.whoAmI && IsMyTurn() && targetTop != default && animationFrame - lastFiredFrame >= attackFrames && vectorBelow.Length() < 32)
			{
				lastFiredFrame = animationFrame;
				// hit the target in ~8 frames;
				targetTop /= 8;
				if(targetTop.Length() > 8)
				{
					targetTop.SafeNormalize();
					targetTop *= 8;
				} else if (targetTop.Length() < 4)
				{
					targetTop.SafeNormalize();
					targetTop *= 4;
				}
				Projectile.NewProjectile(
					projectile.Center,
					targetTop,
					ProjectileType<FishBowlFish>(),
					projectile.damage,
					projectile.knockBack,
					player.whoAmI,
					ai0: projectile.whoAmI);
				for(int i = 0; i < 5; i++)
				{
					Dust.NewDust(projectile.Top, projectile.width, 16, 13, -projectile.velocity.X / 4, -projectile.velocity.Y / 4, newColor: Color.LightBlue);
				}
			}
			vectorBelow.SafeNormalize();
			vectorBelow *= 8;
			int inertia = 12;
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorBelow) / inertia;
		}

		private void DoFishCatchingMovement()
		{
			projectile.tileCollide = false;
			// cop out, always be under the fish
			projectile.Center = new Vector2(launchedFish.Center.X, projectile.Center.Y);
			projectile.velocity.X = launchedFish.velocity.X;
			// start moving Y-wise to catch the fish once it's hit a tile
			int yIntertia = 8;
			if(launchedFish.localAI[0] == 1)
			{
				// regular intertia code, but only for Y
				float yOffset = launchedFish.position.Y - projectile.position.Y;
				if(Math.Abs(yOffset) > 8)
				{
					yOffset = Math.Sign(yOffset) * 8;
				}
				projectile.velocity.Y = (projectile.velocity.Y * (yIntertia - 1) + yOffset) / yIntertia;
			} else
			{
				projectile.velocity.Y = 0;
			}
			if(animationFrame - lastFiredFrame > 8 && Vector2.DistanceSquared(projectile.Center, launchedFish.Center) < 16 * 16)
			{
				projectile.tileCollide = true;
				// (hopefully) get out of any blocks we were stuck in
				Vector2 catchVelocity = -launchedFish.velocity;
				catchVelocity.SafeNormalize();
				catchVelocity.X *= 3;
				catchVelocity.Y *= 6;
				if(Framing.GetTileSafely((int)projectile.Center.X/16, (int)projectile.Center.Y/16).active())
				{
					projectile.Bottom = launchedFish.Bottom;
				} else
				{
					// otherwise, get "kicked back" by the catch
					projectile.velocity -= catchVelocity;
				}
				for(int i = 0; i < 5; i++)
				{
					Dust.NewDust(projectile.Top, projectile.width, 16, 13, catchVelocity.X / 4, catchVelocity.Y / 4, newColor: Color.LightBlue);
				}
				side = side == 0 ? 1 : 0;
				Main.PlaySound(new LegacySoundStyle(19, 1), projectile.Center);
				launchedFish.Kill();
			}

		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(Texture);
			Texture2D waterTexture = GetTexture(base.Texture+ "_Water");
			Texture2D bowlTexture = GetTexture(base.Texture+ "_Bowl");
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Color waterColor = lightColor.MultiplyRGBA(new Color(196, 196, 196, 128));
			spriteBatch.Draw(waterTexture, pos - Main.screenPosition, waterTexture.Bounds, waterColor, 0, origin, 1, 0, 0);
			spriteBatch.Draw(texture, pos - Main.screenPosition, bounds, lightColor, 0, origin, 1, 0, 0);
			spriteBatch.Draw(bowlTexture, pos - Main.screenPosition, bowlTexture.Bounds, lightColor, r, origin, 1, effects, 0);
			DrawWings(spriteBatch, lightColor);
			return false;
		}

		private void DrawWings(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture + "_Wings");
			int frame = (animationFrame / 8) % 4;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Vector2 wingsOffset = new Vector2(-8 * projectile.spriteDirection, -4);
			int frameHeight = texture.Height / 4;
			Rectangle bounds = new Rectangle(0, frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos + wingsOffset - Main.screenPosition, bounds, lightColor, 0, origin, 1, effects, 0);

		}
	}
}
