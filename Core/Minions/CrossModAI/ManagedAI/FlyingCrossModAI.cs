using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI.ManagedAI
{
	/// <summary>
	/// Class that implements the basic flying AIs used by most combat pets and vanilla 
	/// clone minions. Roughly equivalent to HoverShooterMinion in the regular class hierarchy.
	/// </summary>
	internal class FlyingCrossModAI : GroupAwareCrossModAI, ICrossModSimpleMinion
	{

		internal HoverShooterHelper HsHelper { get; set; } = new();

		internal int FramesSinceLastHit { get; set; }

		private int CooldownAfterHitFrames => 144 / (int)MaxSpeed;


		public FlyingCrossModAI(Projectile proj, int buffId, int? projId, bool isPet, bool defaultIdle) : base(proj, buffId, projId, isPet, defaultIdle)
		{
			IdleLocationSets.circlingHead.Add(Projectile.type);
			HsHelper = new HoverShooterHelper(this, FiredProjectileId)
			{
				ExtraAttackConditionsMet = Behavior.IsMyTurn,
				ModifyTargetVector = ModifyTargetVector,
				travelSpeed = MaxSpeed,
				inertia = Inertia
			};
		}

		internal override void ApplyPetDefaults()
		{
			base.ApplyPetDefaults();
			HsHelper.targetInnerRadius = 96;
			HsHelper.targetOuterRadius = 128;
			HsHelper.targetShootProximityRadius = 96;
		}

		public override Vector2 IdleBehavior()
		{
			// This is a bit clunky, but need to keep HoverShooterHelper in sync with cross mod properties
			HsHelper.projectileVelocity = MaxSpeed + 3;
			HsHelper.attackFrames = AttackFrames;
			HsHelper.travelSpeed = MaxSpeed;
			HsHelper.inertia = Inertia;
			return base.IdleBehavior();
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(FiredProjectileId != null)
			{
				HsHelper.TargetedMovement(vectorToTargetPosition);
				// suggest to the minion that it should face towards the enemy while shooting
				ProjCache.Cache(Projectile);
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= 4;
				Projectile.velocity = vectorToTargetPosition;
			} else
			{
				BumblingMovement(vectorToTargetPosition);
			}
		}


		public void OnHitTarget(NPC target)
		{
			FramesSinceLastHit = 0;
		}


		internal void BumblingMovement(Vector2 vectorToTargetPosition)
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
				Projectile.velocity *= Math.Min(0.75f * MaxSpeed, 10); // kick it away from enemies that it's just hit
			}
		}

		internal void ModifyTargetVector(ref Vector2 target)
		{
			Behavior.DistanceFromGroup(ref target);
		}
	}
}
