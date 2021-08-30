using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using log4net;

namespace AmuletOfManyMinions.Core
{
	class CrossMod : ModSystem
	{
		private static HashSet<int> SABuffIds;

		public override void Load()
		{
			SABuffIds = new HashSet<int>();
		}

		public override void OnWorldLoad()
		{
			// not sure if this is the best place to do this
			if(AmuletOfManyMinions.SummonersAssociationLoaded)
			{
				PopulateSABuffList();
			}
			base.OnWorldLoad();
		}
		public override void Unload()
		{
			SABuffIds = null;
		}


		internal void PopulateSABuffList()
		{
			var SAmodels = (List<SummonersAssociation.Models.MinionModel>)
				typeof(SummonersAssociation.SummonersAssociation)
				.GetField("SupportedMinions", BindingFlags.NonPublic | BindingFlags.Static)
				.GetValue(null);
			for(int i = 0; i < SAmodels.Count; i++)
			{
				Mod.Logger.Info((int)SAmodels[i].BuffID);
				SABuffIds.Add((int)SAmodels[i].BuffID);
			}
		}

		/**
		 * Get a more accurate minion variety count using Summoner's Association's list of minion mondels
		 * (through a reflective call that bypasses its visibility)
		 */
		internal static int GetSAVarietyCount(ILog logger)
		{
			int uniqueCount = 0;
			int[] buffTypes = Main.player[Main.myPlayer].buffType;
			for(int i = 0; i < buffTypes.Length; i++)
			{
				if(SABuffIds.Contains(buffTypes[i]))
				{
					uniqueCount++;
				}
			}
			return uniqueCount;
		}
	}
}
