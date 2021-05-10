using Microsoft.Xna.Framework;
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

		// list of NPCs adjacent to the player that may also be a target
		internal List<NPC> playerAdjacentNPCs = new List<NPC>();

		// list of NPCs adjacent to the waypoint that may also be a target
		internal List<NPC> waypointAdjacentNPCs = new List<NPC>();

		// distance to search around the player and waypoint for additional npcs
		static int PLAYER_SEARCH_RADIUS = 600;

		// whether to run LOS calculations on player/waypoint adjacent NPCs
		internal virtual bool UsesPlayerAdjacentNPCs => false;
		internal virtual bool UsesWaypointAdjacentNPCs => false;

		public abstract NPC ChooseTargetFromList(Projectile projectile, List<NPC> possibleTargets);

		public virtual void PreUpdate()
		{
			// no-op by default
		}
		public virtual void PostUpdate()
		{
			frameCounter++;
		}

		public virtual void UpdatePlayerAdjacentNPCs(Player player)
		{
			// TODO may be better to cache at the ModPlayer level
			if(!UsesPlayerAdjacentNPCs)
			{
				return;
			}
			playerAdjacentNPCs.Clear();
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.CanBeChasedBy() || !npc.active)
				{
					continue;
				}
				bool inRange = Vector2.DistanceSquared(npc.Center, player.Center) < PLAYER_SEARCH_RADIUS * PLAYER_SEARCH_RADIUS;
				bool lineOfSight = inRange && Collision.CanHitLine(player.Center, 1, 1, npc.position, npc.width, npc.height);
				if (lineOfSight && inRange)
				{
					playerAdjacentNPCs.Add(npc);
				}
			}
		}

		public virtual void UpdateWaypointAdjacentNPCs(Projectile waypointProj)
		{
			if(!UsesWaypointAdjacentNPCs)
			{
				return;
			}
			waypointAdjacentNPCs.Clear();
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.CanBeChasedBy() || !npc.active)
				{
					continue;
				}
				bool inRange = Vector2.DistanceSquared(npc.Center, waypointProj.Center) < PLAYER_SEARCH_RADIUS * PLAYER_SEARCH_RADIUS;
				bool lineOfSight = inRange && Collision.CanHitLine(waypointProj.Center, 1, 1, npc.position, npc.width, npc.height);
				if (lineOfSight && inRange)
				{
					waypointAdjacentNPCs.Add(npc);
				}
			}

		}

		// whether or not to ignore the waypoint while pursuing this tactic
		// currently only really used for 'defend the player' tactic
		public virtual bool IgnoreWaypoint => false;
	}
}
