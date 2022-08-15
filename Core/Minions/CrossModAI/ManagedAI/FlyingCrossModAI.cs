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
	/// Class that implements the basic flying AIs used by most combat pets and vanilla 
	/// clone minions. Roughly equivalent to HoverShooterMinion in the regular class hierarchy.
	/// </summary>
	internal class FlyingCrossModAI : GroupAwareCrossModAI, ICrossModSimpleMinion
	{
		internal int? FiredProjectileId { get; set; }

		internal HoverShooterHelper HsHelper { get; set; }

		internal bool IsPet { get; set; } = true;

		internal int FramesSinceLastHit { get; set; }

		private int CooldownAfterHitFrames => 144 / (int)MaxSpeed;

		public FlyingCrossModAI(Projectile proj, int buffId, int? projId) : base(proj, buffId)
		{
			FiredProjectileId = projId;
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

		private void ApplyPetDefaults()
		{
			Projectile.minion = true;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			HsHelper.targetInnerRadius = 96;
			HsHelper.targetOuterRadius = 128;
			HsHelper.targetShootProximityRadius = 96;
			// go slower and smaller circle than minions since it's a cute little pet
			CircleHelper.idleBumbleFrames = 90;
			CircleHelper.idleBumbleRadius = 96;
		}

		private void UpdatePetState()
		{
			var leveledPetPlayer = Player.GetModPlayer<LeveledCombatPetModPlayer>();
			var info = CombatPetLevelTable.PetLevelTable[leveledPetPlayer.PetLevel];
			SearchRange = info.BaseSearchRange;
			Inertia = info.Level < 6 ? 10 : 15 - info.Level;
			MaxSpeed = (int)info.BaseSpeed;
			Projectile.originalDamage = leveledPetPlayer.PetDamage;

			HsHelper.projectileVelocity = (int)(info.BaseSpeed + 3);
			HsHelper.attackFrames = Math.Max(30, 60 - 6 * info.Level);
			HsHelper.travelSpeed = MaxSpeed;
			HsHelper.inertia = Inertia;
		}

		public override Vector2 IdleBehavior()
		{
			if(IsPet) { UpdatePetState(); }
			return base.IdleBehavior();
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(FiredProjectileId != null)
			{
				HsHelper.TargetedMovement(vectorToTargetPosition);
			} else
			{
				BumblingMovement(vectorToTargetPosition);
			}
			CacheProjectileState();
			// Main.NewText($"{Projectile.friendly} {Projectile.minion} {Projectile.damage} {Projectile.originalDamage}");
		}


		internal virtual void AfterFiringProjectile()
		{
			//if(ShootSound.HasValue)
			//{
			//	SoundEngine.PlaySound(ShootSound.Value, Projectile.Center);
			//}
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

		public override void PostAI()
		{
			base.PostAI();
			if(IsPet && FiredProjectileId == null)
			{
				Projectile.minion = true;
				Projectile.friendly = true;
				Projectile.DamageType = DamageClass.Summon;
			}
		}
	}
}
