using AmuletOfManyMinions.Core.BackportUtils;
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
	abstract class CombatPetEmblem : BackportModItem
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

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			int maxCombatPets = CombatPetLevelTable.PetLevelTable[PetLevel].MaxPets;
			if(ServerConfig.Instance.AllowMultipleCombatPets &&  maxCombatPets > 1)
			{
			tooltips.Add(new TooltipLine(mod, "MaxCombatPets", 
				"In addition, you can have use up to " + maxCombatPets + " of your minion slots on combat pets." )
			{
				overrideColor = Color.LimeGreen
			});
			}
		}

		public override void SetDefaults()
		{
			// These below are needed for a minion weapon
			Item.noMelee = true;
			Item.summon = true;
			Item.shoot = ProjectileID.WoodenArrowFriendly; // don't actually shoot anything
			Item.damage = CombatPetLevelTable.PetLevelTable[PetLevel].BaseDamage;
			Item.knockBack = 1f; // make nonzero to allow more modifiers
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
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void AddRecipes() =>
			CreateRecipe(1).AddIngredient(ItemID.GoldBar, 12).AddTile(TileID.Anvils).Register();
	}

	class PlatinumCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 1;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Platinum Combat Pet Emblem");
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void AddRecipes() =>
			CreateRecipe(1).AddIngredient(ItemID.PlatinumBar, 12).AddTile(TileID.Anvils).Register();
	}

	class CorruptCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 2;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Corrupt Combat Pet Emblem");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() =>
			CreateRecipe(1).AddIngredient(ItemID.DemoniteBar, 12).AddTile(TileID.Anvils).Register();
	}

	class CrimsonCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 2;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crimson Combat Pet Emblem");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() =>
			CreateRecipe(1).AddIngredient(ItemID.CrimtaneBar, 12).AddTile(TileID.Anvils).Register();
	}

	class SkeletalCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 3;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Skeletal Combat Pet Emblem");
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.Green;
		}

		public override void AddRecipes() =>
			CreateRecipe(1).AddIngredient(ItemID.Bone, 50).AddTile(TileID.Anvils).Register();
	}

	class SoulfulCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 4;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Soulful Combat Pet Emblem");
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.SoulofLight, 8)
			.AddIngredient(ItemID.SoulofNight, 8)
			.AddTile(TileID.Anvils).Register();
	}


	class HallowedCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 5;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Hallowed Combat Pet Emblem");
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Pink;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.HallowedBar, 12)
			.AddIngredient(ItemID.SoulofSight, 1)
			.AddIngredient(ItemID.SoulofMight, 1)
			.AddIngredient(ItemID.SoulofFright, 1)
			.AddTile(TileID.MythrilAnvil).Register();
	}

	class SpectreCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 6;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spectre Combat Pet Emblem");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 11);
			Item.rare = ItemRarityID.Yellow;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.SpectreBar, 12)
			.AddTile(TileID.MythrilAnvil).Register();
	}

	class StardustCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 7;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Stardust Combat Pet Emblem");
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Red;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.FragmentStardust, 12)
			.AddTile(TileID.LunarCraftingStation).Register();
	}

	class CelestialCombatPetEmblem : CombatPetEmblem
	{
		internal override int PetLevel => 8;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Celestial Combat Pet Emblem");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 16);
			Item.rare = ItemRarityID.Red;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.LunarBar, 12)
			.AddIngredient(ModContent.ItemType<StardustCombatPetEmblem>(), 1)
			.AddTile(TileID.LunarCraftingStation).Register();
	}
}
