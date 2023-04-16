using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.NPCs.DropConditions
{
	public class SlimepireCondition : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public static LocalizedText DescriptionText { get; private set; }

		public SlimepireCondition()
		{
			string category = $"DropConditions.";
			DescriptionText ??= Language.GetOrRegister(ModContent.GetInstance<AmuletOfManyMinions>().GetLocalizationKey($"{category}{GetType().Name}.Description"));
		}

		public bool CanDrop(DropAttemptInfo info)
		{
			// drop from any enemy during a blood moon in hardmode
			if (info.IsInSimulation || !Main.bloodMoon || !Main.hardMode) //IsInSimulation is required when things do not depend on the NPC itself (like world state)
			{
				return false;
			}
			bool badCondition = info.npc.lifeMax <= 1 || info.npc.friendly || info.npc.position.Y > Main.rockLayer * 16.0 || info.npc.value < 1f;
			return !badCondition;
		}
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => DescriptionText.ToString();
	}
}
