using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.PassivePathfindingAccessories
{
	class MinionGPS : ModItem
	{
		public static readonly int PathfindingRange = 30;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(PathfindingRange);

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
			player.GetModPlayer<MinionPathfindingPlayer>().PassivePathfindingRange = PathfindingRange * 16;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemType<MinionCompass>(), 1).AddIngredient(ItemID.CrystalShard, 10).AddIngredient(ItemID.SoulofLight, 8).AddTile(TileID.TinkerersWorkbench).Register();
		}
	}
}
