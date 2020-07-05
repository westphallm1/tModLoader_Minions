using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using System.ComponentModel;

namespace DemoMod.Items.Armor
{
	[AutoloadEquip(EquipType.Legs)]
	public class ExampleLeggings : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Butterfly Legs");
			Tooltip.SetDefault(
				"4% increased minion damage"
				+ "\n5% increased movement speed");
		}

		public override void SetDefaults() {
			item.width = 18;
			item.height = 18;
			item.value = 10000;
			item.rare = ItemRarityID.White;
			item.defense = 3;
		}

		public override void UpdateEquip(Player player) {
			player.minionDamageMult += 0.04f;
			player.moveSpeed += 0.05f;
		}
	}
}