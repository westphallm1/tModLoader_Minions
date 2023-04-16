﻿using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.SquireOreArmor
{
	[AutoloadEquip(EquipType.Head)]
	class AdamantiteBicorn : ModItem
	{
		public static readonly int SetBonusDamageIncrease = 20;
		public static readonly int SetBonusSquireTravelRangeIncrease = 5;
		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs(SetBonusDamageIncrease, SetBonusSquireTravelRangeIncrease);
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 18;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
			Item.defense = 6;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemID.AdamantiteBreastplate && legs.type == ItemID.AdamantiteLeggings;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += 0.15f;
			player.maxMinions += 1;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = SetBonusText.ToString();
			player.GetDamage<SummonDamageClass>() += SetBonusDamageIncrease / 100f;
			SquireModPlayer squirePlayer = player.GetModPlayer<SquireModPlayer>();
			squirePlayer.SquireRangeFlatBonus += SetBonusSquireTravelRangeIncrease * 16f;
			squirePlayer.hardmodeOreSquireArmorSetEquipped = true;
			// insert whatever variable needs to be activated so the player's minions will release homing fungi spores similar to the fungi bulb, but just recolored to look like a mushroom.
		}

		public override void ArmorSetShadows(Player player)
		{
			if (player.GetModPlayer<SquireModPlayer>().hardmodeOreSquireArmorSetEquipped)
			{
				player.armorEffectDrawOutlines = true;
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.AdamantiteBar, 12).AddTile(TileID.MythrilAnvil).Register();
		}
	}
}
