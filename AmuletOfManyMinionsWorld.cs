using AmuletOfManyMinions.Projectiles.Minions.BalloonMonkey;
using AmuletOfManyMinions.Projectiles.Minions.ExciteSkull;
using AmuletOfManyMinions.Projectiles.Minions.FishBowl;
using AmuletOfManyMinions.Projectiles.Minions.Rats;
using AmuletOfManyMinions.Projectiles.Minions.TumbleSheep;
using AmuletOfManyMinions.Projectiles.Squires.DemonSquire;
using AmuletOfManyMinions.Projectiles.Squires.GoldenRogueSquire;
using AmuletOfManyMinions.Projectiles.Squires.SkywareSquire;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions
{
	enum ChestFrame
	{
		WoodenChest = 0,
		GoldChest = 1,
		LockedGoldChest = 2,
		LockedShadowChest  = 4,
		RichMahogonyChest = 8,
		IvyChest = 10,
		SkywareChest = 13,
		WaterChest = 17,
		MushroomChest = 32,
		GraniteChest= 50,
		MarbleChest = 51,
	}

	struct ChestLootInfo
	{
		ChestFrame chestFrame;
		int itemType;
		int frequency;
		bool didPlace;
	
		public ChestLootInfo(ChestFrame chestFrame, int frequency, int itemType)
		{
			this.chestFrame = chestFrame;
			this.itemType = itemType;
			this.frequency = frequency;
			this.didPlace = false;
		}

		public int? GetItemForChest(Chest chest)
		{
			int? itemType = null;
			Tile chestTile = Main.tile[chest.x, chest.y];
			if (chestTile.type == TileID.Containers)
			{
				int tileFrame = chestTile.frameX / 36;
				if (tileFrame == (int)chestFrame && (!didPlace || Main.rand.Next(frequency) == 0))
				{
					didPlace = true;
					itemType = this.itemType;
				}
			}
			return itemType;
		}

		public void reset()
		{
			didPlace = false;
		}
	}
	class AmuletOfManyMinionsWorld : ModWorld
	{
		private static ChestLootInfo[] lootInfo;

		public static void Load()
		{
			lootInfo = new ChestLootInfo[]
			{
				new ChestLootInfo(ChestFrame.LockedGoldChest, 6, ItemType<ExciteSkullMinionItem>()),
				new ChestLootInfo(ChestFrame.WoodenChest, 6, ItemType<TumbleSheepMinionItem>()),
				new ChestLootInfo(ChestFrame.WaterChest, 4, ItemType<FishBowlMinionItem>()),
				new ChestLootInfo(ChestFrame.IvyChest, 4, ItemType<BalloonMonkeyMinionItem>()),
				new ChestLootInfo(ChestFrame.SkywareChest, 4, ItemType<SkywareSquireMinionItem>()),// shadow chest/golden rogue
				new ChestLootInfo(ChestFrame.LockedShadowChest, 4, ItemType<DemonSquireMinionItem>()),// shadow chest/golden rogue
				// all the various gold chest variants
				new ChestLootInfo(ChestFrame.GoldChest, 6, ItemType<RatsMinionItem>()),
				new ChestLootInfo(ChestFrame.RichMahogonyChest, 6, ItemType<RatsMinionItem>()),
				new ChestLootInfo(ChestFrame.MushroomChest, 6, ItemType<RatsMinionItem>()),
				new ChestLootInfo(ChestFrame.GraniteChest, 6, ItemType<RatsMinionItem>()),
				new ChestLootInfo(ChestFrame.MarbleChest, 6, ItemType<RatsMinionItem>()),
			};
		}

		public static void Unload()
		{
			lootInfo = null;
		}

		private void placeItemInChest(Chest chest, int itemType)
		{
			for (int i = 0; i < 40; i++)
			{
				if (chest.item[i].type == ItemID.None)
				{
					chest.item[i].SetDefaults(itemType);
					break;
				}
			}
		}

		// populate chests
		public override void PostWorldGen()
		{
			for(int i = 0; i < lootInfo.Length; i++)
			{
				lootInfo[i].reset();
			}

			for (int chestIdx = 0; chestIdx < Main.chest.Length; chestIdx++)
			{
				Chest chest = Main.chest[chestIdx];
				if (chest != null)
				{
					for(int i = 0; i < lootInfo.Length; i++)
					{
						if(lootInfo[i].GetItemForChest(chest) is int chestItem)
						{
							placeItemInChest(chest, chestItem);
							break;
						}
					}
				}
			}
		}
	}
}
