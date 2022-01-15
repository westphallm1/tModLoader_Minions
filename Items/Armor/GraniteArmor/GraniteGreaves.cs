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
			    "Increases squire travel speed by 25%\n" +
				"10% increased movement speed");
		}

		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.value = Item.sellPrice(gold: 3);
			item.rare = ItemRarityID.Pink;
			item.defense = 12;
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamage += 0.1f;
			player.moveSpeed += 0.1f;
			player.GetModPlayer<SquireModPlayer>().squireTravelSpeedMultiplier += 0.28f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.HallowedBar, 14);
			recipe.AddIngredient(ItemType<GraniteSpark>(), 6);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
