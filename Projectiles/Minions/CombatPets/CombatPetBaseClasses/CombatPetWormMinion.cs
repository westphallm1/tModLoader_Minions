using AmuletOfManyMinions.Projectiles.Minions.BoneSerpent;
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
	// class hierarchy was a mistake
	public abstract class CombatPetGroundedWormMinion : GroundTravellingWormMinion
	{
		public override int CounterType => -1;
		internal LeveledCombatPetModPlayer leveledPetPlayer;
		// very strong in pre-HM due to wall phasing, nerf a bit there
		internal bool IsPreHM => (leveledPetPlayer?.PetLevel ?? 0) <= 3;
		internal virtual float DamageMult =>  IsPreHM ? 0.95f : 1f;

		internal int MaxHits => 4;
		internal int hitCount;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
			Projectile.tileCollide = false;
			attackThroughWalls = true;
		}

		public override Vector2 IdleBehavior()
		{
			leveledPetPlayer = player.GetModPlayer<LeveledCombatPetModPlayer>();
			maxFramesInAir = 50 + 8 * leveledPetPlayer.PetLevel;
			Vector2 target = base.IdleBehavior();
			if(IsPreHM)
			{
				Projectile.localNPCHitCooldown = 25;
			}
			return target;
		}

		// don't grow
		protected override int EmpowerCount => 1;
		protected override int GetSegmentCount() => 6;

		protected override int ComputeDamage() => (int)(DamageMult * leveledPetPlayer.PetDamage);

		protected override float ComputeSearchDistance() => leveledPetPlayer.PetLevelInfo.BaseSearchRange;

		protected override float ComputeInertia() => Math.Max(10, 28 - 2 * leveledPetPlayer.PetLevel);

		protected override float ComputeTargetedSpeed() => leveledPetPlayer.PetLevelInfo.BaseSpeed + (IsPreHM ? 0 : 2);

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}

		public override void OnHitTarget(NPC target)
		{
			base.OnHitTarget(target);
			hitCount++;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			if(vectorToIdlePosition.LengthSquared() < 128 * 128)
			{
				hitCount = 0;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// require that the worm returns to owner between attacks
			if(IsPreHM && hitCount >= MaxHits)
			{
				IdleMovement(vectorToIdle);
			} else
			{
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(IsPreHM && hitCount >= MaxHits)
			{
				return false;
			}
			return base.Colliding(projHitbox, targetHitbox);
		}
	}

	public abstract class CombatPetWormMinion : WormMinion
	{
		public override int CounterType => -1;
		internal LeveledCombatPetModPlayer leveledPetPlayer;
		internal virtual float DamageMult => 1f;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
			Projectile.tileCollide = false;
			attackThroughWalls = true;
		}

		public override Vector2 IdleBehavior()
		{
			leveledPetPlayer = player.GetModPlayer<LeveledCombatPetModPlayer>();
			return base.IdleBehavior();
		}

		// don't grow
		protected override int EmpowerCount => 1;
		protected override int GetSegmentCount() => 6;

		protected override int ComputeDamage() => (int)(DamageMult * leveledPetPlayer.PetDamage);

		protected override float ComputeSearchDistance() => leveledPetPlayer.PetLevelInfo.BaseSearchRange;

		protected override float ComputeInertia() => Math.Max(10, 18 - leveledPetPlayer.PetLevel);

		protected override float ComputeTargetedSpeed() => leveledPetPlayer.PetLevelInfo.BaseSpeed + 2;

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}
	}

}
