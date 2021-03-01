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
			item.width = 30;
			item.height = 32;
			item.accessory = true;
			item.value = Item.sellPrice(silver: 50);
			item.rare = ItemRarityID.Green;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<MinionPathfindingPlayer>().PassivePathfindingRange = 20 * 16;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.SetResult(this);
			recipe.AddIngredient(ItemType<GraniteSpark>(), 15);
			recipe.AddIngredient(ItemID.Granite, 10);
			recipe.AddIngredient(ItemID.Compass, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.AddRecipe();
		}
	}
}
