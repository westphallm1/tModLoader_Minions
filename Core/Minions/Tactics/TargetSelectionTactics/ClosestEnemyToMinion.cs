using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;

namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// Default behavior, minion attacks the enemy that is closest to it
	/// </summary>
	public class ClosestEnemyToMinion : TargetSelectionTactic
	{
		public override PlayerTargetSelectionTactic CreatePlayerTactic()
		{
			return new ClosestEnemyToMinionPlayerTactic();
		}
	}
}
