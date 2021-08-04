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
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += 0.25f;
			player.maxMinions -= 1;
			player.GetModPlayer<MinionSpawningItemPlayer>().minionVarietyDamageBonus -= 0.02f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.DarkShard, 1).AddIngredient(ItemID.SoulofLight, 8).AddTile(TileID.Anvils).Register();
		}

	}
}
