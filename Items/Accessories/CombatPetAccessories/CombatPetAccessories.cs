using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Accessories.CombatPetAccessories
{
	class CombatPetStylishTeamworkBow : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Stylish Bow of Teamwork");
			Tooltip.SetDefault("Increases your max combat pets by 1");
		}

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
			player.GetModPlayer<LeveledCombatPetModPlayer>().ExtraPetSlots += 1;
		}
	}

	class CombatPetMightyTeamworkBow : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Mighty Bow of Teamwork");
			Tooltip.SetDefault(
				"Increases your max combat pets by 1 and increases minion damage\n" +
				"but decreases max minions");
		}

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
			player.maxMinions = Math.Max(0, player.maxMinions - 1);
			player.GetModPlayer<LeveledCombatPetModPlayer>().ExtraPetSlots += 1;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<CharmOfMightyMinions>(), 1)
			.AddIngredient(ModContent.ItemType<CombatPetStylishTeamworkBow>(), 1)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
	class CombatPetSpookyTeamworkBow : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Spooky Bow of Teamwork");
			Tooltip.SetDefault(
				"Increases your max combat pets by 2 and increases minion damage");
		}

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
			player.GetModPlayer<LeveledCombatPetModPlayer>().ExtraPetSlots += 2;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.NecromanticScroll, 1)
			.AddIngredient(ModContent.ItemType<CombatPetMightyTeamworkBow>(), 1)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
}
