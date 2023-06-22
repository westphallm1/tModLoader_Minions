using AmuletOfManyMinions.Items.Accessories;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor
{
	[AutoloadEquip(EquipType.Body)]
	public class ForagerBreastplate : ModItem
	{
		public static readonly int MinionKnockbackIncrease = 1;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinionKnockbackIncrease);
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 2, copper: 50);
			Item.rare = ItemRarityID.White;
			Item.defense = 4;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetKnockback<SummonDamageClass>().Base += MinionKnockbackIncrease;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Daybloom, 1).AddIngredient(ItemID.Mushroom, 2).AddRecipeGroup(RecipeGroupID.Wood, 20).AddTile(TileID.WorkBenches).Register();
		}
	}
}
