using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;

namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// Enemy with the highest *max* health
	/// </summary>
	public class StrongestEnemy : TargetSelectionTactic
	{
		public override PlayerTargetSelectionTactic CreatePlayerTactic()
		{
			return new StrongestEnemyPlayerTactic();
		}
	}
}
