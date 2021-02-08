using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;

namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// Each minion attacks a different enemy
	/// </summary>
	public class SpreadOut : TargetSelectionTactic
	{
		public override string DisplayName => "Spread Out";

		public override string Description => "Each minion attacks a different enemy if possible";

		public override PlayerTargetSelectionTactic CreatePlayerTactic()
		{
			return new SpreadOutPlayerTactic();
		}
	}
}
