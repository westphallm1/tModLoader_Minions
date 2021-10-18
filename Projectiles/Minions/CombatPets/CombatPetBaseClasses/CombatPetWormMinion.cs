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

		protected override int ComputeDamage() => leveledPetPlayer.PetDamage;

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
		private LeveledCombatPetModPlayer leveledPetPlayer;
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

		protected override int ComputeDamage() => leveledPetPlayer.PetDamage;

		protected override float ComputeSearchDistance() => leveledPetPlayer.PetLevelInfo.BaseSearchRange;

		protected override float ComputeInertia() => Math.Max(12, 22 - leveledPetPlayer.PetLevel);

		protected override float ComputeTargetedSpeed() => leveledPetPlayer.PetLevelInfo.BaseSpeed + 2;

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}
	}

}
