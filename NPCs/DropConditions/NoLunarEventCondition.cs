using Terraria.GameContent.ItemDropRules;

namespace AmuletOfManyMinions.NPCs.DropConditions
{
	public class NoLunarEventCondition : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => !AnyLunarEventCondition.AnyLunarEventActive();
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => "Does drop during any Lunar Events";
	}
}
