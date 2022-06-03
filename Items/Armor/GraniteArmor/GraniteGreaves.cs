using AmuletOfManyMinions.Items.Materials;
using AmuletOfManyMinions.Projectiles.Squires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Armor.GraniteArmor
{
	[AutoloadEquip(EquipType.Legs)]
	class GraniteGreaves : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Granite Greaves");
			Tooltip.SetDefault(""+
			    "Increases minion damage by 10%\n" +
			    "Increases squire travel range by 2 blocks\n" +
				"10% increased movement speed");
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 18;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Pink;
			Item.defense = 12;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += 0.10f;
			player.moveSpeed += 0.1f;
			player.GetModPlayer<SquireModPlayer>().SquireRangeFlatBonus += 32f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.HallowedBar, 14).AddIngredient(ItemType<GraniteSpark>(), 6).AddTile(TileID.MythrilAnvil).Register();
		}
	}
}
