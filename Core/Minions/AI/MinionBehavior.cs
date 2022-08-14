using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;
using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.AI
{
	public interface IMinion
	{
		Projectile Projectile { get; }

		Player Player { get; }

		int BuffId { get; }

		public void OnSpawn()
		{

		}
		public bool ShouldIgnoreNPC(NPC npc)
		{
			return !npc.CanBeChasedBy();
		}
	}


	internal class MinionBehavior
	{
		public Player Player { get; set; }

		public int? TargetNPCIndex { get; set; }
		public int TargetNPCCacheFrames { get; set; }


		public bool UseBeacon { get; set; } = true;

		public bool UsingBeacon { get; set; } = false;

		public PlayerTargetSelectionTactic CurrentTactic { get; set; }
		public PlayerTargetSelectionTactic PreviousTactic { get; set; }

		public bool Spawned { get; set; }
		internal IMinion Minion { get; set; }

		public MinionBehavior(IMinion minion)
		{
			Minion = minion;
		}

		internal Projectile Projectile => Minion.Projectile;


		public Vector2? PlayerTargetPosition(float maxRange, Vector2? centeredOn = null, float noLOSRange = 0, Vector2? losCenter = null)
		{
			MinionTacticsPlayer tacticsPlayer = Player.GetModPlayer<MinionTacticsPlayer>();
			if (tacticsPlayer.IgnoreVanillaMinionTarget > 0 && tacticsPlayer.SelectedTactic != TargetSelectionTacticHandler.GetTactic<ClosestEnemyToMinion>())
			{
				return null;
			}
			Vector2 center = centeredOn ?? Projectile.Center;
			Vector2 losCenterVector = losCenter ?? Projectile.Center;
			if (Player.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[Player.MinionAttackTargetNPC];
				if (Minion.ShouldIgnoreNPC(npc))
				{
					return null;
				}
				float distance = Vector2.Distance(npc.Center, center);
				if (distance < noLOSRange || distance < maxRange &&
					Collision.CanHitLine(losCenterVector, 1, 1, npc.position, npc.width, npc.height))
				{
					TargetNPCIndex = Player.MinionAttackTargetNPC;
					return npc.Center;
				}
			}
			return null;
		}

		public Vector2? PlayerAnyTargetPosition(float maxRange, Vector2? centeredOn = null)
		{
			Vector2 center = centeredOn ?? Projectile.Center;
			if (Player.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[Player.MinionAttackTargetNPC];
				float distance = Vector2.Distance(npc.Center, center);
				bool lineOfSight = Collision.CanHitLine(center, 1, 1, npc.Center, 1, 1);
				if (distance < maxRange && lineOfSight)
				{
					TargetNPCIndex = Player.MinionAttackTargetNPC;
					return npc.Center;
				}
			}
			return null;
		}

		public Vector2? SelectedEnemyInRange(float maxRange, float noLOSRange = 0, bool maxRangeFromPlayer = true, Vector2? losCenter = null)
		{
			Vector2 losCenterVector = losCenter ?? Projectile.Center;
			MinionTacticsPlayer tacticsPlayer = Player.GetModPlayer<MinionTacticsPlayer>();
			MinionPathfindingPlayer pathfindingPlayer = Player.GetModPlayer<MinionPathfindingPlayer>();

			// Make sure not to cache the target if the target selection tactic changes
			CurrentTactic = tacticsPlayer.GetTacticForMinion(Minion);
			bool tacticDidChange = CurrentTactic != PreviousTactic;
			PreviousTactic = CurrentTactic;

			// to cut back on Line-of-Sight computations, always chase the same NPC for some number of frames once one has been found
			if (!tacticDidChange && TargetNPCIndex is int idx && Main.npc[idx].active && TargetNPCCacheFrames++ < CurrentTactic.TargetCacheFrames)
			{
				return Main.npc[idx].Center;
			}
			Vector2 rangeCheckCenter;
			BlockAwarePathfinder pathfinder = pathfindingPlayer.GetPathfinder(Minion);
			Vector2 waypointPos = pathfindingPlayer.GetWaypointPosition(Minion);
			if (!maxRangeFromPlayer)
			{
				rangeCheckCenter = Projectile.Center;
			}
			else if (!pathfinder.InProgress() && pathfinder.searchSucceeded && waypointPos != default)
			{
				rangeCheckCenter = waypointPos;
			}
			else
			{
				rangeCheckCenter = Player.Center;
			}
			List<NPC> possibleTargets = new List<NPC>();
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (Minion.ShouldIgnoreNPC(npc))
				{
					continue;
				}
				bool inRange = Vector2.DistanceSquared(npc.Center, rangeCheckCenter) < maxRange * maxRange;
				bool inNoLOSRange = Vector2.DistanceSquared(npc.Center, Player.Center) < noLOSRange * noLOSRange;
				bool lineOfSight = inNoLOSRange || inRange && Collision.CanHitLine(losCenterVector, 1, 1, npc.position, npc.width, npc.height);
				if (inNoLOSRange || lineOfSight && inRange)
				{
					possibleTargets.Add(npc);
				}
			}
			int tacticsGroup = tacticsPlayer.GetGroupForMinion(Minion);
			NPC chosen = CurrentTactic.ChooseTargetNPC(Projectile, tacticsGroup, possibleTargets);
			if (chosen != default)
			{
				TargetNPCIndex = chosen.whoAmI;
				TargetNPCCacheFrames = 0;
				return chosen.Center;
			}
			else
			{
				return null;
			}
		}

		// A simpler version of SelectedEnemyInRange that doesn't require any tactics/teams stuff
		public NPC GetClosestEnemyToPosition(Vector2 position, float searchRange, bool requireLOS = true)
		{
			return GetClosestEnemyToPosition(position, searchRange, Minion.ShouldIgnoreNPC, requireLOS);
		}

		public static NPC GetClosestEnemyToPosition(Vector2 position, float searchRange, Func<NPC, bool> shouldIgnore = null, bool requireLOS = true)
		{
			float minDist = float.MaxValue;
			NPC closest = null;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (shouldIgnore?.Invoke(npc) ?? !npc.active || !npc.CanBeChasedBy())
				{
					continue;
				}
				float distanceSq = Vector2.DistanceSquared(npc.Center, position);
				bool inRange = distanceSq < searchRange * searchRange;
				bool lineOfSight = !requireLOS || inRange && Collision.CanHitLine(position, 1, 1, npc.position, npc.width, npc.height);
				if (lineOfSight && inRange && distanceSq < minDist)
				{
					minDist = distanceSq;
					closest = npc;
				}
			}
			return closest;
		}

		public Vector2? AnyEnemyInRange(float maxRange, Vector2? centeredOn = null, bool noLOS = false)
		{
			Vector2 center = centeredOn ?? Projectile.Center;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.CanBeChasedBy())
				{
					continue;
				}
				// 
				bool inRange = Vector2.Distance(center, npc.Center) < maxRange;
				bool lineOfSight = noLOS || inRange && Collision.CanHitLine(center, 1, 1, npc.Center, 1, 1);
				if (lineOfSight && inRange)
				{
					TargetNPCIndex = npc.whoAmI;
					return npc.Center;
				}
			}
			return null;
		}
	}
}
