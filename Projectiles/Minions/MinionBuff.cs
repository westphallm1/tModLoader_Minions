using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions
{
	public abstract class MinionBuff : ModBuff
	{
		// create a buffType <-> hash of fully qualified type name map, used for saving/loading
		// tactics buff map.

		// map from hash of fully qualified buff name to runtime bufftype
		public static Dictionary<uint, int> HashToTypeDict;
		// reverse map for quick look up (probably not efficient)
		public static Dictionary<int, uint> TypeToHashDict;
		public static void Load()
		{
			HashToTypeDict = new Dictionary<uint, int>();
			TypeToHashDict = new Dictionary<int, uint>();
		}

		public static void Unload()
		{
			HashToTypeDict = null;
			TypeToHashDict = null;
		}

		// have to use baked-in hash function since String.GetHashCode() isn't
		// consistent across versions or platforms
		// Uses the 32-bit FNV-1a hash
		// https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		private static uint FNV_1A_32Bit_Hash(string input)
		{
			uint hash = 2166136261;
			for(int i = 0; i < input.Length; i++)
			{
				hash ^= input[i];
				hash *= 16777619;
			}
			return hash;
		}

		internal int[] projectileTypes;
		public MinionBuff(params int[] projectileTypes)
		{
			this.projectileTypes = projectileTypes;
		}

		public override void SetDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;

			string qualifiedName = GetType().FullName;
			uint hashCode = FNV_1A_32Bit_Hash(qualifiedName);
			if(HashToTypeDict.ContainsKey(hashCode))
			{
				throw new Exception("Buff typename collision for " + qualifiedName); 
			}
			HashToTypeDict[hashCode] = Type;
			TypeToHashDict[Type] = hashCode;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (projectileTypes.Select(p => player.ownedProjectileCounts[p]).Sum() > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}
