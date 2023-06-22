using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.AridArmor
{
	[AutoloadEquip(EquipType.Body)]
	public class AridBreastplate : ModItem
	{
		public static readonly int MinionDamageIncrease = 8;
		public static readonly int SquireAttackSpeedIncrease = 10;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinionDamageIncrease, SquireAttackSpeedIncrease);
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 2, copper: 50);
			Item.rare = ItemRarityID.Green;
			Item.defense = 6;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += MinionDamageIncrease / 100f;
			player.GetModPlayer<SquireModPlayer>().SquireAttackSpeedMultiplier *= (1f - SquireAttackSpeedIncrease/100f);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.AncientCloth, 5).AddIngredient(ItemID.AntlionMandible, 5).AddIngredient(ItemID.FossilOre, 15).AddTile(TileID.Anvils).Register();
		}
	}
}