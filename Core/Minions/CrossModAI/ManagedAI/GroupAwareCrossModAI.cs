using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

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

		[CrossModProperty]
		internal int? FiredProjectileId { get; set; }


		[CrossModProperty]
		internal virtual int AttackFrames { get; set; }

		public GroupAwareCrossModAI(Projectile proj, int buffId, int? projId, bool isPet) : base(proj, buffId, isPet: isPet)
		{
			IsPet = true;
			CircleHelper = new HeadCirclingHelper(this);
			FiredProjectileId = projId;
		}

		internal override void ApplyPetDefaults()
		{
			base.ApplyPetDefaults();
			// go slower and smaller circle than minions since it's a cute little pet
			CircleHelper.idleBumbleFrames = 90;
			CircleHelper.idleBumbleRadius = 96;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();

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
			Projectile.tileCollide = false;
			if (vectorToIdlePosition.LengthSquared() > MaxSpeed * MaxSpeed)
			{
				vectorToIdlePosition.SafeNormalize();
				vectorToIdlePosition *= MaxSpeed;
				Projectile.velocity = (Projectile.velocity * (Inertia - 1) + vectorToIdlePosition) / Inertia;
			} else
			{
				Projectile.velocity = vectorToIdlePosition;
			}
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			// Always cache the projectile position, since we're always overriding it
			ProjCache.Cache(Projectile);
		}
	}
}
