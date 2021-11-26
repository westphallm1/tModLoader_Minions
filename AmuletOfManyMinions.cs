using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Netcode;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Minions.NullHatchet;
using AmuletOfManyMinions.Projectiles.Minions.VoidKnife;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;

namespace AmuletOfManyMinions
{
	public class AmuletOfManyMinions : Mod
	{
		internal static ModKeybind CycleTacticHotKey;
		internal static ModKeybind CycleTacticsGroupHotKey;
		internal static ModKeybind QuickDefendHotKey;
		internal static bool SummonersAssociationLoaded;
		public override void Load()
		{
			NetHandler.Load();
			TargetSelectionTacticHandler.Load();
			LandChunkConfigs.Load();
			SpriteCompositionManager.Load();
			CritterConfigs.Load();

			CycleTacticHotKey = KeybindLoader.RegisterKeybind(this, "Cycle Minion Tactic", "K");
			CycleTacticsGroupHotKey = KeybindLoader.RegisterKeybind(this, "Cycle Tactics Group", "L");
			QuickDefendHotKey = KeybindLoader.RegisterKeybind(this, "Minion Quick Defend", "V");
		}

		public override void PostSetupContent()
		{
			CrossMod.AddSummonersAssociationMetadata(this);
			// add Journey Mode support to any item which doesn't explicitly reference it
			var catalog = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId;
			IEnumerable<ModItem> items = GetContent<ModItem>().Where(i=>!catalog.ContainsKey(i.Type));
			foreach(var item in items)
			{
				catalog[item.Type] = 1;
			}
			
		}

		public override void Unload()
		{
			NetHandler.Unload();
			TargetSelectionTacticHandler.Unload();
			LandChunkConfigs.Unload();
			SpriteCompositionManager.Unload();
			CritterConfigs.Unload();

			CycleTacticHotKey = null;
			CycleTacticsGroupHotKey = null;
			QuickDefendHotKey = null;
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
		}

		public override void PostAddRecipes()
		{
			CrossMod.PopulateSummonersAssociationBuffSet(this);
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			//This should be the only thing in here
			NetHandler.HandlePackets(reader, whoAmI);
		}
	}
}
