using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Accessories
{
	[AutoloadEquip(EquipType.Neck)]
	class CharmOfMightyMinions : ModItem
	{
		public static readonly int MaxMinionDecrease = 1;
		public static readonly int MinionVarietyDecrease = 2;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxMinionDecrease, MinionVarietyDecrease);
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
			player.GetDamage<SummonDamageClass>() += 0.25f;
			player.maxMinions = Math.Max(0, player.maxMinions - MaxMinionDecrease);
			player.GetModPlayer<MinionSpawningItemPlayer>().minionVarietyDamageBonus -= MinionVarietyDecrease / 100f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.DarkShard, 1).AddIngredient(ItemID.SoulofLight, 8).AddTile(TileID.Anvils).Register();
		}

	}
}
