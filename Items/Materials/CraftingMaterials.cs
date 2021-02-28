using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Materials
{
	public class GuideHair : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Lock of the Guide's Hair");
			Tooltip.SetDefault("Can fetch a high price on the black market,\namong other nefarious purposes");
			item.width = 30;
			item.height = 32;
			item.value = Item.sellPrice(gold: 5);
			item.rare = ItemRarityID.Lime;
		}
	}
	public class GraniteSpark : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Granite Spark");
			Tooltip.SetDefault(
				"A fragment of energy from a granite elemental.\n" +
				"Used to craft tools that enhance your minions' AI.");
			item.width = 30;
			item.height = 32;
			item.value = Item.sellPrice(copper: 50);
			item.rare = ItemRarityID.White;
		}

	}
}
