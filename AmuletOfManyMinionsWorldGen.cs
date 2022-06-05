using AmuletOfManyMinions.Items.Consumables;
using AmuletOfManyMinions.Projectiles.Minions.BalloonMonkey;
using AmuletOfManyMinions.Projectiles.Minions.ExciteSkull;
using AmuletOfManyMinions.Projectiles.Minions.FishBowl;
using AmuletOfManyMinions.Projectiles.Minions.Rats;
using AmuletOfManyMinions.Projectiles.Minions.TumbleSheep;
using AmuletOfManyMinions.Projectiles.Squires.DemonSquire;
using AmuletOfManyMinions.Projectiles.Squires.GoldenRogueSquire;
using AmuletOfManyMinions.Projectiles.Squires.SkywareSquire;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using AmuletOfManyMinions.Items.Materials;
using AmuletOfManyMinions.Projectiles.Minions.LilEnt;

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
		LivingWoodChest = 12,
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
			if (chestTile.TileType == TileID.Containers)
			{
				int tileFrame = chestTile.TileFrameX / 36;
				if (tileFrame == (int)chestFrame && (!didPlace || Main.rand.NextBool(frequency)))
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

	// For more specific 
	internal class SpecificChestPlacementCriterion
	{
		public List<Chest> CandidateChests { get; private set; } = new();

		public Func<List<Chest>, List<Chest>> SelectChests { get; set; }

		public int ItemType { get; private set; }

		public ChestFrame ChestFrame { get; private set; }

		public SpecificChestPlacementCriterion(ChestFrame chestFrame, int itemType)
		{
			ChestFrame = chestFrame;
			ItemType = itemType;
		}

		public void AddChestIfMatches(Chest chest)
		{
			Tile chestTile = Main.tile[chest.x, chest.y];
			if (chestTile.TileType == TileID.Containers && (chestTile.TileFrameX / 36) == (int)ChestFrame)
			{
				CandidateChests.Add(chest);
			}
		}

		public void PlaceItemInChests()
		{
			SelectChests(CandidateChests).ForEach(chest => 
				AmuletOfManyMinionsWorldGen.PlaceItemInChest(chest, ItemType));
		}
	}

	class AmuletOfManyMinionsWorldGen : ModSystem
	{
		private static ChestLootInfo[] lootInfo;

		private static SpecificChestPlacementCriterion[] chestCriteria;

		public override void OnModLoad()
		{
			lootInfo = new ChestLootInfo[]
			{
				new ChestLootInfo(ChestFrame.LockedGoldChest, 6, ItemType<ExciteSkullMinionItem>()),
				new ChestLootInfo(ChestFrame.WoodenChest, 6, ItemType<TumbleSheepMinionItem>()),
				new ChestLootInfo(ChestFrame.WaterChest, 4, ItemType<FishBowlMinionItem>()),
				new ChestLootInfo(ChestFrame.IvyChest, 4, ItemType<BalloonMonkeyMinionItem>()),
				new ChestLootInfo(ChestFrame.LivingWoodChest, 4, ItemType<LilEntAccessory>()),
				new ChestLootInfo(ChestFrame.SkywareChest, 4, ItemType<SkywareSquireMinionItem>()),
				new ChestLootInfo(ChestFrame.LockedShadowChest, 4, ItemType<DemonSquireMinionItem>()),// shadow chest/golden rogue
				// all the various gold chest variants
				new ChestLootInfo(ChestFrame.GoldChest, 6, ItemType<RatsMinionItem>()),
				new ChestLootInfo(ChestFrame.RichMahogonyChest, 6, ItemType<RatsMinionItem>()),
				new ChestLootInfo(ChestFrame.MushroomChest, 6, ItemType<RatsMinionItem>()),
				new ChestLootInfo(ChestFrame.GraniteChest, 6, ItemType<RatsMinionItem>()),
				new ChestLootInfo(ChestFrame.MarbleChest, 6, ItemType<RatsMinionItem>()),
			};

			chestCriteria = new SpecificChestPlacementCriterion[]
			{
				new SpecificChestPlacementCriterion(ChestFrame.WoodenChest, ItemType<CombatPetFriendshipBow>())
				{
					SelectChests = chests => chests
						.Where(chest => chest.x > Main.maxTilesX / 3 && chest.x < 2 * Main.maxTilesX / 3 )
						.OrderBy(chest=>chest.y)
						.Take(1)
						.ToList()
				},

				new SpecificChestPlacementCriterion(ChestFrame.LockedShadowChest, ItemType<InertCombatPetFriendshipBow>())
				{
					SelectChests = chests => chests
						.OrderBy(chest=>Math.Abs(chest.x - Main.maxTilesX /2))
						.Take(1)
						.ToList()
				}
			};
		}

		public override void Unload()
		{
			lootInfo = null;
			chestCriteria = null;
		}

		internal static void PlaceItemInChest(Chest chest, int itemType)
		{
			for (int i = 0; i < 40; i++)
			{
				if (chest.item[i].IsAir)
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
							PlaceItemInChest(chest, chestItem);
							break;
						}
					}
					for(int i = 0; i < chestCriteria.Length; i++)
					{
						chestCriteria[i].AddChestIfMatches(chest);
					}
				}
			}
			for(int i = 0; i < chestCriteria.Length; i++)
			{

				Chest chosen = chestCriteria[i].SelectChests(chestCriteria[i].CandidateChests)[0];
				Mod.Logger.Info($"Chosen chest {i}: {chosen.x} {chosen.y} ({Main.maxTilesX})");
				chestCriteria[i].PlaceItemInChests();
				chestCriteria[i].CandidateChests.Clear();
			}
		}
	}
}
