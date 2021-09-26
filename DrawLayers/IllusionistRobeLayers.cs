using AmuletOfManyMinions.Items.Armor.IllusionistArmor;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.DrawLayers
{
	public abstract class IllusionistRobeLayer : BaseRobeLayer
	{
		public abstract string ItemName { get; }

		public override string TexturePath => $"AmuletOfManyMinions/Items/Armor/IllusionistArmor/{ItemName}_Legs";

		protected override bool VisibleWithBody(int bodySlot)
		{
			return bodySlot == Mod.GetEquipSlot(ItemName, EquipType.Body);
		}
	}

	public class CorruptIllusionistRobeLayer : IllusionistRobeLayer
	{
		public override string ItemName => nameof(IllusionistCorruptRobe);
	}

	public class CrimsonIllusionistRobeLayer : IllusionistRobeLayer
	{
		public override string ItemName => nameof(IllusionistCrimsonRobe);
	}
}
