namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// Enemy with the *highest current* health
	/// </summary>
	public class LeastDamagedEnemy : TargetSelectionTactic
	{
		public override string DisplayName => "Least Damaged Enemy";

		public override string Description => "Attack enemy with the [c/FF0000:highest current] health";
	}
}
