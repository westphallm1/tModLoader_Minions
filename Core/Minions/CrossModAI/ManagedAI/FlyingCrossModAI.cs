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

		// This is a bit clunky, but need to keep HoverShooterHelper in sync with cross mod properties
		public override int MaxSpeed 
		{
			get => base.MaxSpeed; 
			internal set { base.MaxSpeed = value; HsHelper.travelSpeed = value; }
		}
		public override int Inertia 
		{ 
			get => base.Inertia;
			internal set { base.Inertia = value; HsHelper.inertia = value; }
		}
		internal override int AttackFrames 
		{ 
			get => base.AttackFrames; 
			set { base.AttackFrames = value; HsHelper.attackFrames = value; }
		}

		public FlyingCrossModAI(Projectile proj, int buffId, int? projId, bool isPet) : base(proj, buffId, projId, isPet)
		{
			IdleLocationSets.circlingHead.Add(Projectile.type);
			HsHelper = new HoverShooterHelper(this, FiredProjectileId)
			{
				AfterFiringProjectile = AfterFiringProjectile,
				ExtraAttackConditionsMet = Behavior.IsMyTurn,
				ModifyTargetVector = ModifyTargetVector,
				travelSpeed = MaxSpeed,
				inertia = Inertia
			};
			if(IsPet) { ApplyPetDefaults(); }
		}

		internal override void ApplyPetDefaults()
		{
			base.ApplyPetDefaults();
			HsHelper.targetInnerRadius = 96;
			HsHelper.targetOuterRadius = 128;
			HsHelper.targetShootProximityRadius = 96;
		}

		internal override void UpdatePetState()
		{
			base.UpdatePetState();
			var leveledPetPlayer = Player.GetModPlayer<LeveledCombatPetModPlayer>();
			var info = CombatPetLevelTable.PetLevelTable[leveledPetPlayer.PetLevel];
			HsHelper.projectileVelocity = MaxSpeed + 3;
			HsHelper.attackFrames = Math.Max(30, 60 - 6 * info.Level);
			HsHelper.travelSpeed = MaxSpeed;
			HsHelper.inertia = Inertia;
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


		internal virtual void AfterFiringProjectile()
		{
			// TODO make this customizeable
			SoundEngine.PlaySound(SoundID.Item43);
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
