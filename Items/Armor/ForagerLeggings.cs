using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using System.ComponentModel;

namespace AmuletOfManyMinions.Items.Armor
{
	[AutoloadEquip(EquipType.Legs)]
	public class ForagerLeggings : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Mildew Leggings");
			Tooltip.SetDefault(
				"3% increased minion damage\n"
				+ "10% increased movement speed");
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
			player.minionDamage += 0.03f;
			player.moveSpeed += 0.05f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Moonglow, 1);
			recipe.AddIngredient(ItemID.Mushroom, 2);
			recipe.AddIngredient(ItemID.Wood, 16);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}