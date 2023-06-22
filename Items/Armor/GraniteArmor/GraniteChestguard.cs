﻿using AmuletOfManyMinions.Items.Materials;
using AmuletOfManyMinions.Projectiles.Squires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Armor.GraniteArmor
{
	[AutoloadEquip(EquipType.Body)]
	class GraniteChestguard : ModItem
	{

		public static readonly int MinionDamageIncrease = 10;
		public static readonly int SquireAttackSpeedIncrease = 15;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinionDamageIncrease, SquireAttackSpeedIncrease);
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 18;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Pink;
			Item.defense = 15;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += MinionDamageIncrease / 100f;
			player.GetModPlayer<SquireModPlayer>().SquireAttackSpeedMultiplier *= (1f - SquireAttackSpeedIncrease / 100f);
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.HallowedBar, 20).AddIngredient(ItemType<GraniteSpark>(), 8).AddTile(TileID.MythrilAnvil).Register();
		}
	}
}
