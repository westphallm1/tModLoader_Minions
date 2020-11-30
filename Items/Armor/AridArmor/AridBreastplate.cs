using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.AridArmor
{
	[AutoloadEquip(EquipType.Body)]
	public class AridBreastplate : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Arid Breastplate");
			Tooltip.SetDefault(""
				+ "Increases minion damage by 8%\n"
				+ "Increases squire attack speed by 10%");
		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 18;
			item.value = Item.sellPrice(silver: 2, copper: 50);
			item.rare = ItemRarityID.Green;
			item.defense = 6;
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamageMult += 0.08f;
			player.GetModPlayer<SquireModPlayer>().squireAttackSpeedMultiplier *= 0.9f;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.AncientCloth, 5);
			recipe.AddIngredient(ItemID.AntlionMandible, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}