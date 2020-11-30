using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.IllusionistArmor
{
	public abstract class BaseIllusionistLeggings : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Illusionist Leggings");
			Tooltip.SetDefault("+10% Movement Speed");
		}

		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.value = Item.sellPrice(silver: 2);
			item.rare = ItemRarityID.White;
			item.defense = 6;
		}

		public override void UpdateEquip(Player player)
		{
			player.moveSpeed += 0.1f;
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

	[AutoloadEquip(EquipType.Legs)]
	public class IllusionistCorruptLeggings: BaseIllusionistLeggings
	{

	}

	[AutoloadEquip(EquipType.Legs)]
	public class IllusionistCrimsonLeggings: BaseIllusionistLeggings
	{

	}

}