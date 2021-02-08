using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;

namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// Enemy with the *lowest current* health
	/// </summary>
	public class MostDamagedEnemy : TargetSelectionTactic
	{
		public override string DisplayName => "Most Damaged Enemy";

		public override string Description => "Attack enemy with the [c/FF0000:lowest current] health";

		public override PlayerTargetSelectionTactic CreatePlayerTactic()
		{
			return new MostDamagedEnemyPlayerTactic();
		}
	}
}
