using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI.ManagedAI
{
	internal class GroundedCrossModAI : BaseGroundedCrossModAI
	{
		internal int LastFiredFrame { get; set; }

		internal virtual bool ShouldDoShootingMovement => FiredProjectileId != null;

		public override bool IsInFiringRange => IsAttacking && Behavior.VectorToTarget is Vector2 target &&
				Math.Abs(target.X) < 4 * PreferredTargetDistance &&
				Math.Abs(target.Y) < 4 * PreferredTargetDistance &&
				Collision.CanHitLine(Projectile.Center, 1, 1, Projectile.Center + target, 1, 1);

		public GroundedCrossModAI(Projectile proj, int buffId, int? projId, bool isPet, bool defaultIdle) : 
			base(proj, buffId, projId, isPet, defaultIdle)
		{
		}

		public void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			if(FiredProjectileId is not int projId || projId <= 0) { return; }
			launchVector *= 1.15f;
			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				Behavior.VaryLaunchVelocity(launchVector),
				projId,
				Projectile.damage,
				Projectile.knockBack,
				Player.whoAmI,
				ai0: ai0 ?? 0);
		}

		public override void DoGroundedMovement(Vector2 vector)
		{
			if(!ShouldDoShootingMovement)
			{
				base.DoGroundedMovement(vector);
				return;
			}
			if (vector.Y < -3 * Projectile.height && Math.Abs(vector.X) < StartFlyingHeight)
			{
				GHelper.DoJump(vector);
			}
			if (Behavior.VectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = Player.velocity.X;
				return;
			}
			Behavior.DistanceFromGroup(ref vector);
			// only change speed if the target is a decent distance away
			if (Math.Abs(vector.X) < 4 && Behavior.TargetNPCIndex is int idx && Math.Abs(Main.npc[idx].velocity.X) < 7)
			{
				Projectile.velocity.X = Main.npc[idx].velocity.X;
			}
			else
			{
				Projectile.velocity.X = (Projectile.velocity.X * (Inertia - 1) + Math.Sign(vector.X) * MaxSpeed) / Inertia;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(!ShouldDoShootingMovement)
			{
				base.TargetedMovement(vectorToTargetPosition);
				return;
			}
			if (Player.whoAmI == Main.myPlayer && IsInFiringRange && Behavior.AnimationFrame - LastFiredFrame >= AttackFrames)
			{
				ShouldFireThisFrame = true;
				LastFiredFrame = Behavior.AnimationFrame;
				Vector2 launchVector = vectorToTargetPosition;
				// lead shot a little bit
				if(Behavior.TargetNPCIndex is int idx && Main.npc[idx] is NPC target)
				{
					launchVector += target.velocity * 0.167f;
				}
				launchVector.SafeNormalize();
				launchVector *= LaunchVelocity;
				LaunchProjectile(launchVector);
			}

			// don't move if we're in range-ish of the target
			if (Math.Abs(vectorToTargetPosition.X) < 1.25f * PreferredTargetDistance && 
				Math.Abs(vectorToTargetPosition.X) > 0.5f * PreferredTargetDistance)
			{
				vectorToTargetPosition.X = 0;
			} else if (Math.Abs(vectorToTargetPosition.X) < 0.5f * PreferredTargetDistance)
			{
				// TODO: For cross mod AI only (not normal), this has an issue with triggering the "get unstuck"
				// AI. As a quick fix, explicitly guard against attempting to move into an occupied block.
				Vector2 nextTarget = vectorToTargetPosition - Vector2.UnitX * Math.Sign(vectorToTargetPosition.X) * 0.75f * PreferredTargetDistance;
				if(Collision.CanHitLine(Projectile.Center, 1, 1, Projectile.Center + nextTarget, 1, 1))
				{
					vectorToTargetPosition = nextTarget;
				}
			}

			if(Math.Abs(vectorToTargetPosition.Y) < 1.25f * PreferredTargetDistance && 
				Math.Abs(vectorToTargetPosition.Y) > 0.5 * PreferredTargetDistance)
			{
				vectorToTargetPosition.Y = 0;
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override void AfterMoving()
		{
			Projectile.friendly &= !ShouldDoShootingMovement;
			base.AfterMoving();
			// having a slightly positive velocity from constant gravity messes with the vanilla frame
			// determination
			// This occurs after the velocity cache, so it should be ignored for actual calculations
			if(Projectile.velocity.Y == 0.5f && !IsIdlingNearPlayer)
			{
				Projectile.velocity.Y = 0;
			}
		}
	}
}
