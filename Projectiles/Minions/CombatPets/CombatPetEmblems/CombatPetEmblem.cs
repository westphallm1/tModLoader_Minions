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
}
