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

namespace AmuletOfManyMinions.Items.Armor.SquireSpookyArmor
{
	[AutoloadEquip(EquipType.Head)]
	class SpookyMask : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spooky Mask");
			Tooltip.SetDefault("Increases minion damage by 11%" );
		}

		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.value = Item.sellPrice(gold: 1);
			item.rare = ItemRarityID.Yellow;
			item.defense = 21;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemID.SpookyBreastplate && legs.type == ItemID.SpookyLeggings;
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamageMult += 0.15f;
			player.maxMinions += 1;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "Increases minion damage by 35%\n" +
				"Increases squire travel range by 7 blocks\n" +
				"Increases squire travel speed by 30%\n" +
				"Increases squire attack speed by 20%\n";
			player.minionDamageMult += 0.35f;
			SquireModPlayer squirePlayer = player.GetModPlayer<SquireModPlayer>();
			squirePlayer.squireRangeFlatBonus += 112f;
			squirePlayer.squireTravelSpeedMultiplier += 0.30f;
			squirePlayer.squireAttackSpeedMultiplier *= 0.80f;
			squirePlayer.spookyArmorSetEquipped = true;
			// insert whatever variable needs to be activated so the player's minions will release homing fungi spores similar to the fungi bulb, but just recolored to look like a mushroom.
		}

		public override void ArmorSetShadows(Player player)
		{
			if(player.GetModPlayer<SquireModPlayer>().spookyArmorSetEquipped)
			{
				player.armorEffectDrawOutlines = true;
			}
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.SpookyWood, 200);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
