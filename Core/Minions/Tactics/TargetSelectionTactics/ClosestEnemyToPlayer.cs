using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;

namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// 'Defend' the player by attacking the closest enemy to them
	/// </summary>
	public class ClosestEnemyToPlayer : TargetSelectionTactic
	{
		public override PlayerTargetSelectionTactic CreatePlayerTactic()
		{
			return new ClosestEnemyToPlayerPlayerTactic();
		}
	}
}
