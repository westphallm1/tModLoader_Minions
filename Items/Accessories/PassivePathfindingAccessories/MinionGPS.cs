using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.PassivePathfindingAccessories
{
	class MinionGPS : ModItem
	{

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("GPS of Minion Guidance");
			Tooltip.SetDefault(
				"Allows your minions to automatically attack around corners in a 30 tile radius.");
		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 32;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 2);
			item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<MinionPathfindingPlayer>().PassivePathfindingRange = 30 * 16;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.SetResult(this);
			recipe.AddIngredient(ItemType<MinionCompass>(), 1);
			recipe.AddIngredient(ItemID.CrystalShard, 10);
			recipe.AddIngredient(ItemID.SoulofLight, 8);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.AddRecipe();
		}
	}
}
