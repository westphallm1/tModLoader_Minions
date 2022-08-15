using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI.ManagedAI
{
	/// <summary>
	/// Base class for the 'boilerplate' AIs used by most combat pets and vanilla 
	/// clone minions. Also suitable for applying a fully managed AI to a 
	/// cross-mod minion or pet. Roughly equivalent to HeadCirclingGroupAwareMinion
	/// in the regular class hierarchy.
	/// </summary>
	internal abstract class GroupAwareCrossModAI : BasicCrossModAI
	{
		internal HeadCirclingHelper CircleHelper { get; set; }

		public GroupAwareCrossModAI(Projectile proj, int buffId) : base(proj, buffId)
		{
			CircleHelper = new HeadCirclingHelper(this);
		}

		public override Vector2 IdleBehavior()
		{
			if(CircleHelper.IdleBumble && Player.velocity.Length() < 4)
			{
				return CircleHelper.BumblingHeadCircle();
			} else
			{
				return CircleHelper.DirectHeadCircle();
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// Always calculate an cache a 
			if (vectorToIdlePosition.LengthSquared() > MaxSpeed * MaxSpeed)
			{
				vectorToIdlePosition.SafeNormalize();
				vectorToIdlePosition *= MaxSpeed;
				Projectile.velocity = (Projectile.velocity * (Inertia - 1) + vectorToIdlePosition) / Inertia;
			} else
			{
				Projectile.velocity = vectorToIdlePosition;
			}
			CacheProjectileState();
		}

		public override void PostAI()
		{
			// Always uncache the state
			UncacheProjectileState();
		}
	}
}
