using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.IllusionistArmor
{
	public abstract class BaseIllusionistRobe : ModItem
	{
		public static readonly int MinionDamageIncrease = 4;
		public static readonly int MaxMinionIncrease = 1;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinionDamageIncrease, MaxMinionIncrease);
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
			player.maxMinions += MaxMinionIncrease;
			player.GetDamage<SummonDamageClass>() += MinionDamageIncrease / 100f;
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