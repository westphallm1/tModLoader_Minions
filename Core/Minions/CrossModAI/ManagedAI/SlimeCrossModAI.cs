using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI.ManagedAI
{
	internal class SlimeCrossModAI : BaseGroundedCrossModAI
	{
		protected bool ShouldBounce => Behavior.VectorToTarget != null || Behavior.VectorToIdle.LengthSquared() > 32 * 32;

		// Intended x velocity while jumping, restore to this value if we get stuck
		private float intendedX;

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
	}
}
