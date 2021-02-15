using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Netcode;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.NullHatchet;
using AmuletOfManyMinions.Projectiles.Minions.VoidKnife;
using AmuletOfManyMinions.Projectiles.Squires;
using AmuletOfManyMinions.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace AmuletOfManyMinions
{
	public class AmuletOfManyMinions : Mod
	{
		internal static ModHotKey CycleTacticHotKey;
		internal static ModHotKey QuickDefendHotKey;
		public override void Load()
		{
			NetHandler.Load();
			NPCSets.Load();
			SquireMinionTypes.Load();
			NecromancerAccessory.Load();
			SquireGlobalProjectile.Load();
			IdleLocationSets.Load();
			TargetSelectionTacticHandler.Load();
			UserInterfaces.Load();

			PathfindingHelper.Initialize();

			CycleTacticHotKey = RegisterHotKey("Cycle Minion Tactic", "K");
			QuickDefendHotKey = RegisterHotKey("Minion Quick Defend", "V");
			if (!Main.dedServ)
			{
				AddEquipTexture(null, EquipType.Legs, "RoyalGown_Legs", "AmuletOfManyMinions/Items/Armor/RoyalArmor/RoyalGown_Legs");
			}
		}

		public override void PostSetupContent()
		{
			NPCSets.Populate();
		}

		public override void Unload()
		{
			NetHandler.Unload();
			NPCSets.Unload();
			SquireMinionTypes.Unload();
			NecromancerAccessory.Unload();
			SquireGlobalProjectile.Unload();
			IdleLocationSets.Unload();
			TargetSelectionTacticHandler.Unload();
			UserInterfaces.Unload();

			CycleTacticHotKey = null;
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
		}

		public override void UpdateUI(GameTime gameTime)
		{
			UserInterfaces.UpdateUI(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			UserInterfaces.ModifyInterfaceLayers(layers);
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			//This should be the only thing in here
			NetHandler.HandlePackets(reader, whoAmI);
		}
	}
}
