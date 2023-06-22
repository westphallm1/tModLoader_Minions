using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.AridArmor
{
	[AutoloadEquip(EquipType.Legs)]
	public class AridLeggings : ModItem
	{
		public static readonly int MinionDamageIncrease = 8;
		public static readonly int SquireRangeIncrease = 1;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinionDamageIncrease, SquireRangeIncrease);
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.Green;
			Item.defense = 6;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += MinionDamageIncrease / 100f;
			player.GetModPlayer<SquireModPlayer>().SquireRangeFlatBonus += SquireRangeIncrease * 16f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.AncientCloth, 4).AddIngredient(ItemID.AntlionMandible, 4).AddIngredient(ItemID.FossilOre, 12).AddTile(TileID.Anvils).Register();
		}
	}
}