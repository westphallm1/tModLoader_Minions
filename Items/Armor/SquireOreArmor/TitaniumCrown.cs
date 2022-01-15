using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.SquireOreArmor
{
	[AutoloadEquip(EquipType.Head)]
	class TitaniumCrown : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Titanium Admiral's Helm");
			Tooltip.SetDefault(""
				+ "Increases minion damage by 15%\n"
				+ "Increases max minions by 1");
		}

		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.value = Item.sellPrice(gold: 3);
			item.rare = ItemRarityID.LightRed;
			item.defense = 6;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemID.TitaniumBreastplate && legs.type == ItemID.TitaniumLeggings;
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamage += 0.15f;
			player.maxMinions += 1;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "Become immune after striking an enemy\n" +
				"Increases minion damage by 10%\n" +
				"Increases squire travel range by 5 blocks\n" +
				"Increases squire travel speed by 20%";
			player.minionDamage += 0.10f;
			player.onHitDodge = true; // titanium armor effect
			SquireModPlayer squirePlayer = player.GetModPlayer<SquireModPlayer>();
			squirePlayer.squireRangeFlatBonus += 80f;
			squirePlayer.squireTravelSpeedMultiplier += 0.20f;
			squirePlayer.hardmodeOreSquireArmorSetEquipped = true;
			// insert whatever variable needs to be activated so the player's minions will release homing fungi spores similar to the fungi bulb, but just recolored to look like a mushroom.
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.TitaniumBar, 12);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
