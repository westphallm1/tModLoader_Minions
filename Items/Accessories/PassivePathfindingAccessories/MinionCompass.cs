using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.PassivePathfindingAccessories
{
	class MinionCompass : ModItem
	{

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Compass of Minion Guidance");
			Tooltip.SetDefault(
				"Allows your minions to automatically attack around corners in a 16 tile radius.");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Green;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<MinionPathfindingPlayer>().PassivePathfindingRange = 18 * 16;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemType<GraniteSpark>(), 12).AddIngredient(ItemID.Granite, 10).AddIngredient(ItemID.Compass, 1).AddTile(TileID.Anvils).Register();
		}
	}
}
