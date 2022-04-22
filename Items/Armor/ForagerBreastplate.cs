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
				+ "Increases minion damage by 1\n"
				+ "Increased minion knockback by 1");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 2, copper: 50);
			Item.rare = ItemRarityID.White;
			Item.defense = 3;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>().Flat += 1;
			player.GetKnockback<SummonDamageClass>().Base += 1;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Daybloom, 1).AddIngredient(ItemID.Mushroom, 2).AddRecipeGroup(RecipeGroupID.Wood, 20).AddTile(TileID.WorkBenches).Register();
		}
	}
}
