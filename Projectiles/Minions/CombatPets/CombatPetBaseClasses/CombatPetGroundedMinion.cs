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
			searchDistance = 700;
		}

		public override Vector2 IdleBehavior()
		{
			leveledPetPlayer = player.GetModPlayer<LeveledCombatPetModPlayer>();
			searchDistance = leveledPetPlayer.PetLevelInfo.BaseSearchRange;
			Projectile.originalDamage = (int)(DamageMult * leveledPetPlayer.PetDamage);
			int petLevel = leveledPetPlayer.PetLevel;
			idleInertia = petLevel < 4 ? 15 : 18 - petLevel;
			CrossMod.CombatPetComputeMinionStats(Projectile, leveledPetPlayer);
			return base.IdleBehavior();
		}

		protected void ConfigureDrawBox(int width, int height, int xOffset, int yOffset, int forwardDir = 1)
		{
			CombatPetConvenienceMethods.ConfigureDrawBox(this, width, height, xOffset, yOffset);
			this.forwardDir = forwardDir;
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{
			xMaxSpeed = (int)leveledPetPlayer.PetLevelInfo.BaseSpeed;
			DoDefaultGroundedMovement(vector);
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

	}

	public abstract class CombatPetGroundedRangedMinion : CombatPetGroundedMeleeMinion
	{
		internal int lastFiredFrame = 0;
		// don't get too close
		internal int preferredDistanceFromTarget = 128;

		internal int launchVelocity = 12;

		internal virtual bool ShouldDoShootingMovement => true;

		internal virtual int GetAttackFrames(ICombatPetLevelInfo info) => Math.Max(30, 60 - 6 * info.Level);

		internal virtual float ModifyProjectileDamage(ICombatPetLevelInfo info) => 1f;

		internal virtual int? ProjId => null;
		internal virtual Vector2 LaunchPos => Projectile.Center;
		public virtual void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			if(ProjId is not int projId) { return; }
			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),
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
			CrossMod.CombatPetComputeMinionStats(Projectile, leveledPetPlayer);
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
				// lead shot a little bit
				if(targetNPCIndex is int idx && Main.npc[idx] is NPC target)
				{
					launchVector += target.velocity * 0.167f;
				}
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
