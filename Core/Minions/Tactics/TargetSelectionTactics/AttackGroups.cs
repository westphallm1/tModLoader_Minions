namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// Attack enemies that are close to each other
	/// </summary>
	public class AttackGroups : TargetSelectionTactic
	{
		public override string DisplayName => "Attack Groups";

		public override string Description => "Attack enemies that are close to each other";
	}
}
