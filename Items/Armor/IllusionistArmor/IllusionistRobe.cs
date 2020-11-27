using AmuletOfManyMinions.Items.Accessories;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Armor.IllusionistArmor
{
	public abstract class BaseIllusionistRobe : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Illusionist Robe");
			Tooltip.SetDefault("+1 Max Minions\n" +
				"+4% Minion Damage");
		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 18;
			item.value = Item.sellPrice(silver: 2, copper: 50);
			item.rare = ItemRarityID.Orange;
			item.defense = 7;
		}

		public override void UpdateEquip(Player player)
		{
			player.maxMinions += 1;
			player.minionDamageMult += 0.04f;
		}
	}

	[AutoloadEquip(EquipType.Body)]
	public class IllusionistCorruptRobe : BaseIllusionistRobe
	{
	}

	[AutoloadEquip(EquipType.Body)]
	public class IllusionistCrimsonRobe : BaseIllusionistRobe
	{
	}
}