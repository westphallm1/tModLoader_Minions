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
	[AutoloadEquip(EquipType.Body)]
	class GraniteChestguard : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Granite Chestguard");
			Tooltip.SetDefault(""
				+ "Increases minion damage by 12%\n"
				+ "Increases squire attack speed by 15%");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 18;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Pink;
			Item.defense = 16;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += 0.12f;
			player.GetModPlayer<SquireModPlayer>().squireAttackSpeedMultiplier *= 0.85f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.HallowedBar, 20).AddIngredient(ItemType<GraniteSpark>(), 8).AddTile(TileID.MythrilAnvil).Register();
		}
	}
}
