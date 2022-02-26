using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.SquireSpookyArmor
{
	[AutoloadEquip(EquipType.Head)]
	class SpookyMask : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spooky Mask");
			Tooltip.SetDefault("Increases minion damage by 12%, but decreases max minions");
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
			player.GetDamage<SummonDamageClass>() += 0.12f;
			player.maxMinions -= 1;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "Increases minion damage by 32%\n" +
				"Increases squire travel range by 7 blocks\n" +
				"Increases squire travel speed by 30%\n" +
				"Increases squire attack speed by 20%\n" + 
				"Decreases max minions";
			player.GetDamage<SummonDamageClass>() += 0.32f;
			SquireModPlayer squirePlayer = player.GetModPlayer<SquireModPlayer>();
			squirePlayer.SquireRangeFlatBonus += 112f;
			squirePlayer.SquireTravelSpeedMultiplier += 0.30f;
			squirePlayer.SquireAttackSpeedMultiplier *= 0.80f;
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
