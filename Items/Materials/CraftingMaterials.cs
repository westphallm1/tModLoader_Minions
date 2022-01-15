using AmuletOfManyMinions.Core.BackportUtils;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Materials
{
	public class GuideHair : BackportModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lock of the Guide's Hair");
			Tooltip.SetDefault("Can fetch a high price on the black market,\namong other nefarious purposes");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Lime;
		}
	}
	public class GraniteSpark : BackportModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Granite Spark");
			Tooltip.SetDefault(
				"A fragment of energy from a granite elemental.\n" +
				"Used to craft tools that enhance your minions' AI.");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.maxStack = 999;
			Item.value = Item.sellPrice(copper: 50);
			Item.rare = ItemRarityID.White;
		}
	}

	public class InertCombatPetFriendshipBow: BackportModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Inert Bow of Friendship");
			Tooltip.SetDefault(
				"A Bow of Friendship that's lost its magical powers.\n" +
				"It can be restored when combined with ingredients from various biomes.");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.maxStack = 999;
			Item.value = Item.sellPrice(copper: 50);
			Item.rare = ItemRarityID.Orange;
		}
	}
}
