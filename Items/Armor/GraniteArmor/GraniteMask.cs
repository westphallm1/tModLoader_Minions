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
	[AutoloadEquip(EquipType.Head)]
	class GraniteMask : ModItem
	{
		public static readonly int SetBonusDamageIncrease = 12;
		public static readonly int SetBonusSquireTravelRangeIncrease = 1;

		public static readonly int MinionDamageIncrease = 10;
		public static readonly int SquireRangeIncrease = 4;
		public static readonly int MaxMinionIncrease = 1;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinionDamageIncrease, SquireRangeIncrease, MaxMinionIncrease);
		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs(SetBonusDamageIncrease, SetBonusSquireTravelRangeIncrease);
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
			player.GetDamage<SummonDamageClass>() += MinionDamageIncrease / 100f;
			player.maxMinions += MaxMinionIncrease;
			player.GetModPlayer<SquireModPlayer>().SquireRangeFlatBonus += SquireRangeIncrease * 16f;
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
			player.setBonus = SetBonusText.ToString();
			player.GetDamage<SummonDamageClass>() += SetBonusDamageIncrease / 100f;
			player.onHitDodge = true;
			SquireModPlayer squireModPlayer = player.GetModPlayer<SquireModPlayer>();
			squireModPlayer.SquireRangeFlatBonus += SetBonusSquireTravelRangeIncrease * 16f;
			squireModPlayer.graniteArmorEquipped = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.HallowedBar, 10).AddIngredient(ItemType<GraniteSpark>(), 4).AddTile(TileID.MythrilAnvil).Register();
		}
	}
}
