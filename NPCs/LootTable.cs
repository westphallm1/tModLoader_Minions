using AmuletOfManyMinions.Projectiles.Minions.BalloonBuddy;
using AmuletOfManyMinions.Projectiles.Minions.BeeQueen;
using AmuletOfManyMinions.Projectiles.Minions.BoneSerpent;
using AmuletOfManyMinions.Projectiles.Minions.CharredChimera;
using AmuletOfManyMinions.Projectiles.Minions.GoblinGunner;
using AmuletOfManyMinions.Projectiles.Squires.AncientCobaltSquire;
using AmuletOfManyMinions.Projectiles.Squires.ArmoredBoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.BoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.PottedPal;
using AmuletOfManyMinions.Projectiles.Squires.Squeyere;
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
			float spawnChance = Main.rand.NextFloat();

			if (spawnChance < 0.08f && npc.type == NPCID.ManEater)
			{
				Item.NewItem(npc.getRect(), ItemType<AncientCobaltSquireMinionItem>(), 1, prefixGiven: -1);
			}
			if (spawnChance < 0.03f && npc.TypeName == "Hornet") //hacky, should use a custom set and check npc.netID
			{
				Item.NewItem(npc.getRect(), ItemType<AncientCobaltSquireMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.33f && npc.type == NPCID.QueenBee)
			{
				Item.NewItem(npc.getRect(), ItemType<BeeQueenMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.01f && (npc.type == NPCID.AngryBones ||
			   npc.type == NPCID.AngryBonesBig ||
			   npc.type == NPCID.AngryBonesBigHelmet ||
			   npc.type == NPCID.AngryBonesBigMuscle))
			{
				Item.NewItem(npc.getRect(), ItemType<BoneSquireMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.25f && npc.type == NPCID.WallofFlesh)
			{
				Item.NewItem(npc.getRect(), ItemType<BoneSerpentMinionItem>(), 1, prefixGiven: -1);
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

			if (spawnChance < 0.03f && (npc.type == NPCID.BlueArmoredBones ||
			   npc.type == NPCID.BlueArmoredBonesMace ||
			   npc.type == NPCID.BlueArmoredBonesNoPants ||
			   npc.type == NPCID.BlueArmoredBonesSword))
			{
				Item.NewItem(npc.getRect(), ItemType<ArmoredBoneSquireMinionItem>(), 1, prefixGiven: -1);
			}

			if (spawnChance < 0.025f && npc.TypeName == "Hell Armored Bones") //hacky, should use a custom set and check npc.netID
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
		}
	}
}
