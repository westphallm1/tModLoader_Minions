using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.SquireSpookyArmor
{
	[AutoloadEquip(EquipType.Head)]
	class SpookyMask : ModItem
	{
		public static readonly int SetBonusDamageIncrease = 32;
		public static readonly int SetBonusSquireTravelRangeIncrease = 7;
		public static readonly int SetBonusAttackSpeedIncrease = 20;
		public static readonly int MinionDamageIncrease = 12;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinionDamageIncrease);
		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs(SetBonusDamageIncrease, SetBonusSquireTravelRangeIncrease, SetBonusAttackSpeedIncrease);
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 18;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Yellow;
			Item.defense = 21;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemID.SpookyBreastplate && legs.type == ItemID.SpookyLeggings;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += MinionDamageIncrease / 100f;
			player.maxMinions -= 1;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = SetBonusText.ToString();
			player.GetDamage<SummonDamageClass>() += SetBonusDamageIncrease / 100f;
			SquireModPlayer squirePlayer = player.GetModPlayer<SquireModPlayer>();
			squirePlayer.SquireRangeFlatBonus += SetBonusSquireTravelRangeIncrease * 16f;
			squirePlayer.SquireAttackSpeedMultiplier *= 1 - SetBonusAttackSpeedIncrease / 100f;
			squirePlayer.spookyArmorSetEquipped = true;
			player.maxMinions -= 1;
			// insert whatever variable needs to be activated so the player's minions will release homing fungi spores similar to the fungi bulb, but just recolored to look like a mushroom.
		}

		public override void ArmorSetShadows(Player player)
		{
			if (player.GetModPlayer<SquireModPlayer>().spookyArmorSetEquipped)
			{
				player.armorEffectDrawOutlines = true;
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.SpookyWood, 200).AddTile(TileID.WorkBenches).Register();
		}
	}
}
