using AmuletOfManyMinions.Core.Minions.Pathfinding;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Accessories.PassivePathfindingAccessories
{
	class MinionGPS : ModItem
	{

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("GPS of Many Minions");
			Tooltip.SetDefault(
				"Allows your minions to automatically attack around corners in a 25 tile radius.");
		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 32;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 2);
			item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<MinionPathfindingPlayer>().passivePathfindingRange = 30 * 25;
		}
	}
}
