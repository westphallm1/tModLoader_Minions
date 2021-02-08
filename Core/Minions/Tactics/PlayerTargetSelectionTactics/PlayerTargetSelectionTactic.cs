using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics
{
	public static class ArgMinExtension
	{
		public static T ArgMin<T>(this List<T> values, Func<T, float> heuristic)
		{
			if(values.Count == 0)
			{
				return default;
			}
			float minHeuristic = float.MaxValue;
			int minHeuristicIdx = 0;
			for(int i = 0; i < values.Count; i++)
			{
				float idxHeuristic = heuristic(values[i]);
				if(idxHeuristic < minHeuristic)
				{
					minHeuristic = idxHeuristic;
					minHeuristicIdx = i;
				}
			}
			return values[minHeuristicIdx];
		}

	}
	/**
	 * Represents a per-player instance of a TargetSelectionTactic. Contains code for selecting a target
	 * based on 
	 */
	public abstract class PlayerTargetSelectionTactic
	{
		// minimum number of consecutive frames to return the same NPC for targetting
		public virtual int TargetCacheFrames => 15;

		// the old standby
		internal int frameCounter;
		public abstract NPC ChooseTargetFromList(Projectile projectile, List<NPC> possibleTargets);

		public virtual void PreUpdate()
		{
			// no-op by default
		}
		public virtual void PostUpdate()
		{
			frameCounter++;
		}
	}
}
