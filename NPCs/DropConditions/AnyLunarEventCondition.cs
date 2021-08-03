using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace AmuletOfManyMinions.NPCs.DropConditions
{
	public class AnyLunarEventCondition : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public static bool AnyLunarEventActive()
		{
			for (int i = 0; i<Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];

				if (npc.active && NPCSets.lunarBosses.Contains(npc.type))
				{
					return true;
				}
			}
			return false;
		}

		public bool CanDrop(DropAttemptInfo info) => AnyLunarEventActive();
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => "Drops during any Lunar Event";
	}
}
