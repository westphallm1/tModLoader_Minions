using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.AridArmor
{
	[AutoloadEquip(EquipType.Legs)]
	public class AridLeggings : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Arid Leggings");
			/* Tooltip.SetDefault("Increases minion damage by 6%\n" +
				"Increases squire travel range by 1 block"); */
		}

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
			player.GetDamage<SummonDamageClass>() += 0.06f;
			player.GetModPlayer<SquireModPlayer>().SquireRangeFlatBonus += 16f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.AncientCloth, 4).AddIngredient(ItemID.AntlionMandible, 4).AddIngredient(ItemID.FossilOre, 12).AddTile(TileID.Anvils).Register();
		}
	}
}