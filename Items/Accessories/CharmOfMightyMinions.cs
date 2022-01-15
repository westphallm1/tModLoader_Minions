using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Accessories
{
	[AutoloadEquip(EquipType.Neck)]
	class CharmOfMightyMinions : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Greatly increases minion damage,\n" +
				"but reduces max number of minions by 1,\n" +
				"and reduces minion variety bonus by 2%");
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
			player.minionDamage += 0.25f;
			player.maxMinions -= 1;
			player.GetModPlayer<MinionSpawningItemPlayer>().minionVarietyDamageBonus -= 0.02f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.DarkShard, 1);
			recipe.AddIngredient(ItemID.SoulofLight, 8);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

	}
}
