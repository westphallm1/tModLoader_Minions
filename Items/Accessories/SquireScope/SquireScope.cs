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
							"\nIncreases squire travel range by 8 blocks");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 6, silver: 1);
			Item.rare = ItemRarityID.Lime;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<SquireModPlayer>().SquireRangeFlatBonus += 128;
			if (SquireMinionTypes.Contains(player.HeldItem.shoot))
			{
				player.scope = true;
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.RifleScope, 1).AddIngredient(ItemType<SquireSpyglass.SquireSpyglass>(), 1).AddTile(TileID.TinkerersWorkbench).Register();
		}
	}
}
