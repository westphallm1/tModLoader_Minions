using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.SquireScope
{
	class SquireScope : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Increases view range for squires (Right Click to zoom out)" +
							"\n10% increased minion damage" +
							"\nIncreases squire travel range by 5 blocks");
		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 32;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 6, silver: 1);
			item.rare = ItemRarityID.Lime;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<SquireModPlayer>().squireRangeFlatBonus += 80f;
			player.minionDamageMult += 0.1f;
			if (SquireMinionTypes.Contains(player.HeldItem.shoot))
			{
				player.scope = true;
			}
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.SniperScope, 1);
			recipe.AddIngredient(ItemType<SquireSpyglass.SquireSpyglass>(), 1);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
