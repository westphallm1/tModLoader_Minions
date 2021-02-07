namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// 'Defend' the player by attacking the closest enemy to them
	/// </summary>
	public class ClosestEnemyToPlayer : TargetSelectionTactic
	{
		public override string DisplayName => "Closest Enemy To Player";

		public override string Description => "Attack closest enemy to player";
	}
}
