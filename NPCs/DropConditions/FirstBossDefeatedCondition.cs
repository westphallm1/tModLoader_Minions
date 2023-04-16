using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.NPCs.DropConditions
{
	public class FirstBossDefeatedCondition : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public static LocalizedText DescriptionText { get; private set; }

		public FirstBossDefeatedCondition()
		{
			string category = $"DropConditions.";
			DescriptionText ??= Language.GetOrRegister(ModContent.GetInstance<AmuletOfManyMinions>().GetLocalizationKey($"{category}{GetType().Name}.Description"));
		}

		public bool CanDrop(DropAttemptInfo info) => NPC.downedBoss1 || NPC.downedSlimeKing;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => Condition.DownedEarlygameBoss.Description.ToString();
	}
}
