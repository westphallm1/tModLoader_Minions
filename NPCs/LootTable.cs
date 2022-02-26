using AmuletOfManyMinions.Items.Accessories.SquireSkull;
using AmuletOfManyMinions.Items.Materials;
using AmuletOfManyMinions.Items.WaypointRods;
using AmuletOfManyMinions.NPCs.DropConditions;
using AmuletOfManyMinions.NPCs.DropRules;
using AmuletOfManyMinions.Projectiles.Minions.BalloonBuddy;
using AmuletOfManyMinions.Projectiles.Minions.BalloonMonkey;
using AmuletOfManyMinions.Projectiles.Minions.BeeQueen;
using AmuletOfManyMinions.Projectiles.Minions.BoneSerpent;
using AmuletOfManyMinions.Projectiles.Minions.CharredChimera;
using AmuletOfManyMinions.Projectiles.Minions.ExciteSkull;
using AmuletOfManyMinions.Projectiles.Minions.FishBowl;
using AmuletOfManyMinions.Projectiles.Minions.GoblinGunner;
using AmuletOfManyMinions.Projectiles.Minions.GoblinTechnomancer;
using AmuletOfManyMinions.Projectiles.Minions.MysticPaintbrush;
using AmuletOfManyMinions.Projectiles.Minions.Necromancer;
using AmuletOfManyMinions.Projectiles.Minions.NullHatchet;
using AmuletOfManyMinions.Projectiles.Minions.Rats;
using AmuletOfManyMinions.Projectiles.Minions.Slimepire;
using AmuletOfManyMinions.Projectiles.Minions.SlimeTrain;
using AmuletOfManyMinions.Projectiles.Minions.StarSurfer;
using AmuletOfManyMinions.Projectiles.Minions.StoneCloud;
using AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt;
using AmuletOfManyMinions.Projectiles.Minions.TumbleSheep;
using AmuletOfManyMinions.Projectiles.Minions.VoidKnife;
using AmuletOfManyMinions.Projectiles.Squires.AncientCobaltSquire;
using AmuletOfManyMinions.Projectiles.Squires.ArmoredBoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.BoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.DemonSquire;
using AmuletOfManyMinions.Projectiles.Squires.GoldenRogueSquire;
using AmuletOfManyMinions.Projectiles.Squires.GuideSquire;
using AmuletOfManyMinions.Projectiles.Squires.PottedPal;
using AmuletOfManyMinions.Projectiles.Squires.SkywareSquire;
using AmuletOfManyMinions.Projectiles.Squires.Squeyere;
using AmuletOfManyMinions.Projectiles.Squires.VikingSquire;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.NPCs
{
	public class LootTable : GlobalNPC
	{
		private static void NPCAddExpertScalingRuleCommon(NPCLoot npcLoot, int itemId, int chanceDenominator = 1, int minimumDropped = 1, int maximumDropped = 1, int chanceNumerator = 1, IItemDropRule ruleExpert = null, IItemDropRule ruleNormal = null)
		{
			//Since the conditions are exclusive, only one of them will show up
			IItemDropRule expertRule = new LeadingConditionRule(new Conditions.IsExpert());
			IItemDropRule ruleToAdd = expertRule;
			if (ruleExpert != null)
			{
				ruleToAdd = ruleExpert; //If a rule is specified, use that to add it (Always add the "baseline" rule first)
				expertRule = ruleToAdd.OnSuccess(expertRule);
			}
			expertRule.OnSuccess(new CommonDropWithReroll(itemId, chanceDenominator, minimumDropped, maximumDropped, chanceNumerator));
			npcLoot.Add(ruleToAdd);

			//Vanilla example
			//Conditions.IsPumpkinMoon condition2 = new Conditions.IsPumpkinMoon();
			//Conditions.FromCertainWaveAndAbove condition3 = new Conditions.FromCertainWaveAndAbove(15);

			//LeadingConditionRule entry = new LeadingConditionRule(condition2);
			//LeadingConditionRule ruleToChain = new LeadingConditionRule(condition3);
			//npcLoot.Add(entry).OnSuccess(ruleToChain).OnSuccess(ItemDropRule.Common(1856));

			IItemDropRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
			ruleToAdd = notExpertRule;
			if (ruleNormal != null)
			{
				ruleToAdd = ruleNormal;
				notExpertRule = ruleToAdd.OnSuccess(notExpertRule);
			}
			notExpertRule.OnSuccess(new CommonDrop(itemId, chanceDenominator, minimumDropped, maximumDropped, chanceNumerator));
			npcLoot.Add(ruleToAdd);
		}

		private static void GlobalAddExpertScalingRuleCommon(GlobalLoot globalLoot, int itemId, int chanceDenominator = 1, int minimumDropped = 1, int maximumDropped = 1, int chanceNumerator = 1, IItemDropRule ruleExpert = null, IItemDropRule ruleNormal = null)
		{
			IItemDropRule expertRule = new LeadingConditionRule(new Conditions.IsExpert());
			IItemDropRule ruleToAdd = expertRule;
			if (ruleExpert != null)
			{
				ruleToAdd = ruleExpert; //If a rule is specified, use that to add it (Always add the "baseline" rule first)
				expertRule = ruleToAdd.OnSuccess(expertRule);
			}
			expertRule.OnSuccess(new CommonDropWithReroll(itemId, chanceDenominator, minimumDropped, maximumDropped, chanceNumerator));
			globalLoot.Add(ruleToAdd);

			IItemDropRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
			ruleToAdd = notExpertRule;
			if (ruleNormal != null)
			{
				ruleToAdd = ruleNormal;
				notExpertRule = ruleToAdd.OnSuccess(notExpertRule);
			}
			notExpertRule.OnSuccess(new CommonDrop(itemId, chanceDenominator, minimumDropped, maximumDropped, chanceNumerator));
			globalLoot.Add(ruleToAdd);
		}

		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
			base.ModifyNPCLoot(npc, npcLoot);

			var notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());

			if (npc.type == NPCID.Guide)
			{
				var anyLunars = new AnyLunarEventCondition();
				var anyLunarsRule = new LeadingConditionRule(anyLunars);
				var hair = ItemDropRule.Common(ItemType<GuideHair>(), 1);
				anyLunarsRule.OnSuccess(hair);
				npcLoot.Add(anyLunarsRule);

				var noLunars = new NoLunarEventCondition();
				var noLunarsRule = new LeadingConditionRule(noLunars);
				var anyFirstBoss = new FirstBossDefeatedCondition();
				var squire = ItemDropRule.ByCondition(anyFirstBoss, ItemType<GuideSquireMinionItem>());
				noLunarsRule.OnSuccess(squire);
				npcLoot.Add(noLunarsRule);
			}
			else if (NPCSets.preHardmodeIceEnemies.Contains(npc.netID))
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<VikingSquireMinionItem>(), 20);
			}
			else if (npc.type == NPCID.GraniteFlyer || npc.type == NPCID.GraniteGolem)
			{
				int item = ItemType<GraniteSpark>();
				npcLoot.Add(new DropBasedOnExpertMode(new CommonDrop(item, 1, amountDroppedMaximum: 2),
					new CommonDrop(item, 1, amountDroppedMaximum: 3)));
			}
			else if (npc.type == NPCID.ManEater)
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<AncientCobaltSquireMinionItem>(), 75, chanceNumerator: 3);
			}
			else if (NPCSets.hornets.Contains(npc.netID))
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<AncientCobaltSquireMinionItem>(), 75);
			}
			else if (NPCSets.angryBones.Contains(npc.netID))
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<BoneSquireMinionItem>(), 2000, chanceNumerator: 3);
			}
			else if (npc.type == NPCID.AngryNimbus)
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<StoneCloudMinionItem>(), 25, chanceNumerator: 3);
			}
			else if (npc.type == NPCID.BigMimicHallow)
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<StarSurferMinionItem>(), 3);
			}
			else if (npc.type == NPCID.BigMimicCrimson)
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<NullHatchetMinionItem>(), 3);
			}
			else if (npc.type == NPCID.BigMimicCorruption)
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<VoidKnifeMinionItem>(), 3);
			}
			else if (npc.type == NPCID.GoblinSummoner)
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<GoblinGunnerMinionItem>(), 3);
			}
			else if (npc.type == NPCID.Eyezor)
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<SqueyereMinionItem>(), 10);
			}
			else if (NPCSets.blueArmoredBones.Contains(npc.netID))
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<ArmoredBoneSquireMinionItem>(), 33);
			}
			else if (NPCSets.hellArmoredBones.Contains(npc.netID))
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<CharredChimeraMinionItem>(), 40);
			}
			else if (NPCSets.necromancers.Contains(npc.netID))
			{
				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<NecromancerMinionItem>(), 20);
			}
			else if (npc.type == NPCID.Pumpking)
			{
				var pumpkin = new Conditions.IsPumpkinMoon();
				var wave = new Conditions.FromCertainWaveAndAbove(10);
				IItemDropRule entry = new LeadingConditionRule(pumpkin);
				IItemDropRule ruleToChain = new LeadingConditionRule(wave);
				ruleToChain = entry.OnSuccess(ruleToChain);

				IItemDropRule entry2 = new LeadingConditionRule(pumpkin);
				IItemDropRule ruleToChain2 = new LeadingConditionRule(wave);
				ruleToChain2 = entry2.OnSuccess(ruleToChain2);

				NPCAddExpertScalingRuleCommon(npcLoot, ItemType<GoldenRogueSquireMinionItem>(), 8, ruleExpert: ruleToChain, ruleNormal: ruleToChain2);
				//float pumpkingSpawnChance = 0.0156f + 0.1f * (Main.invasionProgressWave - 10) / 5f;
			}
			else if (npc.type == NPCID.Plantera)
			{
				//Does not use method as it is expert only (no normal mode)
				npcLoot.Add(notExpertRule.OnSuccess(new CommonDropWithReroll(ItemType<PottedPalMinionItem>(), 3)));
			}
			else if (npc.type == NPCID.QueenBee)
			{
				npcLoot.Add(notExpertRule.OnSuccess(new CommonDropWithReroll(ItemType<BeeQueenMinionItem>(), 3)));
			}
			else if (npc.type == NPCID.SkeletronHead)
			{
				npcLoot.Add(notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<BoneWaypointRod>())));
			}
			else if (npc.type == NPCID.SkeletronHead)
			{
				npcLoot.Add(notExpertRule.OnSuccess(new CommonDropWithReroll(ItemType<SquireSkullAccessory>(), 2)));
			}
			else if (npc.type == NPCID.WallofFlesh)
			{
				npcLoot.Add(notExpertRule.OnSuccess(new CommonDropWithReroll(ItemType<BoneSerpentMinionItem>(), 4)));
			}
		}

		public override void ModifyGlobalLoot(GlobalLoot globalLoot)
		{
			//This is used for things like souls of night or yoyos, where there are no clear NPC (or a small subset of NPCs) to give drops to
			var condition = new SlimepireCondition();
			var ruleToChain = new LeadingConditionRule(condition);
			var ruleToChain2 = new LeadingConditionRule(condition);
			GlobalAddExpertScalingRuleCommon(globalLoot, ItemType<SlimepireMinionItem>(), 100, ruleExpert: ruleToChain, ruleNormal: ruleToChain2);
		}

		//Old code kept for reference
		/*
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

			float pumpkingSpawnChance = 0.0156f + 0.1f * (Main.invasionProgressWave - 10) / 5f;
			if (spawnChance < pumpkingSpawnChance && npc.type == NPCID.Pumpking)
			{
				Item.NewItem(npc.getRect(), ItemType<GoldenRogueSquireMinionItem>(), 1, prefixGiven: -1);
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
			}
		}
		*/

		public override void SetupShop(int type, Chest shop, ref int nextSlot)
		{
			if (type == NPCID.PartyGirl && NPC.downedBoss3)
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
					if (spawnChance < 0.67f) { player.QuickSpawnItem(ItemType<BoneSerpentMinionItem>()); }
					break;
				case ItemID.PlanteraBossBag:
					if (spawnChance < 0.67f) { player.QuickSpawnItem(ItemType<PottedPalMinionItem>()); }
					break;
				case ItemID.MoonLordBossBag:
					player.QuickSpawnItem(ItemType<TrueEyeWaypointRod>());
					break;
				// fishing crate chest loot
				case ItemID.WoodenCrate:
				case ItemID.IronCrate:
				case ItemID.WoodenCrateHard:
				case ItemID.IronCrateHard:
					if(spawnChance < 0.03f) { player.QuickSpawnItem(ItemType<TumbleSheepMinionItem>()); }
					else if(spawnChance < 0.06f) { player.QuickSpawnItem(ItemType<RatsMinionItem>()); }
					break;
				case ItemID.JungleFishingCrate:
				case ItemID.JungleFishingCrateHard:
					if(spawnChance < 0.167f) { player.QuickSpawnItem(ItemType<BalloonMonkeyMinionItem>()); }
					break;
				case ItemID.FloatingIslandFishingCrate:
				case ItemID.FloatingIslandFishingCrateHard:
					if(spawnChance < 0.167f) { player.QuickSpawnItem(ItemType<SkywareSquireMinionItem>()); }
					break;
				case ItemID.OceanCrate:
				case ItemID.OceanCrateHard:
					if(spawnChance < 0.167f) { player.QuickSpawnItem(ItemType<FishBowlMinionItem>()); }
					break;
				default:
					break;
			}
			if(context == "lockBox" && spawnChance < 0.167f)
			{
				player.QuickSpawnItem(ItemType<ExciteSkullMinionItem>());
			} else if (context == "obsidianLockBox" && spawnChance < 0.167f)
			{
				player.QuickSpawnItem(ItemType<DemonSquireMinionItem>());
			}
		}

	}
}
