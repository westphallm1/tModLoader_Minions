using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics
{
	public abstract class SimpleHeuristicSelectionTactic : PlayerTargetSelectionTactic
	{
		public abstract float Heuristic(Projectile projectile, NPC npc);
		public override NPC ChooseTargetFromList(Projectile projectile, List<NPC> possibleTargets)
		{
			return possibleTargets.ArgMin(npc=>Heuristic(projectile, npc));
		}
	}

	/**
	 * The default behavior: attack the NPC closest to the minion
	 */
	public class ClosestEnemyToMinionPlayerTactic : SimpleHeuristicSelectionTactic
	{
		public override float Heuristic(Projectile projectile, NPC npc) => (int)Vector2.DistanceSquared(projectile.Center, npc.Center);
	}

	/**
	 * 'Defend' the player by attacking the enemy closest to them
	 */
	public class ClosestEnemyToPlayerPlayerTactic : SimpleHeuristicSelectionTactic
	{
		private NPC closestToPlayer;
		public override float Heuristic(Projectile projectile, NPC npc) => 
			(int)Vector2.DistanceSquared(Main.player[projectile.owner].Center, npc.Center);

		public override bool IgnoreWaypoint => playerAdjacentNPCs.Count > 0;

		internal override bool UsesPlayerAdjacentNPCs => true;

		public override void UpdatePlayerAdjacentNPCs(Player player)
		{
			// find the closest NPC to the player
			base.UpdatePlayerAdjacentNPCs(player);
			if(playerAdjacentNPCs.Count > 0)
			{
				closestToPlayer = playerAdjacentNPCs.ArgMin(npc => Vector2.DistanceSquared(player.Center, npc.Center));
			} else
			{
				closestToPlayer = default;
			}
		}

		public override NPC ChooseTargetFromList(Projectile projectile, List<NPC> possibleTargets)
		{
			NPC target = base.ChooseTargetFromList(projectile, possibleTargets);
			// if there's an enemy directly threatening the player, recall any minions
			if(closestToPlayer != default && closestToPlayer?.whoAmI != target?.whoAmI)
			{
				return default;
			}
			return target;
		}
	}

	/**
	 * Choose the strongest enemy - the one with the highest max health
	 */
	public class StrongestEnemyPlayerTactic : SimpleHeuristicSelectionTactic
	{
		public override float Heuristic(Projectile projectile, NPC npc) => -npc.lifeMax;
	}

	/**
	 * Choose the weakest enemy - the one with the highest max health
	 */
	public class WeakestEnemyPlayerTactic : SimpleHeuristicSelectionTactic
	{
		public override float Heuristic(Projectile projectile, NPC npc) => npc.lifeMax;
	}

	/**
	 * Choose the least damaged enemy - the one with the lowest fraction of missing hitpoints
	 */
	public class LeastDamagedEnemyPlayerTactic : SimpleHeuristicSelectionTactic
	{

		// this one will shift around a lot, so set a higher cache time
		public override int TargetCacheFrames => 90;
		public override float Heuristic(Projectile projectile, NPC npc) => (npc.lifeMax - npc.life) / (float)npc.lifeMax;
	}

	/**
	 * Choose the most damaged enemy - the one with the highest fraction of missing hitpoints 
	 */
	public class MostDamagedEnemyPlayerTactic : SimpleHeuristicSelectionTactic
	{

		// this one will shift around a bit, so set a higher cache time
		public override int TargetCacheFrames => 30;
		public override float Heuristic(Projectile projectile, NPC npc) => (npc.life - npc.lifeMax) / (float)npc.lifeMax;
	}

}
