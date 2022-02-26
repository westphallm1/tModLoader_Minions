using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.RoyalArmor
{
	[AutoloadEquip(EquipType.Body)]
	class RoyalGown : ModItem
	{
		private int legsSlot = -1;

		public override void Load()
		{
			if (!Main.dedServ)
			{
				legsSlot = Mod.AddEquipTexture(this, EquipType.Legs, "AmuletOfManyMinions/Items/Armor/RoyalArmor/RoyalGown_Legs");
			}
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Royal Gown");
			Tooltip.SetDefault(""
				+ "Increases minion damage by 12%\n"
				+ "Increases squire travel range by 2 blocks");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 2, copper: 50);
			Item.rare = ItemRarityID.Green;
			Item.defense = 3;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<SquireModPlayer>().SquireRangeFlatBonus += 32f;
			player.GetDamage<SummonDamageClass>() += 0.12f;
		}

		public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
		{
			robes = true;
			equipSlot = legsSlot;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Gel, 30).AddRecipeGroup("AmuletOfManyMinions:Golds", 15).AddTile(TileID.Solidifier).Register();
		}
	}
}
