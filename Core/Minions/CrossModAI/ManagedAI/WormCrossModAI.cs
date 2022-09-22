using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI.ManagedAI
{
	internal class WormCrossModAI : FlyingCrossModAI
	{
		internal override int CooldownAfterHitFrames => 16;
		public WormCrossModAI(Projectile proj, int buffId, int? projId, bool isPet, bool defaultIdle) : 
			base(proj, buffId, projId, isPet, defaultIdle)
		{
			Behavior.NoLOSPursuitTime = 180;
			Behavior.AttackThroughWalls = true;
		}

		internal override void BumblingMovement(Vector2 vectorToTargetPosition)
		{

			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= MaxSpeed;
			FramesSinceLastHit++;
			if (FramesSinceLastHit < CooldownAfterHitFrames && FramesSinceLastHit > CooldownAfterHitFrames / 2)
			{
				// start turning so we don't double directly back
				Vector2 turnVelocity = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X) / 8;
				turnVelocity *= Math.Sign(Projectile.velocity.X);
				Projectile.velocity += turnVelocity;
			}
			else if (FramesSinceLastHit++ > CooldownAfterHitFrames)
			{
				Projectile.velocity = (Projectile.velocity * (Inertia - 1) + vectorToTargetPosition) / Inertia;
			}
			else
			{
				Projectile.velocity.SafeNormalize();
				Projectile.velocity *= Math.Min(0.75f * MaxSpeed, 12); // kick it away from enemies that it's just hit
			}
		}

		internal override void UpdatePetState()
		{
			base.UpdatePetState();
			MaxSpeed += 2;
			Inertia += 3; // give higher inertia to exagerate turning around more
			if(!Main.hardMode)
			{
				// Worm pets tend to be very good in pre-hardmode, nerf them slightly
				Projectile.originalDamage = 3 * Projectile.originalDamage / 4;
			}
		}
	}
}
