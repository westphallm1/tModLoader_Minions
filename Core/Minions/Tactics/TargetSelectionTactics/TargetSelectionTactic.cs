namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	/// <summary>
	/// Represents the tactic minions will use to attack enemies
	/// </summary>
	public abstract class TargetSelectionTactic
	{
		public string Name => GetType().Name;

		//Unique ID from 0 to DefaultTactic - 1. Do not set it manually
		public byte ID { get; internal set; }

		//Textures associated with this path are cached
		public virtual string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');

		//Cached
		public abstract string DisplayName { get; }

		//Cached
		public abstract string Description { get; }
	}
}
