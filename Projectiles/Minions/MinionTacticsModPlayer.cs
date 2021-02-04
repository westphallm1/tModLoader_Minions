using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmuletOfManyMinions.Projectiles.Minions
{
	internal enum TargetSelectionTactic
	{
		/**
		 * Default behavior, minion attacks the enemy that is closest to it
		 */
		ClosestEnemyToMinion,

		/**
		 * 'Defend' the player by attacking the closest enemy to them
		 */
		ClosestEnemyToPlayer,

		/**
		 * enemy with the highest *max* health
		 */
		StrongestEnemy,

		/**
		 * Enemy with the lowest *max* health
		 */
		WeakestEnemy,

		/**
		 * Enemy with the highest *current* health
		 */
		LeastDamagedEnemy,

		/**
		 * Enemy with the lowest *current* health
		 */
		MostDamagedEnemy,

		/**
		 * Each minion attacks a different enemy
		 */
		SpreadOut,

		/**
		 * Attack enemies that are close to each other
		 */
		AttackGroups

	}
	public class MinionTacticsModPlayer
	{
		internal TargetSelectionTactic selectionTactic = TargetSelectionTactic.ClosestEnemyToMinion;

		// TODO
	}
}
