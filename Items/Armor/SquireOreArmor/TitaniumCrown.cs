using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.SquireOreArmor
{
	[AutoloadEquip(EquipType.Head)]
	class TitaniumCrown : ModItem
	{
		public static readonly int SetBonusDamageIncrease = 10;
		public static readonly int SetBonusSquireTravelRangeIncrease = 5;
		public static readonly int MinionDamageIncrease = 15;
		public static readonly int MaxMinionIncrease = 1;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinionDamageIncrease, MaxMinionIncrease);
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
			return body.type == ItemID.TitaniumBreastplate && legs.type == ItemID.TitaniumLeggings;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += MinionDamageIncrease / 100f;
			player.maxMinions += MaxMinionIncrease;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = SetBonusText.ToString();
			player.GetDamage<SummonDamageClass>() += SetBonusDamageIncrease / 100f;
			player.onHitTitaniumStorm = true;
			SquireModPlayer squirePlayer = player.GetModPlayer<SquireModPlayer>();
			squirePlayer.SquireRangeFlatBonus += SetBonusSquireTravelRangeIncrease * 16f;
			squirePlayer.hardmodeOreSquireArmorSetEquipped = true;
			// insert whatever variable needs to be activated so the player's minions will release homing fungi spores similar to the fungi bulb, but just recolored to look like a mushroom.
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.TitaniumBar, 12).AddTile(TileID.MythrilAnvil).Register();
		}
	}
}
