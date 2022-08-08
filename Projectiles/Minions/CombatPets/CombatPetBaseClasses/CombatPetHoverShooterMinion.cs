using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses
{
	public abstract class CombatPetHoverShooterMinion : HoverShooterMinion
	{
		internal LeveledCombatPetModPlayer leveledPetPlayer;

		internal int forwardDir = 1;
		internal virtual int GetAttackFrames(ICombatPetLevelInfo info) => Math.Max(30, 60 - 6 * info.Level);
		internal virtual int GetProjectileVelocity(ICombatPetLevelInfo info) => (int)info.BaseSpeed + 3;
		internal virtual float DamageMult => 1f;

		internal virtual bool DoBumblingMovement => false;

		// counters for bumbling movement
		private int framesSinceLastHit;
		private int cooldownAfterHitFrames => 144/ (int)leveledPetPlayer.PetLevelInfo.BaseSpeed;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
			Projectile.width = 32;
			Projectile.height = 32;
			// these are likely to be static
			hsHelper.targetInnerRadius = 96;
			hsHelper.targetOuterRadius = 128;
			hsHelper.targetShootProximityRadius = 96;
			// go slower and smaller circle than minions since it's a cute little pet
			circleHelper.idleBumbleFrames = 90;
			circleHelper.idleBumbleRadius = 96;
			UpdateHsHelperWithPetLevel(0);
		}

		public override Vector2 IdleBehavior()
		{
			leveledPetPlayer = Player.GetModPlayer<LeveledCombatPetModPlayer>();
			Projectile.originalDamage = (int)(DamageMult * leveledPetPlayer.PetDamage);
			UpdateHsHelperWithPetLevel(leveledPetPlayer.PetLevel);
			DealsContactDamage = DoBumblingMovement;
			CrossMod.CombatPetComputeMinionStats(Projectile, leveledPetPlayer);
			return base.IdleBehavior();
		}

		internal void BumblingMovement(Vector2 vectorToTargetPosition)
		{

			float inertia = Math.Max(16, 30 - 3 * leveledPetPlayer.PetLevel);
			float speed = hsHelper.travelSpeed;
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= speed;
			framesSinceLastHit++;
			if (framesSinceLastHit < cooldownAfterHitFrames && framesSinceLastHit > cooldownAfterHitFrames / 2)
			{
				// start turning so we don't double directly back
				Vector2 turnVelocity = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X) / 8;
				turnVelocity *= Math.Sign(Projectile.velocity.X);
				Projectile.velocity += turnVelocity;
			}
			else if (framesSinceLastHit++ > cooldownAfterHitFrames)
			{
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
			else
			{
				Projectile.velocity.SafeNormalize();
				Projectile.velocity *= Math.Min(0.75f*speed, 10); // kick it away from enemies that it's just hit
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(DoBumblingMovement)
			{
				BumblingMovement(vectorToTargetPosition);
			} else
			{
				framesSinceLastHit = cooldownAfterHitFrames;
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		public override void OnHitTarget(NPC target)
		{
			framesSinceLastHit = 0;
		}
		
		private void UpdateHsHelperWithPetLevel(int petLevel)
		{
			ICombatPetLevelInfo info = CombatPetLevelTable.PetLevelTable[petLevel];
			targetSearchDistance = info.BaseSearchRange;
			attackFrames = GetAttackFrames(info);
			hsHelper.projectileVelocity = GetProjectileVelocity(info);
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = (int)info.BaseSpeed;
			hsHelper.inertia = info.Level < 6 ? 10 : 15 - info.Level;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if(VectorToTarget is Vector2 target)
			{
				Projectile.spriteDirection = forwardDir * Math.Sign(target.X);
			}
			else if(Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = forwardDir;
			}
			else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = -forwardDir;
			}
			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}
	}

	public abstract class CombatPetHoverDasherMinion : CombatPetHoverShooterMinion
	{
		internal bool isDashing;
		private Vector2 dashVector;
		internal MotionBlurDrawer blurHelper;

		internal override int? FiredProjectileId => null;
		internal override int GetAttackFrames(ICombatPetLevelInfo info) => 90;

		public override void SetDefaults()
		{
			base.SetDefaults();
			DealsContactDamage = true;
			hsHelper.targetInnerRadius = 96;
			hsHelper.targetOuterRadius = 128;
			hsHelper.targetShootProximityRadius = 112;
			blurHelper = new MotionBlurDrawer(5);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(DoBumblingMovement)
			{
				BumblingMovement(vectorToTargetPosition);
				return;
			} 
			int framesSinceShoot = AnimationFrame - hsHelper.lastShootFrame;
			if(framesSinceShoot > 20 && framesSinceShoot % 15 < 10 && framesSinceShoot < attackFrames)
			{
				// dash at the target
				isDashing = true;
				if(dashVector == default)
				{
					dashVector = vectorToTargetPosition;
					dashVector.SafeNormalize();
					dashVector *= (hsHelper.travelSpeed + 1);
				}
				Projectile.velocity = dashVector;
			} else
			{
				dashVector = default;
				isDashing = false;
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			blurHelper.Update(Projectile.Center, isDashing);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// need to draw sprites manually for some reason
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1  ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			// motion blur
			if(isDashing)
			{
				blurHelper.DrawBlur(texture, lightColor, bounds, r);
			}
			// regular version
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r, bounds.GetOrigin(), 1, effects, 0);
			return false;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			isDashing = false;
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 target = base.IdleBehavior();
			DealsContactDamage = true;
			CrossMod.CombatPetComputeMinionStats(Projectile, leveledPetPlayer);
			return target;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
            if(Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = forwardDir;
			}
			else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = -forwardDir;
			}
		}
	}
}
