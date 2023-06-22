using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Accessories
{
	[AutoloadEquip(EquipType.Neck)]
	class CharmOfManyMinions : ModItem
	{
		public static readonly int MaxMinionIncrease = 1;
		public static readonly int MinionVarietyIncrease = 1;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxMinionIncrease, MinionVarietyIncrease);

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() -= 0.1f;
			player.maxMinions += MaxMinionIncrease;
			player.GetModPlayer<MinionSpawningItemPlayer>().minionVarietyDamageBonus += MinionVarietyIncrease / 100f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.LightShard, 1).AddIngredient(ItemID.SoulofNight, 8).AddTile(TileID.Anvils).Register();
		}

	}
}
