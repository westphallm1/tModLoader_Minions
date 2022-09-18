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
	internal class SlimeCrossModAI : BaseGroundedCrossModAI
	{
		internal int PreferredTargetDist { get; set; } = 128;
		internal int LaunchVelocity { get; set; } = 12;
		internal int LastFiredFrame { get; set; }

		protected bool ShouldBounce => AlwaysBounce || Behavior.VectorToTarget != null || 
			Behavior.VectorToIdle.LengthSquared() > 32 * 32;

		internal virtual bool ShouldDoShootingMovement => FiredProjectileId != null;

		// Intended x velocity while jumping, restore to this value if we get stuck
		private float intendedX;

		internal bool AlwaysBounce { get; set; }

		public SlimeCrossModAI(Projectile proj, int buffId, int? projId, bool isPet) : base(proj, buffId, projId, isPet)
		{
		}

		public override bool DoPreStuckCheckGroundedMovement()
		{
			if (!GHelper.didJustLand)
			{
				Projectile.velocity.X = intendedX;
				// only path after landing
				return false;
			}
			return true;
		}

		public override bool CheckForStuckness()
		{
			return true;
		}

		public override void DoGroundedMovement(Vector2 vector)
		{
			if(!ShouldBounce)
			{
				// slide to a halt
				Projectile.velocity.X *= 0.75f;
				return;
			}
			// always jump "long" if we're far away from the enemy
			if (Math.Abs(vector.X) > StartFlyingDist && vector.Y < -32)
			{
				vector.Y = -32;
			}
			GHelper.DoJump(vector);
			int maxHorizontalSpeed = vector.Y < -64 ? MaxSpeed/2 : MaxSpeed;
			if(Behavior.TargetNPCIndex is int idx && vector.Length() < 64)
			{
				// go fast enough to hit the enemy while chasing them
				Vector2 targetVelocity = Main.npc[idx].velocity;
				Projectile.velocity.X = Math.Max(4, Math.Min(maxHorizontalSpeed, Math.Abs(targetVelocity.X) * 1.25f)) * Math.Sign(vector.X);
			} else
			{
				maxHorizontalSpeed = vector.Y < -64 ? 4 : 8;
				// try to match the player's speed while not chasing an enemy
				Projectile.velocity.X = Math.Max(1, Math.Min(maxHorizontalSpeed, Math.Abs(vector.X) / 16)) * Math.Sign(vector.X);
			}
			intendedX = Projectile.velocity.X;
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

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(!ShouldDoShootingMovement)
			{
				base.TargetedMovement(vectorToTargetPosition);
				return;
			}
			bool inLaunchRange = 
				Math.Abs(vectorToTargetPosition.X) < 4 * PreferredTargetDist &&
				Math.Abs(vectorToTargetPosition.Y) < 4 * PreferredTargetDist;
			if (Player.whoAmI == Main.myPlayer && inLaunchRange && Behavior.AnimationFrame - LastFiredFrame >= AttackFrames)
			{
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
			if (Math.Abs(vectorToTargetPosition.X) < 1.25f * PreferredTargetDist && 
				Math.Abs(vectorToTargetPosition.X) > 0.5f * PreferredTargetDist)
			{
				vectorToTargetPosition.X = 0;
			} else if (Math.Abs(vectorToTargetPosition.X) < 0.5f * PreferredTargetDist)
			{
				// TODO: For cross mod AI only (not normal), this has an issue with triggering the "get unstuck"
				// AI. As a quick fix, explicitly guard against attempting to move into an occupied block.
				Vector2 nextTarget = vectorToTargetPosition - Vector2.UnitX * Math.Sign(vectorToTargetPosition.X) * 0.75f * PreferredTargetDist;
				if(Collision.CanHitLine(Projectile.Center, 1, 1, Projectile.Center + nextTarget, 1, 1))
				{
					vectorToTargetPosition = nextTarget;
				}
			}

			if(Math.Abs(vectorToTargetPosition.Y) < 1.25f * PreferredTargetDist && 
				Math.Abs(vectorToTargetPosition.Y) > 0.5 * PreferredTargetDist)
			{
				vectorToTargetPosition.Y = 0;
			}
			base.TargetedMovement(vectorToTargetPosition);
		}
		public override void AfterMoving()
		{
			base.AfterMoving();
			Projectile.friendly &= !ShouldDoShootingMovement;
			// having a slightly positive velocity from constant gravity messes with the vanilla frame
			// determination
			// This occurs after the velocity cache, so it should be ignored for actual calculations
			if(Projectile.velocity.Y > 0.8f && Projectile.velocity.Y < 1f)
			{
				Projectile.velocity.Y = 0.8f;
			}
		}
	}
}
