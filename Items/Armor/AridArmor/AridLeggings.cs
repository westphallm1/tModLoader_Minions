using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.AridArmor
{
	[AutoloadEquip(EquipType.Legs)]
	public class AridLeggings : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Arid Leggings");
			Tooltip.SetDefault("Increases minion damage by 6%\n" +
				"Increases squire travel speed by 12%");
		}

		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.value = Item.sellPrice(silver: 2);
			item.rare = ItemRarityID.Green;
			item.defense = 6;
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamageMult += 0.06f;
			player.GetModPlayer<SquireModPlayer>().squireTravelSpeedMultiplier += 0.12f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.AncientCloth, 4);
			recipe.AddIngredient(ItemID.AntlionMandible, 4);
			recipe.AddIngredient(ItemID.FossilOre, 12);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}