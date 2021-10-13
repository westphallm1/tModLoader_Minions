using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses
{
	public abstract class CombatPetGroundedMeleeMinion : SimpleGroundBasedMinion 
	{

		internal LeveledCombatPetModPlayer leveledPetPlayer;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
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
			Projectile.originalDamage = leveledPetPlayer.PetDamage;
			return base.IdleBehavior();
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
	}

	public abstract class CombatPetGroundedRangedMinion : CombatPetGroundedMeleeMinion
	{
		internal int lastFiredFrame = 0;
		// don't get too close
		internal int preferredDistanceFromTarget = 128;

		internal virtual int GetAttackFrames(CombatPetLevelInfo info) => Math.Max(30, 60 - 6 * info.Level);

		public abstract void LaunchProjectile();

		public override void AfterMoving()
		{
			Projectile.friendly = false;
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 target = base.IdleBehavior();
			attackFrames = GetAttackFrames(leveledPetPlayer.PetLevelInfo);
			return target;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{

			if (Math.Abs(vectorToTargetPosition.X) < 4 * preferredDistanceFromTarget &&
				Math.Abs(vectorToTargetPosition.Y) < 4 * preferredDistanceFromTarget &&
				animationFrame - lastFiredFrame >= attackFrames)
			{
				LaunchProjectile();
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
	}
}
