using AmuletOfManyMinions.Items.Accessories;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor
{
	[AutoloadEquip(EquipType.Body)]
	public class ForagerBreastplate : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Mildew Breastplate");
			Tooltip.SetDefault(""
				+ "Increased minion knockback by 1");
		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 18;
			item.value = Item.sellPrice(silver: 2, copper: 50);
			item.rare = ItemRarityID.White;
			item.defense = 3;
		}

		public override void UpdateEquip(Player player)
		{
			player.minionKB += 1;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Daybloom, 1);
			recipe.AddIngredient(ItemID.Mushroom, 2);
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 20);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}