using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Minions.NullHatchet;
using AmuletOfManyMinions.Projectiles.Minions.VoidKnife;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using AmuletOfManyMinions.Items.Accessories.CombatPetAccessories;

namespace AmuletOfManyMinions
{
	public class AoMMSystem : ModSystem
	{
		//Not all localizations are "code-ified", i.e. those pertaining to gear stat changes as they are only used in the lang file itself
		public static LocalizedText AoMMVersionText { get; private set; }
		public static LocalizedText ReplicaText { get; private set; }
		public static LocalizedText ReplicaCommonTooltipText { get; private set; }

		// The "mold" to combine two texts together
		public static LocalizedText ConcatenateTwoText { get; private set; }

		public static LocalizedText AppendAoMMVersion(LocalizedText text)
		{
			return ConcatenateTwoText.WithFormatArgs(text, AoMMVersionText);
		}

		public static LocalizedText PrependReplica(LocalizedText text)
		{
			return ConcatenateTwoText.WithFormatArgs(ReplicaText, text);
		}

		public override void Load()
		{
			LoadCommonLocalization();
		}

		private void LoadCommonLocalization()
		{
			string commonKey = "Common.";
			AoMMVersionText = Language.GetOrRegister(Mod.GetLocalizationKey($"{commonKey}AoMMVersion"));
			ReplicaText = Language.GetOrRegister(Mod.GetLocalizationKey($"{commonKey}Replicas.Replica"));
			ReplicaCommonTooltipText = Language.GetOrRegister(Mod.GetLocalizationKey($"{commonKey}Replicas.CommonTooltip"));

			ConcatenateTwoText = Language.GetOrRegister(Mod.GetLocalizationKey($"{commonKey}ConcatenateTwo"));
		}

		public override void AddRecipeGroups()
		{
			RecipeGroup silverGroup = new RecipeGroup(
				() => Language.GetTextValue("LegacyMisc.37") + " " + Language.GetTextValue("ItemName.SilverBar"),
				new int[] { ItemID.SilverBar, ItemID.TungstenBar });
			RecipeGroup.RegisterGroup("AmuletOfManyMinions:Silvers", silverGroup);

			RecipeGroup goldGroup = new RecipeGroup(
				() => Language.GetTextValue("LegacyMisc.37") + " " + Language.GetTextValue("ItemName.GoldBar"),
				new int[] { ItemID.GoldBar, ItemID.PlatinumBar});
			RecipeGroup.RegisterGroup("AmuletOfManyMinions:Golds", goldGroup);

			RecipeGroup evilBarGroup = new RecipeGroup(
				() => Language.GetTextValue("ItemName.DemoniteBar") + "/" + Language.GetTextValue("ItemName.CrimtaneBar") ,
				new int[] { ItemID.DemoniteBar, ItemID.CrimtaneBar});
			RecipeGroup.RegisterGroup("AmuletOfManyMinions:EvilBars", evilBarGroup);

			RecipeGroup evilWoodSwordGroup = new RecipeGroup(
				() => Language.GetTextValue("ItemName.EbonwoodSword") + "/" + Language.GetTextValue("ItemName.ShadewoodSword") ,
				new int[] { ItemID.EbonwoodSword, ItemID.ShadewoodSword});
			RecipeGroup.RegisterGroup("AmuletOfManyMinions:EvilWoodSwords", evilWoodSwordGroup);

			RecipeGroup voidDaggerGroup = new RecipeGroup(
				() => "Void Dagger/Null Hatchet",
				new int[] { ModContent.ItemType<VoidKnifeMinionItem>(), ModContent.ItemType<NullHatchetMinionItem>()});
			RecipeGroup.RegisterGroup("AmuletOfManyMinions:VoidDaggers", voidDaggerGroup);

			RecipeGroup stardustDragonGroup = new RecipeGroup(
				() => Language.GetTextValue("LegacyMisc.37") + " " + Language.GetTextValue("ItemName.StardustDragonStaff"),
				new int[] { ItemID.StardustDragonStaff, ModContent.ItemType<StardustDragonMinionItem>()});
			RecipeGroup.RegisterGroup("AmuletOfManyMinions:StardustDragons", stardustDragonGroup);

			RecipeGroup combatPetChewToyGroup = new RecipeGroup(
				() => Language.GetTextValue("LegacyMisc.37") + " " + "Chaotic Chew Toy",
				new int[] { ModContent.ItemType<CombatPetChaoticChewToy>(),ModContent.ItemType<CombatPetCrimsonChewToy>()  });
			RecipeGroup.RegisterGroup("AmuletOfManyMinions:CombatPetChewToys", combatPetChewToyGroup);
		}

		public override void PostAddRecipes()
		{
			CrossMod.PopulateSummonersAssociationBuffSet(Mod);
		}
	}
}
