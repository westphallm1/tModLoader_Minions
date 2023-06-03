using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Accessories.SquireSpyglass
{
	class SquireSpyglass : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<SquireModPlayer>().SquireRangeFlatBonus += 48f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddRecipeGroup(AoMMSystem.GoldBarRecipeGroup, 6).AddIngredient(ItemID.Lens, 4).AddTile(TileID.Anvils).Register();
		}
	}
}
