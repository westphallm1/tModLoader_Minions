using Terraria.ID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using AmuletOfManyMinions.Projectiles.Squires.VikingSquire;
using AmuletOfManyMinions.Projectiles.Squires.GoldenRogueSquire;
using AmuletOfManyMinions.Projectiles.Squires.SeaSquire;

namespace AmuletOfManyMinions
{
	class AmuletOfManyMinionsWorld : ModWorld
	{
		private bool didPlaceShadowSquire;
		private bool didPlaceSeaSquire;
		private bool didPlaceVikingSquire;
		private void placeItemInChest(Chest chest, int itemType)
		{
			for(int i = 0; i < 40; i++)
			{
				if(chest.item[i].type == ItemID.None)
				{
					chest.item[i].SetDefaults(itemType);
					break;
				}
			}
		}

		private int? getItemForChest(Chest chest)
		{
			int frozenFrame = 11;
			int shadowFrame = 4;
			int waterFrame = 17;
			int? itemType = null;
			Tile chestTile = Main.tile[chest.x, chest.y];
			if(chestTile.type == TileID.Containers)
			{
				int tileFrame = chestTile.frameX / 36;
				if(tileFrame == frozenFrame && (!didPlaceVikingSquire || Main.rand.Next(3) == 0))
				{
					// ice chests are rarer than I thought, make an npc drop instead
					//didPlaceVikingSquire = true;
					//itemType = ItemType<VikingSquireMinionItem>();
				} else if(tileFrame == shadowFrame && (!didPlaceShadowSquire || Main.rand.Next(4) == 0))
				{
					didPlaceShadowSquire = true;
					itemType = ItemType<GoldenRogueSquireMinionItem>();
				} else if(tileFrame == waterFrame && (!didPlaceSeaSquire || Main.rand.Next(6) == 0))
				{
					// don't care for chest-only items
					//didPlaceSeaSquire = true;
					//itemType = ItemType<SeaSquireMinionItem>();
				}
			}
			return itemType;
		}
		// populate chests
		public override void PostWorldGen()
		{
			didPlaceShadowSquire = false;
			didPlaceVikingSquire = false;
			didPlaceSeaSquire = false;
			for(int chestIdx = 0; chestIdx < Main.chest.Length; chestIdx++)
			{
				Chest chest = Main.chest[chestIdx];
				if(chest != null && getItemForChest(chest) is int chestItem)
				{
					placeItemInChest(chest, chestItem);
				}
			}
		}
	}
}
