using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace AmuletOfManyMinions.NPCs.DropConditions
{
	public class FirstBossDefeatedCondition : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => NPC.downedBoss1 || NPC.downedSlimeKing;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => "Drops after first defeated boss";
	}
}
