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
			Tooltip.SetDefault("10% increased movement speed");
		}

		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.value = Item.sellPrice(silver: 2);
			item.rare = ItemRarityID.Orange;
			item.defense = 6;
		}

		public override void UpdateEquip(Player player)
		{
			player.moveSpeed += 0.1f;
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	public class IllusionistCorruptLeggings : BaseIllusionistLeggings
	{
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.ShadowScale, 15);
			recipe.AddIngredient(ItemID.Bone, 30);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

	}

	[AutoloadEquip(EquipType.Legs)]
	public class IllusionistCrimsonLeggings : BaseIllusionistLeggings
	{
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.TissueSample, 15);
			recipe.AddIngredient(ItemID.Bone, 30);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

	}

}