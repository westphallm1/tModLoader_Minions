using AmuletOfManyMinions.Projectiles.Minions.FlyingSword;
using AmuletOfManyMinions.Projectiles.Minions.NullHatchet;
using AmuletOfManyMinions.Projectiles.Minions.PossessedCopperSword;
using AmuletOfManyMinions.Projectiles.Minions.SpiritGun;
using AmuletOfManyMinions.Projectiles.Minions.VoidKnife;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories
{
	[AutoloadEquip(EquipType.Neck)]
	class AmuletOfManyMinions : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Increases your max number of minions by 2\n" +
				"Greatly improves minion damage,\n" +
				"and increases minion variety bonus by 2%");
		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 32;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 5);
			item.rare = ItemRarityID.Expert;
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamageMult += 0.2f;
			player.maxMinions += 2;
			player.GetModPlayer<MinionSpawningItemPlayer>().minionVarietyDamageBonus += 0.02f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<CopperSwordMinionItem>(), 1);
			recipe.AddIngredient(ItemType<SpiritGunMinionItem>(), 1);
			recipe.AddIngredient(ItemType<FlyingSwordMinionItem>(), 1);
			recipe.AddRecipeGroup("AmuletOfManyMinions:VoidDaggers");
			recipe.AddIngredient(ItemType<CharmOfManyMinions>(), 1);
			recipe.AddIngredient(ItemType<CharmOfMightyMinions>(), 1);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

	}
}
