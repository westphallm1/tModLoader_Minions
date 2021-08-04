using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.NPCs
{
	/// <summary>
	/// A class containing sets of netIDs for NPCs
	/// </summary>
	public class NPCSets : ModSystem
	{
		public static HashSet<int> hornets;

		public static HashSet<int> angryBones;

		public static HashSet<int> blueArmoredBones;

		public static HashSet<int> hellArmoredBones;

		public static HashSet<int> lunarBosses;

		public static HashSet<int> preHardmodeIceEnemies;

		public static HashSet<int> necromancers;

		public override void OnModLoad()
		{
			hornets = new HashSet<int>();
			angryBones = new HashSet<int>();
			blueArmoredBones = new HashSet<int>();
			hellArmoredBones = new HashSet<int>();
			lunarBosses = new HashSet<int>();
			preHardmodeIceEnemies = new HashSet<int>();
			necromancers = new HashSet<int>();
		}

		public override void PostSetupContent()
		{
			hornets.Add(NPCID.Hornet);

			//Variants
			hornets.Add(NPCID.HornetFatty);
			hornets.Add(NPCID.HornetHoney);
			hornets.Add(NPCID.HornetLeafy);
			hornets.Add(NPCID.HornetSpikey);
			hornets.Add(NPCID.HornetStingy);

			//BIG
			hornets.Add(NPCID.BigHornetFatty);
			hornets.Add(NPCID.BigHornetHoney);
			hornets.Add(NPCID.BigHornetLeafy);
			hornets.Add(NPCID.BigHornetSpikey);
			hornets.Add(NPCID.BigHornetStingy);

			//smol
			hornets.Add(NPCID.LittleHornetFatty);
			hornets.Add(NPCID.LittleHornetHoney);
			hornets.Add(NPCID.LittleHornetLeafy);
			hornets.Add(NPCID.LittleHornetSpikey);
			hornets.Add(NPCID.LittleHornetStingy);

			angryBones.Add(NPCID.AngryBones);
			angryBones.Add(NPCID.AngryBonesBig);
			angryBones.Add(NPCID.AngryBonesBigHelmet);
			angryBones.Add(NPCID.AngryBonesBigMuscle);

			blueArmoredBones.Add(NPCID.BlueArmoredBones);
			blueArmoredBones.Add(NPCID.BlueArmoredBonesMace);
			blueArmoredBones.Add(NPCID.BlueArmoredBonesSword);
			blueArmoredBones.Add(NPCID.BlueArmoredBonesNoPants);

			hellArmoredBones.Add(NPCID.HellArmoredBones);
			hellArmoredBones.Add(NPCID.HellArmoredBonesMace);
			hellArmoredBones.Add(NPCID.HellArmoredBonesSword);
			hellArmoredBones.Add(NPCID.HellArmoredBonesSpikeShield);

			lunarBosses.Add(NPCID.LunarTowerNebula);
			lunarBosses.Add(NPCID.LunarTowerSolar);
			lunarBosses.Add(NPCID.LunarTowerStardust);
			lunarBosses.Add(NPCID.LunarTowerVortex);
			lunarBosses.Add(NPCID.MoonLordCore);

			preHardmodeIceEnemies.Add(NPCID.IceBat);
			preHardmodeIceEnemies.Add(NPCID.SnowFlinx);
			preHardmodeIceEnemies.Add(NPCID.UndeadViking);
			preHardmodeIceEnemies.Add(NPCID.SpikedIceSlime);

			necromancers.Add(NPCID.Necromancer);
			necromancers.Add(NPCID.NecromancerArmored);
		}

		public override void Unload()
		{
			hornets = null;
			angryBones = null;
			blueArmoredBones = null;
			hellArmoredBones = null;
			lunarBosses = null;
			preHardmodeIceEnemies = null;
			necromancers = null;
		}
	}
}
