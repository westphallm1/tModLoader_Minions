using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.RoyalArmor
{
	[AutoloadEquip(EquipType.Body)]
	class RoyalGown : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Royal Gown");
			Tooltip.SetDefault(""
				+ "Increases minion damage by 12%\n"
				+ "Increases squire travel range by 10%");
		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 18;
			item.value = Item.sellPrice(silver: 2, copper: 50);
			item.rare = ItemRarityID.Green;
			item.defense = 3;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<SquireModPlayer>().squireRangeMultiplier += 0.1f;
			player.minionDamageMult += 0.12f;
		}

		public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
		{
			robes = true;
			equipSlot = mod.GetEquipSlot("RoyalGown_Legs", EquipType.Legs);
		}
	}
}
