using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace AmuletOfManyMinions.Items.Armor
{
	[AutoloadEquip(EquipType.Head)]
	public class ForagerHelmet : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Forager's Helmet");
			Tooltip.SetDefault(""
				+ "3% increased minion damage\n"
				+ "+1 minion knockback");
		}

		public override void SetDefaults() {
			item.width = 18;
			item.height = 18;
			item.value = Item.sellPrice(silver: 1);
			item.rare = ItemRarityID.White;
			item.defense = 2;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ItemType<ForagerBreastplate>() && legs.type == ItemType<ForagerLeggings>();
		}

		public override void UpdateEquip(Player player) {
			player.minionDamageMult += 0.05f;
		}

		public override void UpdateArmorSet(Player player) {
			player.setBonus = "Increases your max minions by 1.";
			player.maxMinions++;
		}

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Daybloom, 2);
            recipe.AddIngredient(ItemID.Wood, 9);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
	}
}