using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses
{
	public abstract class CombatPetGroundedMeleeMinion : SimpleGroundBasedMinion 
	{

		internal LeveledCombatPetModPlayer leveledPetPlayer;
		internal Dictionary<GroundAnimationState, (int, int?)> frameInfo;

		internal virtual float DamageMult => 1f;

		internal int forwardDir = 1;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			searchDistance = 700;
			maxJumpVelocity = 12;
		}

		public override Vector2 IdleBehavior()
		{
			leveledPetPlayer = player.GetModPlayer<LeveledCombatPetModPlayer>();
			searchDistance = leveledPetPlayer.PetLevelInfo.BaseSearchRange;
			SetOriginalDamage((int)(DamageMult * leveledPetPlayer.PetDamage));
			int petLevel = leveledPetPlayer.PetLevel;
			idleInertia = petLevel < 4 ? 15 : 18 - petLevel;
			return base.IdleBehavior();
		}

		protected void ConfigureDrawBox(int width, int height, int xOffset, int yOffset, int forwardDir = 1)
		{
			CombatPetConvenienceMethods.ConfigureDrawBox(this, width, height, xOffset, yOffset);
			this.forwardDir = forwardDir;
		}

		protected void ConfigureFrames(int total, (int, int) idle, (int, int) walking, (int, int) jumping, (int, int) flying)
		{
			// this should really go into SetStaticDefaults, but we're trying to condense
			// things as much as possible
			Main.projFrames[Projectile.type] = total;
			frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
			{
				[GroundAnimationState.STANDING] = idle,
				[GroundAnimationState.WALKING] = walking,
				[GroundAnimationState.JUMPING] = jumping,
				[GroundAnimationState.FLYING] = flying,
			};
		}
		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -3 * Projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = (int)leveledPetPlayer.PetLevelInfo.BaseSpeed;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			if (animationFrame - lastHitFrame > Math.Max(6, 90 / xMaxSpeed))
			{
				Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
			else
			{
				Projectile.velocity.X = Math.Sign(Projectile.velocity.X) * xMaxSpeed * 0.75f;
			}
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
            if (Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = forwardDir;
			} else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = -forwardDir;
			}
		}

		protected void DoSimpleFlyingDust()
		{
			if(gHelper.isFlying)
			{
				Projectile.rotation = Projectile.velocity.X * 0.05f;
				int idx = Dust.NewDust(Projectile.Bottom, 8, 8, 16, -Projectile.velocity.X / 2, -Projectile.velocity.Y / 2);
				Main.dust[idx].alpha = 112;
				Main.dust[idx].scale = .9f;
			} else
			{
				Projectile.rotation = 0;
			}
		}
	}

	public abstract class CombatPetGroundedRangedMinion : CombatPetGroundedMeleeMinion
	{
		internal int lastFiredFrame = 0;
		// don't get too close
		internal int preferredDistanceFromTarget = 128;

		internal int launchVelocity = 12;

		internal virtual bool ShouldDoShootingMovement => true;

		internal virtual int GetAttackFrames(CombatPetLevelInfo info) => Math.Max(30, 60 - 6 * info.Level);

		internal virtual float ModifyProjectileDamage(CombatPetLevelInfo info) => 1f;

		internal virtual int? ProjId => null;
		internal virtual Vector2 LaunchPos => Projectile.Center;
		public virtual void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			if(!(ProjId is int projId)) { return; }
			Projectile.NewProjectile(
				LaunchPos,
				VaryLaunchVelocity(launchVector),
				projId,
				(int)(ModifyProjectileDamage(leveledPetPlayer.PetLevelInfo) * Projectile.damage),
				Projectile.knockBack,
				player.whoAmI,
				ai0: ai0 ?? Projectile.whoAmI);
		}

		public override void AfterMoving()
		{
			Projectile.friendly &= !ShouldDoShootingMovement;
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 target = base.IdleBehavior();
			attackFrames = GetAttackFrames(leveledPetPlayer.PetLevelInfo);
			return target;
		}
		protected override void DoGroundedMovement(Vector2 vector)
		{
			if(!ShouldDoShootingMovement)
			{
				base.DoGroundedMovement(vector);
				return;
			}
			if (vector.Y < -3 * Projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = (int)leveledPetPlayer.PetLevelInfo.BaseSpeed;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			// only change speed if the target is a decent distance away
			if (Math.Abs(vector.X) < 4 && targetNPCIndex is int idx && Math.Abs(Main.npc[idx].velocity.X) < 7)
			{
				Projectile.velocity.X = Main.npc[idx].velocity.X;
			}
			else
			{
				Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(!ShouldDoShootingMovement)
			{
				base.TargetedMovement(vectorToTargetPosition);
				return;
			}
			bool inLaunchRange = 
				Math.Abs(vectorToTargetPosition.X) < 4 * preferredDistanceFromTarget &&
				Math.Abs(vectorToTargetPosition.Y) < 4 * preferredDistanceFromTarget;
			if (player.whoAmI == Main.myPlayer && inLaunchRange && animationFrame - lastFiredFrame >= attackFrames)
			{
				lastFiredFrame = animationFrame;
				Vector2 launchVector = vectorToTargetPosition;
				// todo lead shot
				launchVector.SafeNormalize();
				launchVector *= launchVelocity;
				LaunchProjectile(launchVector);
			}

			// don't move if we're in range-ish of the target
			if (Math.Abs(vectorToTargetPosition.X) < 1.25f * preferredDistanceFromTarget && 
				Math.Abs(vectorToTargetPosition.X) > 0.5f * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X = 0;
			} else if (Math.Abs(vectorToTargetPosition.X) < 0.5f * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X -= Math.Sign(vectorToTargetPosition.X) * 0.75f * preferredDistanceFromTarget;
			}

			if(Math.Abs(vectorToTargetPosition.Y) < 1.25f * preferredDistanceFromTarget && 
				Math.Abs(vectorToTargetPosition.Y) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.Y = 0;
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate();
			if (vectorToTarget is Vector2 target && Math.Abs(target.X) < 1.5 * preferredDistanceFromTarget)
			{
				Projectile.spriteDirection = forwardDir * Math.Sign(target.X);
			} else if (Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = forwardDir;
			} else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = -forwardDir;
			}
		}

	}
}
