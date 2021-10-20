using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetEmblems
{
	abstract class CombatPetEmblem : ModItem
	{
		internal abstract int PetLevel { get; }


		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Tooltip.SetDefault(
				"An emblem that increases the power of your combat pets!\n" +
				"As long as this item is in your inventory, your combat pet will deal\n" +
				"additional damage, and will receive a bonus to movement speed and attack range.");
		}
		public override void SetDefaults()
		{
			// These below are needed for a minion weapon
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.shoot = ProjectileID.WoodenArrowFriendly; // don't actually shoot anything
			Item.damage = CombatPetLevelTable.PetLevelTable[PetLevel].BaseDamage;
		}

		public override bool CanUseItem(Player player)
		{
			return false;
		}
	}

	class GoldenCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 1;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Golden Combat Pet Emblem");
		}
	}

	class PlatinumCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 1;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Platinum Combat Pet Emblem");
		}
	}

	class CorruptCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 2;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Corrupt Combat Pet Emblem");
		}
	}

	class CrimsonCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 2;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crimson Combat Pet Emblem");
		}
	}

	class SkeletalCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 3;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Skeletal Combat Pet Emblem");
		}
	}

	class SoulfulCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 4;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Soulful Combat Pet Emblem");
		}
	}


	class HallowedCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 5;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spectre Combat Pet Emblem");
		}
	}

	class SpectreCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 6;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spectre Combat Pet Emblem");
		}
	}

	class StardustCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 7;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Stardust Combat Pet Emblem");
		}
	}

	class CelestialCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 8;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Stardust Combat Pet Emblem");
		}
	}
}
