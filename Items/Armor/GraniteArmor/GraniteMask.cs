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
			item.width = 28;
			item.height = 18;
			item.value = Item.sellPrice(gold: 5);
			item.rare = ItemRarityID.Pink;
			item.defense = 8;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemType<GraniteChestguard>() && legs.type == ItemType<GraniteGreaves>();
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamage += 0.1f;
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
			player.setBonus = "Increases minion damage by 10%\n"
				+ "Increases squire travel range by 2 blocks\n"
				+ "Increases movement speed by 10%";
			player.minionDamage += 0.1f;
			player.moveSpeed += 0.1f;
			player.GetModPlayer<SquireModPlayer>().squireRangeFlatBonus += 16f;
			player.GetModPlayer<SquireModPlayer>().graniteArmorEquipped = true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.HallowedBar, 10);
			recipe.AddIngredient(ItemType<GraniteSpark>(), 4);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
