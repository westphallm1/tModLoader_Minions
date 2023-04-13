using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.AridArmor
{
	[AutoloadEquip(EquipType.Body)]
	public class AridBreastplate : ModItem
	{
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
			player.GetDamage<SummonDamageClass>() += 0.08f;
			player.GetModPlayer<SquireModPlayer>().SquireAttackSpeedMultiplier *= 0.9f;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.AncientCloth, 5).AddIngredient(ItemID.AntlionMandible, 5).AddIngredient(ItemID.FossilOre, 15).AddTile(TileID.Anvils).Register();
		}
	}
}