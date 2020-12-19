using AmuletOfManyMinions.Items.Accessories.SquireBat;
using AmuletOfManyMinions.Items.Accessories.SquireSkull;
using AmuletOfManyMinions.Items.Armor.RoyalArmor;
using AmuletOfManyMinions.Items.Materials;
using AmuletOfManyMinions.Projectiles.Minions.BalloonBuddy;
using AmuletOfManyMinions.Projectiles.Minions.BeeQueen;
using AmuletOfManyMinions.Projectiles.Minions.BoneSerpent;
using AmuletOfManyMinions.Projectiles.Minions.CharredChimera;
using AmuletOfManyMinions.Projectiles.Minions.GoblinGunner;
using AmuletOfManyMinions.Projectiles.Squires.AncientCobaltSquire;
using AmuletOfManyMinions.Projectiles.Squires.ArmoredBoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.BoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.GuideSquire;
using AmuletOfManyMinions.Projectiles.Squires.PottedPal;
using AmuletOfManyMinions.Projectiles.Squires.Squeyere;
using AmuletOfManyMinions.Projectiles.Squires.VikingSquire;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.NPCs
{
	class LootTable : GlobalNPC
	{
		public static void SpawnRoyalArmor(Player lootRecipient, Action<int> spawnAction)
		{
			// always give the player the part of the set they're missing, if they're missing a part
			int itemType;
			if(!lootRecipient.inventory.Any(item=>(!item.IsAir) && item.type == ItemType<RoyalCrown>()))
			{
				itemType = ItemType<RoyalCrown>();
			} else if (!lootRecipient.inventory.Any(item=>(!item.IsAir) && item.type == ItemType<RoyalGown>()))
			{
				itemType = ItemType<RoyalGown>();
			} else
			{
				itemType = Main.rand.Next() < 0.5f ? ItemType<RoyalGown>() : ItemType<RoyalCrown>();
			}
			spawnAction.Invoke(itemType);
		}

		public override void NPCLoot(NPC npc)
		{
			base.NPCLoot(npc);
			// make all spawn chances more likely on expert mode
			float spawnChance = Main.rand.NextFloat() * (Main.expertMode ? 0.67f : 1);

			if(npc.type == NPCID.Guide)
			{
				if (Main.npc.Any(n => n.active && NPCSets.lunarBosses.Contains(n.type)))
				{
					Item.NewItem(npc.getRect(), ItemType<GuideHair>(), 1);
				} else if (NPC.downedBoss1 || NPC.downedSlimeKing)
				{
					Item.NewItem(npc.getRect(), ItemType<GuideSquireMinionItem>(), 1, prefixGiven: -1);
				}
			}

			if(spawnChance < 0.03f && NPCSets.preHardmodeIceEnemies.Contains(npc.netID))
			{
				Item.NewItem(npc.getRect(), ItemType<VikingSquireMinionItem>(), 1);
			}

			if(npc.type == NPCID.KingSlime && !Main.expertMode)
			{
				Player lootRecipient = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)];
				SpawnRoyalArmor(lootRecipient, item => Item.NewItem(npc.getRect(), item, 1));
			}

			if (spawnChance < 0.12f && npc.type == NPCID.ManEater)
			{
				Item.NewItem(npc.getRect(), ItemType<AncientCobaltSquireMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.04f && NPCSets.hornets.Contains(npc.netID))
			{
				Item.NewItem(npc.getRect(), ItemType<AncientCobaltSquireMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.33f && npc.type == NPCID.QueenBee)
			{
				Item.NewItem(npc.getRect(), ItemType<BeeQueenMinionItem>(), 1, prefixGiven: -1);
			}

			if((Main.expertMode || spawnChance < 0.5f) && npc.type == NPCID.SkeletronHead)
			{
				Item.NewItem(npc.getRect(), ItemType<SquireSkullAccessory>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.01f && NPCSets.angryBones.Contains(npc.netID))
			{
				Item.NewItem(npc.getRect(), ItemType<BoneSquireMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.25f && npc.type == NPCID.WallofFlesh)
			{
				Item.NewItem(npc.getRect(), ItemType<BoneSerpentMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.05f && npc.type == NPCID.GiantBat)
			{
				Item.NewItem(npc.getRect(), ItemType<SquireBatAccessory>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.33f && npc.type == NPCID.GoblinSummoner)
			{
				Item.NewItem(npc.getRect(), ItemType<GoblinGunnerMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.10f && npc.type == NPCID.Eyezor)
			{
				Item.NewItem(npc.getRect(), ItemType<SqueyereMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.33f && npc.type == NPCID.Plantera)
			{
				Item.NewItem(npc.getRect(), ItemType<PottedPalMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.03f && NPCSets.blueArmoredBones.Contains(npc.netID))
			{
				Item.NewItem(npc.getRect(), ItemType<ArmoredBoneSquireMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.025f && NPCSets.hellArmoredBones.Contains(npc.netID))
			{
				Item.NewItem(npc.getRect(), ItemType<CharredChimeraMinionItem>(), 1, prefixGiven: -1);
			}
		}

		public override void SetupShop(int type, Chest shop, ref int nextSlot)
		{
			if (type == NPCID.PartyGirl)
			{
				shop.item[nextSlot].SetDefaults(ItemType<BalloonBuddyMinionItem>());
				nextSlot++;
			}

			if(type == NPCID.Clothier)
			{
				shop.item[nextSlot].SetDefaults(ItemID.AncientCloth);
				nextSlot++;
			}
		}
	}

	public class BossBagGlobalItem: GlobalItem
	{

		public override void OpenVanillaBag(string context, Player player, int arg)
		{
			if(arg == ItemID.KingSlimeBossBag)
			{
				LootTable.SpawnRoyalArmor(player, item=>player.QuickSpawnItem(item));
			}
		}

	}
}
