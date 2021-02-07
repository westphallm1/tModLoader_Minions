namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// Default behavior, minion attacks the enemy that is closest to it
	/// </summary>
	public class ClosestEnemyToMinion : TargetSelectionTactic
	{
		public override string DisplayName => "Closest Enemy To Minion";

		public override string Description => "Attack closest enemy to minion";
	}
}
