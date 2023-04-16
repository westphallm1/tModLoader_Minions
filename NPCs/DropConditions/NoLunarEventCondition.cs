using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.NPCs.DropConditions
{
	public class NoLunarEventCondition : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public static LocalizedText DescriptionText { get; private set; }

		public NoLunarEventCondition()
		{
			string category = $"DropConditions.";
			DescriptionText ??= Language.GetOrRegister(ModContent.GetInstance<AmuletOfManyMinions>().GetLocalizationKey($"{category}{GetType().Name}.Description"));
		}

		public bool CanDrop(DropAttemptInfo info) => !AnyLunarEventCondition.AnyLunarEventActive();
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => DescriptionText.ToString();
	}
}
