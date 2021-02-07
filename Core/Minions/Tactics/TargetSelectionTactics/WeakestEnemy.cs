namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// Enemy with the lowest *max* health
	/// </summary>
	public class WeakestEnemy : TargetSelectionTactic
	{
		public override string DisplayName => "Weakest Enemy";

		public override string Description => "Attack enemy with the [c/FF0000:lowest maximum] health";
	}
}
