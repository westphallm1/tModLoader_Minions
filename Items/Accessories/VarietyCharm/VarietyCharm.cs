using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Accessories.VarietyCharm
{
	class VarietyCharm : ModItem
	{
		public static readonly int MinionVarietyBonus = 1;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinionVarietyBonus);
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
			player.GetModPlayer<MinionSpawningItemPlayer>().minionVarietyDamageBonus += MinionVarietyBonus / 100f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Ruby, 3).AddIngredient(ItemID.Topaz, 3).AddIngredient(ItemID.Emerald, 3).AddIngredient(ItemID.Sapphire, 3).AddIngredient(ItemID.Amethyst, 3).AddTile(TileID.Anvils).Register();
		}

	}
}
