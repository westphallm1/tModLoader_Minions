using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics
{
	//TODO change to inherit from ModType
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
		public virtual LocalizedText DisplayName =>
			Language.GetOrRegister(ModContent.GetInstance<AmuletOfManyMinions>().GetLocalizationKey($"TargetSelectionTactics.{Name}.DisplayName"));

		//Cached
		public virtual LocalizedText Description =>
			Language.GetOrRegister(ModContent.GetInstance<AmuletOfManyMinions>().GetLocalizationKey($"TargetSelectionTactics.{Name}.Description"));

		public virtual PlayerTargetSelectionTactic CreatePlayerTactic()
		{
			// todo override per subclass
			return new ClosestEnemyToMinionPlayerTactic();
		}
	}
}
