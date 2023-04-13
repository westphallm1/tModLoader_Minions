using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.IllusionistArmor
{
	public abstract class BaseIllusionistLeggings : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.Orange;
			Item.defense = 6;
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
			CreateRecipe(1).AddIngredient(ItemID.ShadowScale, 15).AddIngredient(ItemID.Bone, 30).AddTile(TileID.Anvils).Register();
		}

	}

	[AutoloadEquip(EquipType.Legs)]
	public class IllusionistCrimsonLeggings : BaseIllusionistLeggings
	{
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.TissueSample, 15).AddIngredient(ItemID.Bone, 30).AddTile(TileID.Anvils).Register();
		}

	}

}