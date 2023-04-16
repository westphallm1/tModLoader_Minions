using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor
{
	[AutoloadEquip(EquipType.Legs)]
	public class ForagerLeggings : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.White;
			Item.defense = 2;
		}

		public override void UpdateEquip(Player player)
		{
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddRecipeGroup(RecipeGroupID.Wood, 16).AddTile(TileID.WorkBenches).Register();
		}
	}
}