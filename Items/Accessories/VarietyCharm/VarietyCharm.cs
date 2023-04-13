using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Accessories.VarietyCharm
{
	class VarietyCharm : ModItem
	{
		public override void SetStaticDefaults()
		{
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<MinionSpawningItemPlayer>().minionVarietyDamageBonus += .01f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Ruby, 3).AddIngredient(ItemID.Topaz, 3).AddIngredient(ItemID.Emerald, 3).AddIngredient(ItemID.Sapphire, 3).AddIngredient(ItemID.Amethyst, 3).AddTile(TileID.Anvils).Register();
		}

	}
}
