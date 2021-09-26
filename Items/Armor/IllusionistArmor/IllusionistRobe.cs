using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.IllusionistArmor
{
	public abstract class BaseIllusionistRobe : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Illusionist Robe");
			Tooltip.SetDefault("Increases your max number of minions by 1" +
							   "\nIncreases minion damage by 4%");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 2, copper: 50);
			Item.rare = ItemRarityID.Orange;
			Item.defense = 7;
		}

		//public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
		//{
		//	drawAltHair = true;
		//}

		public override void UpdateEquip(Player player)
		{
			player.maxMinions += 1;
			player.GetDamage<SummonDamageClass>() += 0.04f;
		}
	}

	[AutoloadEquip(EquipType.Body)]
	public class IllusionistCorruptRobe : BaseIllusionistRobe
	{
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.ShadowScale, 20).AddIngredient(ItemID.Bone, 35).AddTile(TileID.Anvils).Register();
		}
	}

	[AutoloadEquip(EquipType.Body)]
	public class IllusionistCrimsonRobe : BaseIllusionistRobe
	{
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.TissueSample, 20).AddIngredient(ItemID.Bone, 35).AddTile(TileID.Anvils).Register();
		}
	}
}