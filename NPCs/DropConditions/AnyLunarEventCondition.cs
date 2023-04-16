using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.NPCs.DropConditions
{
	public class AnyLunarEventCondition : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public static LocalizedText DescriptionText { get; private set; }

		public AnyLunarEventCondition()
		{
			string category = $"DropConditions.";
			DescriptionText ??= Language.GetOrRegister(ModContent.GetInstance<AmuletOfManyMinions>().GetLocalizationKey($"{category}{GetType().Name}.Description"));
		}

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
		public string GetConditionDescription() => DescriptionText.ToString();
	}
}
