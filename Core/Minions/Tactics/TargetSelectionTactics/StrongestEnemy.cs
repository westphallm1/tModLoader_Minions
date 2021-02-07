namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// Enemy with the highest *max* health
	/// </summary>
	public class StrongestEnemy : TargetSelectionTactic
	{
		public override string DisplayName => "Strongest Enemy";

		public override string Description => "Attack enemy with the [c/FF0000:highest maximum] health";
	}
}
