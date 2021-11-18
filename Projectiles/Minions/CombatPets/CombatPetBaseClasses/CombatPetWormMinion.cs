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
			maxFramesInAir = 50 + 8 * leveledPetPlayer.PetLevel;
			return base.IdleBehavior();
		}

		// don't grow
		protected override int EmpowerCount => 1;
		protected override int GetSegmentCount() => 6;

		protected override int ComputeDamage() => (int)(DamageMult * leveledPetPlayer.PetDamage);

		protected override float ComputeSearchDistance() => leveledPetPlayer.PetLevelInfo.BaseSearchRange;

		protected override float ComputeInertia() => Math.Max(12, 22 - leveledPetPlayer.PetLevel);

		protected override float ComputeTargetedSpeed() => leveledPetPlayer.PetLevelInfo.BaseSpeed + 2;

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
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

		protected override float ComputeInertia() => Math.Max(12, 22 - leveledPetPlayer.PetLevel);

		protected override float ComputeTargetedSpeed() => leveledPetPlayer.PetLevelInfo.BaseSpeed + 2;

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}
	}

}
