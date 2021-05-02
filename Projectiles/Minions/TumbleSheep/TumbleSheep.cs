using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.TumbleSheep
{

	public class TumbleSheepMinionBuff : MinionBuff
	{
		public TumbleSheepMinionBuff() : base(ProjectileType<TumbleSheepMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("TumbleSheep");
			Description.SetDefault("A slime miner will fight for you!");
		}
	}

	public class TumbleSheepMinionItem : MinionItem<TumbleSheepMinionBuff, TumbleSheepMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("TumbleSheep Staff");
			Tooltip.SetDefault("Summons slime miner to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 11;
			item.knockBack = 4f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 5, 0);
			item.rare = ItemRarityID.White;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Minecart, 1);
			recipe.AddIngredient(ItemID.MiningHelmet, 1);
			recipe.AddRecipeGroup("AmuletOfManyMinions:Silvers", 12);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class TumbleSheepMinion : SimpleGroundBasedMinion
	{
		internal override int BuffId => BuffType<TumbleSheepMinionBuff>();
		static int bounceCycleLength = 60;
		int lastFiredFrame = -bounceCycleLength;
		// don't get too close
		int preferredDistanceFromTarget = 64;

		private Vector2 launchPos;

		private bool IsBouncing => animationFrame - lastFiredFrame < bounceCycleLength;

		private SpriteCompositionHelper scHelper;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.TumbleSheep"));
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 32;
			attackFrames = 90;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			searchDistance = 900;
			maxJumpVelocity = 12;
			scHelper = new SpriteCompositionHelper(this);
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			projectile.damage = (int)(1.5f * projectile.damage);
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -3 * projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = 11;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			// only change speed if the target is a decent distance away
			if (Math.Abs(vector.X) < 4 && targetNPCIndex is int idx && Math.Abs(Main.npc[idx].velocity.X) < 7)
			{
				projectile.velocity.X = Main.npc[idx].velocity.X;
			}
			else
			{
				projectile.velocity.X = (projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
		}

		private void LaunchBounce(Vector2 vectorToTarget)
		{
			lastFiredFrame = animationFrame;
			launchPos = projectile.position;
			Main.PlaySound(new LegacySoundStyle(2, 17), projectile.position);
			if(gHelper.didJustLand && vectorToTarget.Y > -Math.Abs(vectorToTarget.X/4))
			{
				vectorToTarget.Y = -Math.Abs(vectorToTarget.X / 4);
			}
			vectorToTarget.SafeNormalize();
			vectorToTarget *= 8;
			projectile.velocity = vectorToTarget;
			for(int i = 0; i < 3; i++)
			{
				int idx = Dust.NewDust(projectile.BottomLeft - new Vector2(0, 16), 32, 16, 16, 0, 0);
				Main.dust[idx].alpha = 112;
				Main.dust[idx].scale = 1.2f;
			}
			// TODO
		}

		// If your name is AG...
		private void DoBounce()
		{
			if(animationFrame - lastFiredFrame > 10 && projectile.velocity.Y < 16)
			{
				projectile.velocity.Y += 0.5f;
			}
			if(Vector2.DistanceSquared(launchPos, projectile.position) > 240 * 240)
			{
				// snap out of bounce if we go too far in a straight line
				lastFiredFrame = animationFrame - bounceCycleLength;
			}

		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(IsBouncing)
			{
				DoBounce();
			} else
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{

			if(IsBouncing)
			{
				DoBounce();
				return;
			}

			if (Math.Abs(vectorToTargetPosition.X) < 4 * preferredDistanceFromTarget &&
				Math.Abs(vectorToTargetPosition.Y) < preferredDistanceFromTarget &&
				animationFrame - lastFiredFrame >= attackFrames)
			{
				LaunchBounce(vectorToTargetPosition);
				return;
			}

			// don't move if we're in range-ish of the target
			if (Math.Abs(vectorToTargetPosition.X) < 2 * preferredDistanceFromTarget && Math.Abs(vectorToTargetPosition.X) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X = 0;
			}
			if(Math.Abs(vectorToTargetPosition.Y) < 2 * preferredDistanceFromTarget && Math.Abs(vectorToTargetPosition.Y) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.Y = 0;
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override void AfterMoving()
		{
			projectile.friendly = IsBouncing;
			projectile.tileCollide |= IsBouncing;
			scHelper.UpdateMovement();
		}

		private void DrawLegs(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			// used for both idle and active
			if(gHelper.isFlying && !IsBouncing)
			{
				return;
			}
			cycleAngle = helper.IsWalking ? cycleAngle : 0;
			for(int i = 0; i < 2; i++)
			{
				Vector2 legRotationAngle = (cycleAngle + i * MathHelper.Pi).ToRotationVector2();
				legRotationAngle.X *= 4;
				legRotationAngle.Y *= 1.5f;
				helper.AddSpriteToBatch(GetTexture(Texture + "_Legs" + (i+1)), legRotationAngle + new Vector2(0, 14));
			}
		}

		private void DrawClouds(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			if(IsBouncing || !gHelper.isFlying)
			{
				return;
			}
			for(int i = 0; i < 3; i++)
			{
				float mySin = (float)Math.Sin(cycleAngle + i * MathHelper.TwoPi / 3);
				Vector2 offsetVector = new Vector2(12 * (i - 1), 12 + 3 * mySin);
				float r = -MathHelper.Pi / 64 + MathHelper.Pi / 32 * (float)mySin;
				byte oldAlpha = helper.lightColor.A;
				helper.lightColor.A = 128;
				helper.AddSpriteToBatch(GetTexture(Texture), (i,3), offsetVector, r, 0.5f + 0.1f * mySin);
				helper.lightColor.A = oldAlpha;
			}
		}

		private void DrawBodyWalking(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			frame %= helper.walkCycleFrames;
			int textureFrame = frame < 15 ? 0 : frame < 30 ? 1 : frame < 45 ? 2 : 1;
			float r = -MathHelper.Pi / 64 + MathHelper.Pi / 32 * (float)Math.Sin(cycleAngle);
			// body
			Vector2 offsetVector = (-1 + 2f * (float)Math.Sin(2 * cycleAngle)) * Vector2.UnitY;
			helper.AddSpriteToBatch(GetTexture(Texture), (textureFrame, 3), offsetVector, r, 1);
			// head
			offsetVector = new Vector2(projectile.spriteDirection * 10, -2 + 1.5f * (float)Math.Sin(2 * cycleAngle));
			helper.AddSpriteToBatch(GetTexture(Texture + "_Head"), offsetVector);
		}

		private void DrawBodyIdle(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			// body
			Vector2 offsetVector = (-1 + 2f * (float)Math.Sin(cycleAngle + MathHelper.Pi / 2)) * Vector2.UnitY;
			helper.AddSpriteToBatch(GetTexture(Texture), (1, 3),  offsetVector);
			
			// head
			offsetVector = new Vector2(projectile.spriteDirection * 10, -2 + 1.5f * (float)Math.Sin(cycleAngle));
			scHelper.AddSpriteToBatch(GetTexture(Texture + "_Head"), offsetVector);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if(scHelper.IsWalking)
			{
				scHelper.Process(spriteBatch, lightColor, true, DrawLegs, DrawBodyWalking, DrawClouds);
			} else
			{
				scHelper.Process(spriteBatch, lightColor, false, DrawLegs, DrawBodyIdle, DrawClouds);
			}
			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if(IsBouncing)
			{
				bool hitFloor = (projectile.velocity.Y == 0 || projectile.velocity.Y == -0.5f) && oldVelocity.Y >= 0;
				bool hitCeil = (projectile.velocity.Y == 0 || projectile.velocity.Y == 0.5f) && oldVelocity.Y < 0;
				bool hitWall = projectile.velocity.X == 0;
				if(hitFloor)
				{
					projectile.velocity.Y = Math.Min(-4, -projectile.oldVelocity.Y * 0.95f);
				} 				
				if(hitCeil)
				{
					projectile.velocity.Y = Math.Max(4, -projectile.oldVelocity.Y);
				}
				if(hitWall)
				{
					projectile.velocity.X = -oldVelocity.X;
				}
				return false;
			} else
			{
				return base.OnTileCollide(oldVelocity);
			}
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(IsBouncing)
			{
				projectile.spriteDirection = Math.Sign(projectile.velocity.X);
				projectile.rotation += projectile.spriteDirection * MathHelper.Pi / 16;
				return;
			} 				
			projectile.rotation = 0;
			if (vectorToTarget is Vector2 target && Math.Abs(target.X) < 1.5 * preferredDistanceFromTarget)
			{
				projectile.spriteDirection = Math.Sign(target.X);
			} else if (projectile.velocity.X > 1)
			{
				projectile.spriteDirection = 1;
			} else if (projectile.velocity.X < -1)
			{
				projectile.spriteDirection = -1;
			}
		}
	}
}
