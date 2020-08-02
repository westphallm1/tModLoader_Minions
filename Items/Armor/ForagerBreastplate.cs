using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Items.Armor
{
	[AutoloadEquip(EquipType.Body)]
	public class ForagerBreastplate : ModItem
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Forager's Breastplate");
			Tooltip.SetDefault(""
				+ "3% increased minion damge\n"
				+ "+1 minion knockback");
		}

		public override void SetDefaults() {
			item.width = 18;
			item.height = 18;
			item.value = 10000;
			item.rare = ItemRarityID.White;
			item.defense = 4;
		}

		public override void UpdateEquip(Player player) {
			player.minionDamageMult += 0.03f;
			player.minionKB += 1;
		}
	}
}