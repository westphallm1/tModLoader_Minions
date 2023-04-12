using AmuletOfManyMinions.Core;
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
using static AmuletOfManyMinions.CrossModClient.SummonersShine.CrossModSetup;

namespace AmuletOfManyMinions.Projectiles.Minions.FishBowl
{
	public class FishBowlMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<FishBowlMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
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
		public override void ApplyCrossModChanges()
		{
			WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, SummonersShineDefaultSpecialWhitelistType.RANGED);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 14;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.buyPrice(0, 0, 2, 0);
			Item.rare = ItemRarityID.White;
		}
	}

	/// <summary>
	/// uses ai[0] so owner can track it,
	/// localAI[0] so owner knows if it's hit a wall
	/// </summary>
	public class FishBowlFish : ModProjectile
	{

		public override string Texture => "Terraria/Images/NPC_" + NPCID.Goldfish;
		const int TIME_TO_LIVE = 90;
		bool isFlopping = false;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = TIME_TO_LIVE;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
			Projectile.localNPCHitCooldown = 30;
		}

		public override void AI()
		{
			if(TIME_TO_LIVE - Projectile.timeLeft > 10)
			{
				Projectile.velocity.Y += 0.5f;
			}
			Projectile.frameCounter++;
			if(isFlopping)
			{
				Projectile.frame = 4 + (Projectile.frameCounter / 8) % 2;
				Projectile.rotation = 0;
			} else
			{
				Projectile.frame = (Projectile.frameCounter / 8) % 4;
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			isFlopping = true;
			Projectile.friendly = false;
			if(oldVelocity.Y > 0 && Projectile.velocity.Y == 0 || Projectile.velocity.Y == -0.5f)
			{
				// let the bowl know it's time to go home
				Projectile.localAI[0] = 1;
				Projectile.velocity.X = 0;
			}
			return false;
		}

	}

	public class FishBowlMinion : HeadCirclingGroupAwareMinion
	{
		int lastFiredFrame = 0;
		int side = -1;
		public override int BuffId => BuffType<FishBowlMinionBuff>();

		Projectile launchedFish = default;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying FishBowl");
			Main.projFrames[Projectile.type] = 17;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		public override void LoadAssets()
		{
			AddTexture(Texture + "_Water");
			AddTexture(Texture + "_Bowl");
			AddTexture(Texture + "_Wings");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			DrawOriginOffsetX = -4;
			DrawOriginOffsetY = -2;
			attackFrames = 30;
			FrameSpeed = 15;
			DealsContactDamage = false;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			Projectile.rotation = 0.05f * Projectile.velocity.X;
			if(launchedFish != default)
			{
				Projectile.frame = 0;
			} else
			{
				base.Animate(4, maxFrame);
			}
			if(VectorToTarget is Vector2 target)
			{
				Projectile.spriteDirection = Math.Sign(target.X);
			}
		}


		public override Vector2 IdleBehavior()
		{
			//check if we've got a fish in the air
			launchedFish = default;
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == Main.myPlayer && p.type == ProjectileType<FishBowlFish>() && p.ai[0] == Projectile.whoAmI)
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
				side = minions.IndexOf(Projectile) % 2;
			} else if (side == -1)
			{
				side = 0;
			}

			Vector2 targetTop = default;
			if(TargetNPCIndex is int idx && Main.npc[idx].active)
			{
				targetTop = Main.npc[idx].Top - Projectile.Center;
				targetTop += 2 * Main.npc[idx].velocity;
				Vector2 newTarget = side == 0 ? Main.npc[idx].BottomRight : Main.npc[idx].BottomLeft; 
				vectorToTargetPosition = newTarget - Projectile.Center;
			}
			int targetBelow = 80;
			Vector2 vectorBelow = vectorToTargetPosition;
			// angle to travel downwards at
			// prefer to be below the enemy at a ~30 degree angle
			float losCheckAngle = MathHelper.Pi / 6 + 2 * MathHelper.Pi * side / 3;

			Vector2 losCheckVector = losCheckAngle.ToRotationVector2();

			// only check for exact position once close to target
			if (vectorToTargetPosition.LengthSquared() < 256 * 256)
			{
				for (int i = 16; i < targetBelow; i++)
				{
					vectorBelow = vectorToTargetPosition + i * losCheckVector;
					if (!Collision.CanHit(Projectile.Center, 1, 1, Projectile.Center + vectorBelow, 1, 1))
					{
						break;
					}
				}
			}
			DistanceFromGroup(ref vectorBelow);
			// this is a bit of a touchy launch condition, may want to revisit
			if (Main.myPlayer == Player.whoAmI && IsMyTurn() && targetTop != default && AnimationFrame - lastFiredFrame >= attackFrames && vectorBelow.Length() < 32)
			{
				lastFiredFrame = AnimationFrame;
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
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					targetTop,
					ProjectileType<FishBowlFish>(),
					Projectile.damage,
					Projectile.knockBack,
					Player.whoAmI,
					ai0: Projectile.whoAmI);
				for(int i = 0; i < 5; i++)
				{
					Dust.NewDust(Projectile.Top, Projectile.width, 16, 13, -Projectile.velocity.X / 4, -Projectile.velocity.Y / 4, newColor: Color.LightBlue);
				}
			}
			vectorBelow.SafeNormalize();
			vectorBelow *= 8;
			int inertia = 12;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorBelow) / inertia;
		}

		private void DoFishCatchingMovement()
		{
			Projectile.tileCollide = false;
			// cop out, always be under the fish
			Projectile.Center = new Vector2(launchedFish.Center.X, Projectile.Center.Y);
			Projectile.velocity.X = launchedFish.velocity.X;
			// start moving Y-wise to catch the fish once it's hit a tile
			int yIntertia = 8;
			if(launchedFish.localAI[0] == 1)
			{
				// regular intertia code, but only for Y
				float yOffset = launchedFish.position.Y - Projectile.position.Y;
				if(Math.Abs(yOffset) > 8)
				{
					yOffset = Math.Sign(yOffset) * 8;
				}
				Projectile.velocity.Y = (Projectile.velocity.Y * (yIntertia - 1) + yOffset) / yIntertia;
			} else
			{
				Projectile.velocity.Y = 0;
			}
			if(AnimationFrame - lastFiredFrame > 8 && Vector2.DistanceSquared(Projectile.Center, launchedFish.Center) < 16 * 16)
			{
				Projectile.tileCollide = true;
				// (hopefully) get out of any blocks we were stuck in
				Vector2 catchVelocity = -launchedFish.velocity;
				catchVelocity.SafeNormalize();
				catchVelocity.X *= 3;
				catchVelocity.Y *= 6;
				if(Framing.GetTileSafely((int)Projectile.Center.X/16, (int)Projectile.Center.Y/16).HasTile)
				{
					Projectile.Bottom = launchedFish.Bottom;
				} else
				{
					// otherwise, get "kicked back" by the catch
					Projectile.velocity -= catchVelocity;
				}
				for(int i = 0; i < 5; i++)
				{
					Dust.NewDust(Projectile.Top, Projectile.width, 16, 13, catchVelocity.X / 4, catchVelocity.Y / 4, newColor: Color.LightBlue);
				}
				side = side == 0 ? 1 : 0;
				SoundEngine.PlaySound(SoundID.SplashWeak, Projectile.Center);
				launchedFish.Kill();
			}

		}

		public override bool PreDraw(ref Color lightColor)
		{
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Texture2D waterTexture = ExtraTextures[0].Value;
			Texture2D bowlTexture = ExtraTextures[1].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = bounds.GetOrigin();
			Color waterColor = lightColor.MultiplyRGBA(new Color(196, 196, 196, 128));
			Main.EntitySpriteDraw(waterTexture, pos - Main.screenPosition, waterTexture.Bounds, waterColor, 0, origin, 1, 0, 0);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition, bounds, lightColor, 0, origin, 1, 0, 0);
			Main.EntitySpriteDraw(bowlTexture, pos - Main.screenPosition, bowlTexture.Bounds, lightColor, r, origin, 1, effects, 0);
			DrawWings(lightColor);
			return false;
		}

		private void DrawWings(Color lightColor)
		{
			Texture2D texture = ExtraTextures[2].Value;
			int frame = (AnimationFrame / 8) % 4;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Vector2 wingsOffset = new Vector2(-8 * Projectile.spriteDirection, -4);
			int frameHeight = texture.Height / 4;
			Rectangle bounds = new Rectangle(0, frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = bounds.GetOrigin();
			Main.EntitySpriteDraw(texture, pos + wingsOffset - Main.screenPosition, bounds, lightColor, 0, origin, 1, effects, 0);

		}
	}
}
