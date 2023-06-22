using AmuletOfManyMinions.Projectiles.Minions.FlyingSword;
using AmuletOfManyMinions.Projectiles.Minions.NullHatchet;
using AmuletOfManyMinions.Projectiles.Minions.PossessedCopperSword;
using AmuletOfManyMinions.Projectiles.Minions.SpiritGun;
using AmuletOfManyMinions.Projectiles.Minions.VoidKnife;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories
{
	[AutoloadEquip(EquipType.Neck)]
	class AmuletOfManyMinions : ModItem
	{
		public static readonly int MaxMinionIncrease = 2;
		public static readonly int MinionVarietyIncrease = 2;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxMinionIncrease, MinionVarietyIncrease);

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
		}
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Expert;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += 0.2f;
			player.maxMinions += MaxMinionIncrease;
			player.GetModPlayer<MinionSpawningItemPlayer>().minionVarietyDamageBonus += MinionVarietyIncrease / 100f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemType<CopperSwordMinionItem>(), 1).AddIngredient(ItemType<SpiritGunMinionItem>(), 1).AddIngredient(ItemType<FlyingSwordMinionItem>(), 1).AddRecipeGroup(AoMMSystem.VoidDaggerRecipeGroup).AddIngredient(ItemType<CharmOfManyMinions>(), 1).AddIngredient(ItemType<CharmOfMightyMinions>(), 1).AddTile(TileID.MythrilAnvil).Register();
		}

	}
}
