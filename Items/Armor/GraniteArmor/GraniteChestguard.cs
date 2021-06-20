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
			item.width = 30;
			item.height = 18;
			item.value = Item.sellPrice(gold: 4);
			item.rare = ItemRarityID.Pink;
			item.defense = 16;
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamageMult += 0.12f;
			player.GetModPlayer<SquireModPlayer>().squireAttackSpeedMultiplier *= 0.85f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.HallowedBar, 20);
			recipe.AddIngredient(ItemType<GraniteSpark>(), 8);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
