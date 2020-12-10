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

namespace AmuletOfManyMinions.Items.Armor.SquireOreArmor
{
	[AutoloadEquip(EquipType.Head)]
	class AdamantiteBicorn : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Adamantite Commander's Helm");
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
			return body.type == ItemID.AdamantiteBreastplate && legs.type == ItemID.AdamantiteLeggings;
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamageMult += 0.15f;
			player.maxMinions += 1;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "Increases minion damage by 25%\n" +
				"Increases squire travel range by 5 blocks\n" +
				"Increases squire travel speed by 25%";
			player.minionDamageMult += 0.25f;
			SquireModPlayer squirePlayer = player.GetModPlayer<SquireModPlayer>();
			squirePlayer.squireRangeFlatBonus += 60f;
			squirePlayer.squireTravelSpeedMultiplier += 0.25f;
			squirePlayer.hardmodeOreSquireArmorSetEquipped = true;
			// insert whatever variable needs to be activated so the player's minions will release homing fungi spores similar to the fungi bulb, but just recolored to look like a mushroom.
		}

		public override void ArmorSetShadows(Player player)
		{
			if(player.GetModPlayer<SquireModPlayer>().hardmodeOreSquireArmorSetEquipped)
			{
				player.armorEffectDrawOutlines = true;
			}
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.AdamantiteBar, 12);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
