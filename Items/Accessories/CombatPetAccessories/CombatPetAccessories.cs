using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Accessories.CombatPetAccessories
{
	class CombatPetStylishTeamworkBow : ModItem
	{
		public static readonly int MaxPetIncrease = 1;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxPetIncrease);

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<LeveledCombatPetModPlayer>().ExtraPetSlots += MaxPetIncrease;
		}
	}

	class CombatPetMightyTeamworkBow : ModItem
	{
		public static readonly int MaxPetIncrease = 1;
		public static readonly int MaxMinionDecrease = 1;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxPetIncrease, MaxMinionDecrease);
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += 0.2f;
			player.maxMinions = Math.Max(0, player.maxMinions - MaxMinionDecrease);
			// Reducing max minions by one also decreases max combat pets by one,
			// So increase max combat pets by 2 for a total increase of 1 combat pet (a bit confusing)
			player.GetModPlayer<LeveledCombatPetModPlayer>().ExtraPetSlots += MaxPetIncrease + MaxMinionDecrease;
			player.GetModPlayer<LeveledCombatPetModPlayer>().PetSpeedBonus += 2;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<CharmOfMightyMinions>(), 1)
			.AddRecipeGroup(AoMMSystem.CombatPetChewToyRecipeGroup)
			.AddIngredient(ModContent.ItemType<CombatPetStylishTeamworkBow>(), 1)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
	class CombatPetSpookyTeamworkBow : ModItem
	{
		public static readonly int MaxPetIncrease = 2;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxPetIncrease);
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Yellow;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += 0.2f;
			player.GetModPlayer<LeveledCombatPetModPlayer>().PetSpeedBonus += 2;
			player.GetModPlayer<LeveledCombatPetModPlayer>().ExtraPetSlots += MaxPetIncrease;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.NecromanticScroll, 1)
			.AddIngredient(ModContent.ItemType<CombatPetMightyTeamworkBow>(), 1)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}

	abstract class CombatPetChewToy: ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<LeveledCombatPetModPlayer>().PetSpeedBonus += 2;
		}
	}
	class CombatPetChaoticChewToy : CombatPetChewToy
	{
		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.DemoniteBar, 12)
			.AddIngredient(ItemID.ShadowScale, 6)
			.AddTile(TileID.Anvils)
			.Register();
	}
	class CombatPetCrimsonChewToy : CombatPetChewToy
	{
		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.CrimtaneBar, 12)
			.AddIngredient(ItemID.TissueSample, 6)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
