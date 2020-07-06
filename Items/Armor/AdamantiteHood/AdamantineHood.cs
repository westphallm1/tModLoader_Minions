using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace DemoMod.Items.Armor.AdamantiteHood
{
	[AutoloadEquip(EquipType.Head)]
	public class AdamantineHood : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Adamantite Hood");
			Tooltip.SetDefault(""
				+ "7% increased minion damge"
				+ "+1 maximum minions");
		}

        public override void AddRecipes()
        {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.AdamantiteBar, 12);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

        public override void SetDefaults() {
			item.width = 18;
			item.height = 18;
			item.value = 10000;
			item.rare = ItemRarityID.White;
			item.defense = 2;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ItemID.AdamantiteLeggings && legs.type == ItemID.AdamantiteBreastplate;
		}

		public override void UpdateEquip(Player player) {
			player.minionDamageMult += 0.07f;
			player.maxMinions++;
		}

		public override void UpdateArmorSet(Player player) {
			player.setBonus = "+2 maximum minions\n" +
				"13% increased minion damage";
			player.maxMinions+=2;
			player.minionDamage += 0.13f;
		}
	}
}