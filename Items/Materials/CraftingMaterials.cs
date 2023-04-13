using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Materials
{
	public class GuideHair : ModItem
	{
		public override void SetStaticDefaults()
		{
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Lime;
		}
	}
	public class GraniteSpark : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.maxStack = 999;
			Item.value = Item.sellPrice(copper: 50);
			Item.rare = ItemRarityID.White;
		}
	}

	public class InertCombatPetFriendshipBow: ModItem
	{
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
