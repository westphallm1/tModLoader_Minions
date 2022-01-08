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
	[AutoloadEquip(EquipType.Head)]
	class GraniteMask : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Granite Mask");
			Tooltip.SetDefault(""
				+ "Increases minion damage by 10%\n"
				+ "Increases your max number of minions by 1\n"
				+ "Increases squire travel range by 4 blocks");
		}
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 18;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
			Item.defense = 9;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemType<GraniteChestguard>() && legs.type == ItemType<GraniteGreaves>();
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += 0.10f;
			player.maxMinions += 1;
			player.GetModPlayer<SquireModPlayer>().squireRangeFlatBonus += 64;
		}

		public override void ArmorSetShadows(Player player)
		{
			if (player.GetModPlayer<SquireModPlayer>().graniteArmorEquipped)
			{
				player.armorEffectDrawOutlines = true;
			}
		}
		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "Become immune after striking an enemy\n"
				+ "Increases minion damage by 12%\n"
				+ "Increases squire travel range by 2 blocks";
			player.GetDamage<SummonDamageClass>() += 0.12f;
			player.onHitDodge = true;
			player.GetModPlayer<SquireModPlayer>().squireRangeFlatBonus += 16f;
			player.GetModPlayer<SquireModPlayer>().graniteArmorEquipped = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.HallowedBar, 10).AddIngredient(ItemType<GraniteSpark>(), 4).AddTile(TileID.MythrilAnvil).Register();
		}
	}
}
