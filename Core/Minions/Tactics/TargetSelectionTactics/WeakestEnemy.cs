using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;

namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// Enemy with the lowest *max* health
	/// </summary>
	public class WeakestEnemy : TargetSelectionTactic
	{
		public override PlayerTargetSelectionTactic CreatePlayerTactic()
		{
			return new WeakestEnemyPlayerTactic();
		}
	}
}
