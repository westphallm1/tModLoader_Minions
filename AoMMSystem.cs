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
		public static int SilverBarRecipeGroup { get; private set; }
		public static int GoldBarRecipeGroup { get; private set; }
		public static int EvilBarRecipeGroup { get; private set; }
		public static int EvilWoodSwordRecipeGroup { get; private set; }
		public static int VoidDaggerRecipeGroup { get; private set; }
		public static int StardustDragonRecipeGroup { get; private set; }
		public static int CombatPetChewToyRecipeGroup { get; private set; }

		//Not all localizations are "code-ified", i.e. those pertaining to gear stat changes as they are only used in the lang file itself
		public static LocalizedText AoMMVersionText { get; private set; }
		public static LocalizedText ReplicaText { get; private set; }
		public static LocalizedText ReplicaCommonTooltipText { get; private set; }

		public static LocalizedText RecipeGroupAnyText { get; private set; }
		public static LocalizedText RecipeGroupEitherOrText { get; private set; }

		// The "mold" to combine two texts together
		public static LocalizedText ConcatenateTwoText { get; private set; }

		public static LocalizedText AcceptClientChangesText { get; private set; }

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

			string recipeGroupKey = $"{commonKey}RecipeGroups.";
			RecipeGroupAnyText = Language.GetOrRegister(Mod.GetLocalizationKey($"{recipeGroupKey}Any"));
			RecipeGroupEitherOrText = Language.GetOrRegister(Mod.GetLocalizationKey($"{recipeGroupKey}EitherOr"));

			ConcatenateTwoText = Language.GetOrRegister(Mod.GetLocalizationKey($"{commonKey}ConcatenateTwo"));

			string configCategory = $"Configs.Common.";
			AcceptClientChangesText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"{configCategory}AcceptClientChanges"));
		}

		public override void AddRecipeGroups()
		{
			var any = Language.GetTextValue("LegacyMisc.37");
			RecipeGroup silverGroup = new RecipeGroup(
				() => RecipeGroupAnyText.Format(any, Lang.GetItemNameValue(ItemID.SilverBar)),
				new int[] { ItemID.SilverBar, ItemID.TungstenBar });
			SilverBarRecipeGroup = RecipeGroup.RegisterGroup(nameof(ItemID.SilverBar), silverGroup);

			RecipeGroup goldGroup = new RecipeGroup(
				() => RecipeGroupAnyText.Format(any, Lang.GetItemNameValue(ItemID.GoldBar)),
				new int[] { ItemID.GoldBar, ItemID.PlatinumBar});
			GoldBarRecipeGroup = RecipeGroup.RegisterGroup(nameof(ItemID.GoldBar), goldGroup);

			RecipeGroup evilBarGroup = new RecipeGroup(
				() => RecipeGroupEitherOrText.Format(Lang.GetItemNameValue(ItemID.DemoniteBar), Lang.GetItemNameValue(ItemID.CrimtaneBar)),
				new int[] { ItemID.DemoniteBar, ItemID.CrimtaneBar});
			EvilBarRecipeGroup = RecipeGroup.RegisterGroup(nameof(ItemID.DemoniteBar), evilBarGroup);

			RecipeGroup evilWoodSwordGroup = new RecipeGroup(
				() => RecipeGroupEitherOrText.Format(Lang.GetItemNameValue(ItemID.EbonwoodSword), Lang.GetItemNameValue(ItemID.ShadewoodSword)),
				new int[] { ItemID.EbonwoodSword, ItemID.ShadewoodSword});
			EvilWoodSwordRecipeGroup = RecipeGroup.RegisterGroup("AmuletOfManyMinions:EvilWoodSwords", evilWoodSwordGroup);

			RecipeGroup voidDaggerGroup = new RecipeGroup(
				() => RecipeGroupEitherOrText.Format(ModContent.GetInstance<VoidKnifeMinionItem>().DisplayName, ModContent.GetInstance<NullHatchetMinionItem>().DisplayName),
				new int[] { ModContent.ItemType<VoidKnifeMinionItem>(), ModContent.ItemType<NullHatchetMinionItem>()});
			VoidDaggerRecipeGroup = RecipeGroup.RegisterGroup("AmuletOfManyMinions:VoidDaggers", voidDaggerGroup);

			RecipeGroup stardustDragonGroup = new RecipeGroup(
				() => RecipeGroupAnyText.Format(any, Lang.GetItemNameValue(ItemID.StardustDragonStaff)),
				new int[] { ItemID.StardustDragonStaff, ModContent.ItemType<StardustDragonMinionItem>()});
			StardustDragonRecipeGroup = RecipeGroup.RegisterGroup("AmuletOfManyMinions:StardustDragons", stardustDragonGroup);

			RecipeGroup combatPetChewToyGroup = new RecipeGroup(
				() => RecipeGroupAnyText.Format(any, ModContent.GetInstance<CombatPetChaoticChewToy>().DisplayName),
				new int[] { ModContent.ItemType<CombatPetChaoticChewToy>(),ModContent.ItemType<CombatPetCrimsonChewToy>()  });
			CombatPetChewToyRecipeGroup = RecipeGroup.RegisterGroup("AmuletOfManyMinions:CombatPetChewToys", combatPetChewToyGroup);
		}

		public override void PostAddRecipes()
		{
			CrossMod.PopulateSummonersAssociationBuffSet(Mod);
		}
	}
}
