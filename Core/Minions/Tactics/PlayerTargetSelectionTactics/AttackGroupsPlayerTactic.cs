using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics
{
	internal class NPCProximityCount
	{
		internal int count;
		internal NPC npc;

		public NPCProximityCount(NPC npc)
		{
			this.count = 0;
			this.npc = npc;
		}
	}
	public class AttackGroupsPlayerTactic : PlayerTargetSelectionTactic
	{

		static float distanceThreshold = 1000f;
		static int maxSampleSize = 30;
		static int npcProximityThreshold = 160;
		static int minCountForGroup = 1;
		// ordered by "group count"
		private List<NPC> npcsInGroups;
		private List<NPCProximityCount> proximityCounts;

		private bool hasBuiltList;

		public AttackGroupsPlayerTactic()
		{
			proximityCounts = new List<NPCProximityCount>();
		}

		private void BuildProximityList(Player player)
		{
			foreach (NPC npc in Main.npc)
			{
				if(npc.CanBeChasedBy() && proximityCounts.Count < maxSampleSize &&
				   Vector2.DistanceSquared(npc.Center, player.Center) < distanceThreshold * distanceThreshold)
				{
					proximityCounts.Add(new NPCProximityCount(npc));
				}
			}

			// O(n^2) on a fixed upper bound, should be fine(?)
			for(int i = 0; i< proximityCounts.Count - 1; i++)
			{
				for(int j = i+1; j < proximityCounts.Count; j++)
				{
					NPCProximityCount pair = proximityCounts[i];
					NPCProximityCount pair2 = proximityCounts[j];
					if(Vector2.DistanceSquared(pair.npc.Center, pair2.npc.Center) < npcProximityThreshold * npcProximityThreshold)
					{
						pair.count++;
						pair2.count++;
					}
				}
			}
			npcsInGroups = proximityCounts
				.Where(pair => pair.count >= minCountForGroup)
				.OrderBy(pair=>pair.count)
				.ThenBy(pair=>pair.npc.whoAmI)
				.Select(pair => pair.npc)
				.ToList();
		}

		public override NPC ChooseTargetFromList(Projectile projectile, List<NPC> possibleTargets)
		{
			if(!hasBuiltList)
			{
				BuildProximityList(Main.player[projectile.owner]);
				hasBuiltList = true;
			}
			// also O(n^2), not a particularly clever implementation
			return possibleTargets
				.OrderBy(npc => -npcsInGroups.IndexOf(npc))
				.ThenBy(npc => Vector2.DistanceSquared(npc.Center, projectile.Center))
				.FirstOrDefault();
		}

		public override void PreUpdate()
		{
			proximityCounts.Clear();
			hasBuiltList = false;
		}
	}
}
