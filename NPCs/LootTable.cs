using AmuletOfManyMinions.Items.Accessories.SquireBat;
using AmuletOfManyMinions.Items.Accessories.SquireSkull;
using AmuletOfManyMinions.Items.Materials;
using AmuletOfManyMinions.Items.WaypointRods;
using AmuletOfManyMinions.Projectiles.Minions.BalloonBuddy;
using AmuletOfManyMinions.Projectiles.Minions.BeeQueen;
using AmuletOfManyMinions.Projectiles.Minions.BoneSerpent;
using AmuletOfManyMinions.Projectiles.Minions.CharredChimera;
using AmuletOfManyMinions.Projectiles.Minions.GoblinGunner;
using AmuletOfManyMinions.Projectiles.Minions.GoblinTechnomancer;
using AmuletOfManyMinions.Projectiles.Minions.MysticPaintbrush;
using AmuletOfManyMinions.Projectiles.Minions.Necromancer;
using AmuletOfManyMinions.Projectiles.Minions.NullHatchet;
using AmuletOfManyMinions.Projectiles.Minions.Slimepire;
using AmuletOfManyMinions.Projectiles.Minions.SlimeTrain;
using AmuletOfManyMinions.Projectiles.Minions.StarSurfer;
using AmuletOfManyMinions.Projectiles.Minions.StoneCloud;
using AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt;
using AmuletOfManyMinions.Projectiles.Minions.VoidKnife;
using AmuletOfManyMinions.Projectiles.Squires.AncientCobaltSquire;
using AmuletOfManyMinions.Projectiles.Squires.ArmoredBoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.BoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.GuideSquire;
using AmuletOfManyMinions.Projectiles.Squires.PottedPal;
using AmuletOfManyMinions.Projectiles.Squires.Squeyere;
using AmuletOfManyMinions.Projectiles.Squires.VikingSquire;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.NPCs
{
	class LootTable : GlobalNPC
	{
		public override void NPCLoot(NPC npc)
		{
			base.NPCLoot(npc);
			// make all spawn chances more likely on expert mode
			float spawnChance = Main.rand.NextFloat() * (Main.expertMode ? 0.67f : 1);

			if (npc.type == NPCID.Guide)
			{
				if (Main.npc.Any(n => n.active && NPCSets.lunarBosses.Contains(n.type)))
				{
					Item.NewItem(npc.getRect(), ItemType<GuideHair>(), 1);
				}
				else if (NPC.downedBoss1 || NPC.downedSlimeKing)
				{
					Item.NewItem(npc.getRect(), ItemType<GuideSquireMinionItem>(), 1, prefixGiven: -1);
				}
			}

			if (spawnChance < 0.05f && NPCSets.preHardmodeIceEnemies.Contains(npc.netID))
			{
				Item.NewItem(npc.getRect(), ItemType<VikingSquireMinionItem>(), 1);
			}

			if(npc.type == NPCID.GraniteFlyer || npc.type == NPCID.GraniteGolem)
			{
				int amount = Main.rand.Next(1, Main.expertMode ? 4 : 3);
				Item.NewItem(npc.getRect(), ItemType<GraniteSpark>(), amount);
			}

			if (spawnChance < 0.12f && npc.type == NPCID.ManEater)
			{
				Item.NewItem(npc.getRect(), ItemType<AncientCobaltSquireMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.04f && NPCSets.hornets.Contains(npc.netID))
			{
				Item.NewItem(npc.getRect(), ItemType<AncientCobaltSquireMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.015f && NPCSets.angryBones.Contains(npc.netID))
			{
				Item.NewItem(npc.getRect(), ItemType<BoneSquireMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.12f && npc.type == NPCID.AngryNimbus)
			{
				Item.NewItem(npc.getRect(), ItemType<StoneCloudMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.05f && npc.type == NPCID.GiantBat)
			{
				Item.NewItem(npc.getRect(), ItemType<SquireBatAccessory>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.33f && npc.type == NPCID.BigMimicHallow)
			{
				Item.NewItem(npc.getRect(), ItemType<StarSurferMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.33f && npc.type == NPCID.BigMimicCrimson)
			{
				Item.NewItem(npc.getRect(), ItemType<NullHatchetMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.33f && npc.type == NPCID.BigMimicCorruption)
			{
				Item.NewItem(npc.getRect(), ItemType<VoidKnifeMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.33f && npc.type == NPCID.GoblinSummoner)
			{
				Item.NewItem(npc.getRect(), ItemType<GoblinGunnerMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.10f && npc.type == NPCID.Eyezor)
			{
				Item.NewItem(npc.getRect(), ItemType<SqueyereMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.03f && NPCSets.blueArmoredBones.Contains(npc.netID))
			{
				Item.NewItem(npc.getRect(), ItemType<ArmoredBoneSquireMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.025f && NPCSets.hellArmoredBones.Contains(npc.netID))
			{
				Item.NewItem(npc.getRect(), ItemType<CharredChimeraMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.05f && NPCSets.necromancers.Contains(npc.netID))
			{
				Item.NewItem(npc.getRect(), ItemType<NecromancerMinionItem>(), 1, prefixGiven: -1);
			}

			// drop from any enemy during a blood moon in pre-hardmode
			if (spawnChance < 0.01f && npc.CanBeChasedBy() && !npc.SpawnedFromStatue && Main.bloodMoon && Main.hardMode)
			{
				Item.NewItem(npc.getRect(), ItemType<SlimepireMinionItem>(), 1, prefixGiven: -1);
			}

			if (!Main.expertMode)
			{
				if (spawnChance < 0.33f && npc.type == NPCID.Plantera)
				{
					Item.NewItem(npc.getRect(), ItemType<PottedPalMinionItem>(), 1, prefixGiven: -1);
				}

				if (spawnChance < 0.33f && npc.type == NPCID.QueenBee)
				{
					Item.NewItem(npc.getRect(), ItemType<BeeQueenMinionItem>(), 1, prefixGiven: -1);
				}

				if (npc.type == NPCID.SkeletronHead)
				{
					Item.NewItem(npc.getRect(), ItemType<BoneWaypointRod>(), 1);
				}

				if (spawnChance < 0.5f && npc.type == NPCID.SkeletronHead)
				{
					Item.NewItem(npc.getRect(), ItemType<SquireSkullAccessory>(), 1, prefixGiven: -1);
				}

				if (spawnChance < 0.25f && npc.type == NPCID.WallofFlesh)
				{
					Item.NewItem(npc.getRect(), ItemType<BoneSerpentMinionItem>(), 1, prefixGiven: -1);
				}
				if(spawnChance < 0.11f && npc.type == NPCID.MoonLordCore)
				{
					Item.NewItem(npc.getRect(), ItemType<SlimeTrainMinionItem>(), 1, prefixGiven: -1);
				} else if (spawnChance < 0.22f && npc.type == NPCID.MoonLordCore)
				{
					Item.NewItem(npc.getRect(), ItemType<TerrarianEntMinionItem>(), 1, prefixGiven: -1);
				}
			}
		}

		public override void SetupShop(int type, Chest shop, ref int nextSlot)
		{
			if (type == NPCID.PartyGirl)
			{
				shop.item[nextSlot].SetDefaults(ItemType<BalloonBuddyMinionItem>());
				nextSlot++;
			}

			if (type == NPCID.Clothier)
			{
				shop.item[nextSlot].SetDefaults(ItemID.AncientCloth);
				nextSlot++;
			}

			if (type == NPCID.Painter && NPC.downedBoss1)
			{
				shop.item[nextSlot].SetDefaults(ItemType<MysticPaintbrushMinionItem>());
				nextSlot++;
			}

			if (type == NPCID.GoblinTinkerer && NPC.downedMartians)
			{
				shop.item[nextSlot].SetDefaults(ItemType<GoblinTechnomancerMinionItem>());
				nextSlot++;
			}
		}
	}

	public class BossBagGlobalItem : GlobalItem
	{

		public override void OpenVanillaBag(string context, Player player, int arg)
		{
			float spawnChance = Main.rand.NextFloat();
			switch (arg)
			{
				case ItemID.QueenBeeBossBag:
					if (spawnChance < 0.67f)
					{
						player.QuickSpawnItem(ItemType<BeeQueenMinionItem>());
					}
					break;
				case ItemID.SkeletronBossBag:
					player.QuickSpawnItem(ItemType<SquireSkullAccessory>());
					player.QuickSpawnItem(ItemType<BoneWaypointRod>());
					break;
				case ItemID.WallOfFleshBossBag:
					if (spawnChance < 0.67f)
					{
						player.QuickSpawnItem(ItemType<BoneSerpentMinionItem>());
					}
					break;
				case ItemID.PlanteraBossBag:
					if (spawnChance < 0.67f)
					{
						player.QuickSpawnItem(ItemType<PottedPalMinionItem>());
					}
					break;
				case ItemID.MoonLordBossBag:
					player.QuickSpawnItem(ItemType<TrueEyeWaypointRod>());
					if(spawnChance < 0.11f)
					{
						player.QuickSpawnItem(ItemType<SlimeTrainMinionItem>());
					} else if (spawnChance < 0.22f)
					{
						player.QuickSpawnItem(ItemType<TerrarianEntMinionItem>());
					}
					break;
				default:
					break;
			}
		}

	}
}
