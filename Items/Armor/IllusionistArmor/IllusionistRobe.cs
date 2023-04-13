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
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 2, copper: 50);
			Item.rare = ItemRarityID.Orange;
			Item.defense = 7;
		}

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