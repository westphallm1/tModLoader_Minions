using AmuletOfManyMinions.Core;
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
using static AmuletOfManyMinions.CrossModClient.SummonersShine.CrossModSetup;

namespace AmuletOfManyMinions.Projectiles.Minions.TumbleSheep
{

	public class TumbleSheepMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<TumbleSheepMinion>() };
	}

	public class TumbleSheepMinionItem : MinionItem<TumbleSheepMinionBuff, TumbleSheepMinion>
	{
		public override void ApplyCrossModChanges()
		{
			WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, SummonersShineDefaultSpecialWhitelistType.MELEE);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 11;
			Item.knockBack = 4f;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.buyPrice(0, 0, 5, 0);
			Item.rare = ItemRarityID.White;
		}
	}

	public class TumbleSheepMinion : SimpleGroundBasedMinion
	{
		public override int BuffId => BuffType<TumbleSheepMinionBuff>();
		static int bounceCycleLength = 45;
		int lastFiredFrame = -bounceCycleLength;
		// don't get too close
		int preferredDistanceFromTarget = 64;

		private Vector2 launchPos;

		private bool IsBouncing => AnimationFrame - lastFiredFrame < bounceCycleLength;

		private SpriteCompositionHelper scHelper;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault(Language.GetTextValue("ProjectileName.TumbleSheep"));
		}
		public override void LoadAssets()
		{
			AddTexture(Texture + "_Legs1");
			AddTexture(Texture + "_Legs2");
			AddTexture(Texture + "_Head");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 32;
			attackFrames = 60;
			NoLOSPursuitTime = 300;
			StartFlyingHeight = 96;
			StartFlyingDist = 64;
			DefaultJumpVelocity = 4;
			searchDistance = 900;
			MaxJumpVelocity = 12;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			scHelper = new SpriteCompositionHelper(this, new Rectangle(0, 0, 48, 48));
			Projectile.originalDamage = (int)(1.5f * Projectile.originalDamage);
			scHelper.Attach();
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -3 * Projectile.height && Math.Abs(vector.X) < StartFlyingHeight)
			{
				GHelper.DoJump(vector);
			}
			float xInertia = GHelper.stuckInfo.overLedge && !GHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = 11;
			if (VectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = Player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			// only change speed if the target is a decent distance away
			if (Math.Abs(vector.X) < 4 && TargetNPCIndex is int idx && Math.Abs(Main.npc[idx].velocity.X) < 7)
			{
				Projectile.velocity.X = Main.npc[idx].velocity.X;
			}
			else
			{
				Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
		}

		private void LaunchBounce(Vector2 vectorToTarget)
		{
			lastFiredFrame = AnimationFrame;
			launchPos = Projectile.position;
			SoundEngine.PlaySound(SoundID.Item17, Projectile.position);
			if(TargetNPCIndex is int idx && Main.npc[idx].active)
			{
				vectorToTarget += 4 * Main.npc[idx].velocity; // track the target NPC a bit
			}
			if(GHelper.didJustLand && vectorToTarget.Y > -Math.Abs(vectorToTarget.X/4))
			{
				vectorToTarget.Y = -Math.Abs(vectorToTarget.X / 4);
			}
			vectorToTarget.SafeNormalize();
			vectorToTarget *= 8;
			Projectile.velocity = vectorToTarget;
			for(int i = 0; i < 3; i++)
			{
				int dustIdx = Dust.NewDust(Projectile.BottomLeft - new Vector2(0, 16), 32, 16, 16, 0, 0);
				Main.dust[dustIdx].alpha = 112;
				Main.dust[dustIdx].scale = 1.2f;
			}
		}

		// If your name is AG...
		private void DoBounce()
		{
			Projectile.tileCollide = true;
			if(AnimationFrame - lastFiredFrame > 10 && Projectile.velocity.Y < 16)
			{
				Projectile.velocity.Y += 0.5f;
			}
			if(Vector2.DistanceSquared(launchPos, Projectile.position) > 240 * 240)
			{
				// snap out of bounce if we go too far in a straight line
				lastFiredFrame = AnimationFrame - bounceCycleLength;
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
				AnimationFrame - lastFiredFrame >= attackFrames)
			{
				LaunchBounce(vectorToTargetPosition);
				return;
			}

			// don't move if we're in range-ish of the target
			if (Math.Abs(vectorToTargetPosition.X) < 2 * preferredDistanceFromTarget && Math.Abs(vectorToTargetPosition.X) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X = 0;
			}
			if(Math.Abs(vectorToTargetPosition.Y) < preferredDistanceFromTarget && Math.Abs(vectorToTargetPosition.Y) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.Y = 0;
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override void AfterMoving()
		{
			// manually set
			Projectile.friendly = IsBouncing;
			Projectile.tileCollide |= IsBouncing;
			scHelper.UpdateMovement();
			if(scHelper.IsWalking)
			{
				scHelper.UpdateDrawers(true, DrawLegs, DrawBodyWalking, DrawClouds);
			} else
			{
				scHelper.UpdateDrawers(false, DrawLegs, DrawBodyIdle, DrawClouds);
			}
		}

		private void DrawLegs(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			// used for both idle and active
			if(GHelper.isFlying && !IsBouncing)
			{
				return;
			}
			cycleAngle = helper.IsWalking ? cycleAngle : 0;
			for(int i = 0; i < 2; i++)
			{
				Vector2 legRotationAngle = (cycleAngle + i * MathHelper.Pi).ToRotationVector2();
				legRotationAngle.X *= 4;
				legRotationAngle.Y *= 1.5f;
				helper.AddSpriteToBatch(ExtraTextures[i].Value, legRotationAngle + new Vector2(0, 14));
			}
		}

		private void DrawClouds(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			if(IsBouncing || !GHelper.isFlying)
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
				helper.AddSpriteToBatch(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value, (i,3), offsetVector, r, 0.5f + 0.1f * mySin);
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
			helper.AddSpriteToBatch(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value, (textureFrame, 3), offsetVector, r, 1);
			// head
			offsetVector = new Vector2(10, -2 + 1.5f * (float)Math.Sin(2 * cycleAngle));
			helper.AddSpriteToBatch(ExtraTextures[2].Value, offsetVector);
		}

		private void DrawBodyIdle(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			// body
			Vector2 offsetVector = (-1 + 2f * (float)Math.Sin(cycleAngle + MathHelper.Pi / 2)) * Vector2.UnitY;
			helper.AddSpriteToBatch(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value, (1, 3),  offsetVector);
			
			// head
			offsetVector = new Vector2(10, -2 + 1.5f * (float)Math.Sin(cycleAngle));
			scHelper.AddSpriteToBatch(ExtraTextures[2].Value, offsetVector);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			scHelper.Draw(lightColor);
			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if(IsBouncing)
			{
				bool hitFloor = (Projectile.velocity.Y == 0 || Projectile.velocity.Y == -0.5f) && oldVelocity.Y >= 0;
				bool hitCeil = (Projectile.velocity.Y == 0 || Projectile.velocity.Y == 0.5f) && oldVelocity.Y < 0;
				bool hitWall = Projectile.velocity.X == 0;
				if(hitFloor)
				{
					Projectile.velocity.Y = Math.Min(-4, -Projectile.oldVelocity.Y * 0.95f);
				} 				
				if(hitCeil)
				{
					Projectile.velocity.Y = Math.Max(4, -Projectile.oldVelocity.Y);
				}
				if(hitWall)
				{
					Projectile.velocity.X = -oldVelocity.X;
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
				Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
				Projectile.rotation += Projectile.spriteDirection * MathHelper.Pi / 16;
				return;
			} 				
			Projectile.rotation = 0;
			if (VectorToTarget is Vector2 target && Math.Abs(target.X) < 1.5 * preferredDistanceFromTarget)
			{
				Projectile.spriteDirection = Math.Sign(target.X);
			} else if (Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = 1;
			} else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = -1;
			}
		}
	}
}
