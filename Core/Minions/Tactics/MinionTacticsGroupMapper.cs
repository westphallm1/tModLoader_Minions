using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using static AmuletOfManyMinions.Core.Minions.MinionTacticsPlayer;

namespace AmuletOfManyMinions.Core.Minions.Tactics
{
	// Contains a list of static utility methods for persisting minion team assignments to disk/
	// transporting them over the network
	public class MinionTacticsGroupMapper : ModSystem
	{
		// create a buffType <-> hash of fully qualified type name map, used for saving/loading
		// tactics buff map.

		// map from hash of fully qualified buff name to runtime bufftype
		public static Dictionary<uint, int> HashToTypeDict;
		// reverse map for quick look up (probably not efficient)
		public static Dictionary<int, uint> TypeToHashDict;
		public override void OnModLoad()
		{
			HashToTypeDict = new Dictionary<uint, int>();
			TypeToHashDict = new Dictionary<int, uint>();
		}

		public override void Unload()
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

		public static void AddBuffMapping(ModBuff buff)
		{
			string qualifiedName = buff.GetType().FullName;
			uint hashCode = FNV_1A_32Bit_Hash(qualifiedName);
			if(HashToTypeDict.ContainsKey(hashCode))
			{
				throw new Exception("Buff typename collision for " + qualifiedName); 
			}
			HashToTypeDict[hashCode] = buff.Type;
			TypeToHashDict[buff.Type] = hashCode;
		}

		public static void WriteBuffMap(BinaryWriter writer, Dictionary<int, int> tacticsMap)
		{
			List<uint>[] hashes = new List<uint>[TACTICS_GROUPS_COUNT - 1];
			// a little bit verbose, but gets the job done
			for(int i = 0; i < hashes.Length; i++)
			{
				hashes[i] = new List<uint>();
			}
			foreach(var tactic in tacticsMap)
			{
				if(TypeToHashDict.TryGetValue(tactic.Key, out uint hash)) {
					hashes[tactic.Value].Add(hash);
				}
			}
			// <tactics-group-count> byte header with lengths of each hash array
			for(int i = 0; i < hashes.Length; i++)
			{
				writer.Write((byte)hashes[i].Count);
			}
			// 4-n bytes containing the arrays of uint hashes themselves 
			for(int i = 0; i < hashes.Length; i++)
			{
				for(int j = 0; j < hashes[i].Count; j++)
				{
					writer.Write(hashes[i][j]);
				}
			}
		}

		public static void ReadBuffMap(BinaryReader reader, out Dictionary<int, int> outputMap)
		{
			outputMap = new Dictionary<int, int>();
			ReadBuffMap(reader, outputMap);
		}

		public static void ReadBuffMap(BinaryReader reader, Dictionary<int, int> outputMap)
		{
			byte[] buffCounts = new byte[TACTICS_GROUPS_COUNT - 1];
			for(int i = 0; i < buffCounts.Length; i++)
			{
				buffCounts[i] = reader.ReadByte();
			}
			for(int i = 0; i < buffCounts.Length; i++)
			{
				for(int j = 0; j < buffCounts[i]; j++)
				{
					uint hash = reader.ReadUInt32();
					if(HashToTypeDict.TryGetValue(hash, out int type))
					{
						outputMap[type] = i;
					}
				}
			}
		}
	}
}
