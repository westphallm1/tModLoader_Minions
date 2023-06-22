using AmuletOfManyMinions.Projectiles.Squires;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.RoyalArmor
{
	[AutoloadEquip(EquipType.Body)]
	class RoyalGown : ModItem
	{
		public static readonly int SummonDamageIncrease = 12;
		public static readonly int SquireRangeIncrease = 2;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(SummonDamageIncrease, SquireRangeIncrease);

		private int legsSlot = -1;

		public override void Load()
		{
			if (!Main.dedServ)
			{
				legsSlot = EquipLoader.AddEquipTexture(Mod, Texture + "_Legs", EquipType.Legs, name: nameof(RoyalGown) + "_Legs");
			}
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
			player.GetModPlayer<SquireModPlayer>().SquireRangeFlatBonus += SquireRangeIncrease * 16f;
			player.GetDamage<SummonDamageClass>() += SquireRangeIncrease / 100f;
		}

		public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
		{
			robes = true;
			equipSlot = legsSlot;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Gel, 30).AddRecipeGroup(AoMMSystem.GoldBarRecipeGroup, 15).AddTile(TileID.Solidifier).Register();
		}
	}
}
