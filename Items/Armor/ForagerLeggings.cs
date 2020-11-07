using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor
{
	[AutoloadEquip(EquipType.Legs)]
	public class ForagerLeggings : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Mildew Leggings");
			Tooltip.SetDefault("");
		}

		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.value = Item.sellPrice(silver: 2);
			item.rare = ItemRarityID.White;
			item.defense = 2;
		}

		public override void UpdateEquip(Player player)
		{
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Moonglow, 1);
			recipe.AddIngredient(ItemID.Wood, 16);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}