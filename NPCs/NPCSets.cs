using Terraria.ID;
using System.Collections.Generic;

namespace AmuletOfManyMinions.NPCs
{
	/// <summary>
	/// A class containing sets of netIDs for NPCs
	/// </summary>
	public static class NPCSets
	{
		public static HashSet<int> hornets;

		public static HashSet<int> angryBones;

		public static HashSet<int> blueArmoredBones;

		public static HashSet<int> hellArmoredBones;

		public static void Load()
		{
			hornets = new HashSet<int>();
			angryBones = new HashSet<int>();
			blueArmoredBones = new HashSet<int>();
			hellArmoredBones = new HashSet<int>();
		}

		public static void Populate()
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
		}

		public static void Unload()
		{
			hornets = null;
			angryBones = null;
			blueArmoredBones = null;
			hellArmoredBones = null;
		}
	}
}
