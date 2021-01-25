using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Accessories
{
	[AutoloadEquip(EquipType.Neck)]
	class CharmOfManyMinions : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Increases max number of minions by 1\n" +
				"and increases minion variety bonus by 1%\n" +
				"but each minion deals slightly less damage");
		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 32;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 5);
			item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamageMult -= 0.1f;
			player.maxMinions += 1;
			player.GetModPlayer<MinionSpawningItemPlayer>().minionVarietyDamageBonus += 0.01f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.LightShard, 1);
			recipe.AddIngredient(ItemID.SoulofNight, 8);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

	}
}
