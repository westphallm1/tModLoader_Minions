using AmuletOfManyMinions.Projectiles.Squires;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static AmuletOfManyMinions.CrossModContent.SummonersShine.Squire_Lacerate_Data;

namespace AmuletOfManyMinions.CrossModContent.SummonersShine
{
	public class Squire_Lacerate_Data
	{
		public List<taggedEnemyCollection> taggedEnemies = new();
		public Action<int> SetDuration;
		public Action forceNetUpdate;
		public class taggedEnemyCollection
		{
			public int netID;
			public int duration;
			public taggedEnemyCollection(int netID, int duration) { this.netID = netID; this.duration = duration; }
		}
	}
	public static class Squire_Lacerate
	{
		const int SCALING_TYPE_ADD = 0;
		const int ROUNDING_TYPE_INT = 1;
		static Tuple<float, int, int, bool>[] GetMinionPower(float mp1, float mp2)
		{
			return new Tuple<float, int, int, bool>[] {
				new(mp1, SCALING_TYPE_ADD, ROUNDING_TYPE_INT, false)
			};
		}
		static Tuple<float, int, int, bool>[] GetRandomMinionPower(Random rand)
		{
			return new Tuple<float, int, int, bool>[] {
				new(rand.Next(0, 4) * 5 + 20, SCALING_TYPE_ADD, ROUNDING_TYPE_INT, false)
			};
		}

		static object GetArbitraryData(float mp1, float mp2)
		{
			return new Squire_Lacerate_Data();
		}

		static void GetCallbacks(float mp1, float mp2, object lacerateData, Action<int> modifyDurationFunc, Action forceNetUpdateFunc)
		{
			Squire_Lacerate_Data data = (Squire_Lacerate_Data)lacerateData;
			data.SetDuration = modifyDurationFunc;
			data.forceNetUpdate = forceNetUpdateFunc;
		}
		static bool? Update(float mp1, float mp2, object lacerateData)
		{
			Squire_Lacerate_Data data = ((Squire_Lacerate_Data)lacerateData);
			data.taggedEnemies.ForEach(i => i.duration--);
			data.taggedEnemies.RemoveAll(i => i.duration <= 0);
			return null;
		}

		static float GetMinionArmorNegationPerc(NPC whipped, float mp1, float mp2, object lacerateData)
		{
			if (((Squire_Lacerate_Data)lacerateData).taggedEnemies.Find(i => i.netID == whipped.netID) != null)
				return mp1 / 100;
			return 0;
		}

		static void OnWhippedEnemy(NPC enemy, float mp1, float mp2, object lacerateData)
		{
			Squire_Lacerate_Data data = ((Squire_Lacerate_Data)lacerateData);
			data.SetDuration(300);
			taggedEnemyCollection col = data.taggedEnemies.Find(i => i.netID == enemy.netID);
			if (col == null)
				data.taggedEnemies.Add(new taggedEnemyCollection(enemy.netID, 300));
			else
				col.duration = 300;
			data.forceNetUpdate();
		}


		static void LoadNetData_extra(BinaryReader reader, float mp1, float mp2, object lacerateData)
		{
			Squire_Lacerate_Data data = ((Squire_Lacerate_Data)lacerateData);
			int count = reader.Read7BitEncodedInt();
			data.taggedEnemies.Clear();
			for (int x = 0; x < count; x++)
			{
				data.taggedEnemies.Add(new taggedEnemyCollection(reader.Read7BitEncodedInt(), reader.Read7BitEncodedInt()));
			}
		}

		static void SaveNetData_extra(ModPacket writer, float mp1, float mp2, object lacerateData)
		{
			Squire_Lacerate_Data data = ((Squire_Lacerate_Data)lacerateData);
			writer.Write7BitEncodedInt(data.taggedEnemies.Count);
			data.taggedEnemies.ForEach(i => {
				writer.Write7BitEncodedInt(i.netID);
				writer.Write7BitEncodedInt(i.duration);
			});
		}

		static bool GetValidForItem(Item testItem, Projectile testProj)
		{
			return testItem.ModItem as SquireMinionItemDetector != null;
		}
		public static void Hook(Mod summonersShine)
		{
			int MODIFY_MODDED_WHIP_SPECIAL = 20;
			Mod AOMM = ModLoader.GetMod("AmuletOfManyMinions");
			summonersShine.Call(MODIFY_MODDED_WHIP_SPECIAL, AOMM, "SquireLacerate", 0, GetMinionPower);
			summonersShine.Call(MODIFY_MODDED_WHIP_SPECIAL, AOMM, "SquireLacerate", 1, GetRandomMinionPower);
			summonersShine.Call(MODIFY_MODDED_WHIP_SPECIAL, AOMM, "SquireLacerate", 2, GetValidForItem);
			summonersShine.Call(MODIFY_MODDED_WHIP_SPECIAL, AOMM, "SquireLacerate", 3, GetArbitraryData);
			summonersShine.Call(MODIFY_MODDED_WHIP_SPECIAL, AOMM, "SquireLacerate", 4, 300);
			summonersShine.Call(MODIFY_MODDED_WHIP_SPECIAL, AOMM, "SquireLacerate", 5, GetCallbacks);
			summonersShine.Call(MODIFY_MODDED_WHIP_SPECIAL, AOMM, "SquireLacerate", 6, Update);
			summonersShine.Call(MODIFY_MODDED_WHIP_SPECIAL, AOMM, "SquireLacerate", 8, GetMinionArmorNegationPerc);
			summonersShine.Call(MODIFY_MODDED_WHIP_SPECIAL, AOMM, "SquireLacerate", 11, OnWhippedEnemy);
			summonersShine.Call(MODIFY_MODDED_WHIP_SPECIAL, AOMM, "SquireLacerate", 13, LoadNetData_extra);
			summonersShine.Call(MODIFY_MODDED_WHIP_SPECIAL, AOMM, "SquireLacerate", 14, SaveNetData_extra);
		}	
	}
}
