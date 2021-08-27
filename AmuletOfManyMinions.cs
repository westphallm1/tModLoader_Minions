using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Netcode;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.Pirate;
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
using Microsoft.Xna.Framework.Graphics;
using AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using System.Linq;
using System;
using AmuletOfManyMinions.Projectiles.Minions.GoblinTechnomancer;

namespace AmuletOfManyMinions
{
	public class AmuletOfManyMinions : Mod
	{
		internal static ModKeybind CycleTacticHotKey;
		internal static ModKeybind CycleTacticsGroupHotKey;
		internal static ModKeybind QuickDefendHotKey;
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
			if (!Main.dedServ)
			{
				//TODO 1.4
				//AddEquipTexture(null, EquipType.Legs, "RoyalGown_Legs", "AmuletOfManyMinions/Items/Armor/RoyalArmor/RoyalGown_Legs");
			}
		}

		public override void PostSetupContent()
		{
			if (ModLoader.TryGetMod("SummonersAssociation", out Mod summonersAssociation))
			{
				if (summonersAssociation.Version >= new Version(0, 4, 6))
				{
					//1. Collect all "EmpoweredMinion" that have a valid CounterType: WORKS FOR ALL EMPOWERED MINIONS (but also includes some "regular" minions which is unintended)
					//var empoweredMinionsWithCounterType = this.GetContent<ModProjectile>().OfType<EmpoweredMinion>().Where(e => e.CounterType > ProjectileID.None).ToList();
					//var counterTypes = empoweredMinionsWithCounterType.Select((e) => e.CounterType).ToHashSet();

					//2. Collect all "CounterMinion": DOES NOT WORK FOR ALL EMPOWERED MINIONS (those that use "regular" minions for counts), but covers all 100% safe ones
					var counterMinions = this.GetContent<ModProjectile>().OfType<CounterMinion>().ToList();
					var counterTypes = counterMinions.ToHashSet();

					//Empowered minion "counter" projectiles should not teleport
					foreach (var counterType in counterTypes)
					{
						summonersAssociation.Call("AddTeleportConditionMinion", counterType);
					}

					//Special non-counter projectiles that should not teleport

					//Return false to prevent a teleport
					//Not specifying one will default to "false"
					summonersAssociation.Call("AddTeleportConditionMinion", ModContent.ProjectileType<SharknadoMinion>(), (Func<Projectile, bool>)SharknadoFunc);

					//Static minions should not teleport

					//Don't include the probes, those can move around
					summonersAssociation.Call("AddTeleportConditionMinion", ModContent.ProjectileType<GoblinTechnomancerMinion>());

					//TODO Add more static minions here
				}

				if (summonersAssociation.Version > new Version(0, 4, 1))
				{
					//Minion weapons that summon more than one type

					summonersAssociation.Call(
						"AddMinionInfo",
						ModContent.ItemType<SpiderMinionItem>(),
						ModContent.BuffType<SpiderMinionBuff>(),
						new List<int> {
						ModContent.ProjectileType<JumperSpiderMinion>(),
						ModContent.ProjectileType<VenomSpiderMinion>(),
						ModContent.ProjectileType<DangerousSpiderMinion>()
						}
					);

					summonersAssociation.Call(
						"AddMinionInfo",
						ModContent.ItemType<TwinsMinionItem>(),
						ModContent.BuffType<TwinsMinionBuff>(),
						new List<int> {
						ModContent.ProjectileType<MiniRetinazerMinion>(),
						ModContent.ProjectileType<MiniSpazmatismMinion>()
						}
					);

					summonersAssociation.Call(
						"AddMinionInfo",
						ModContent.ItemType<PirateMinionItem>(),
						ModContent.BuffType<PirateMinionBuff>(),
						new List<int> {
						ModContent.ProjectileType<PirateMinion>(),
						ModContent.ProjectileType<PirateDeadeyeMinion>(),
						ModContent.ProjectileType<ParrotMinion>(),
						ModContent.ProjectileType<FlyingDutchmanMinion>(),
						}
					);

					summonersAssociation.Call(
						"AddMinionInfo",
						ModContent.ItemType<PygmyMinionItem>(),
						ModContent.BuffType<PygmyMinionBuff>(),
						new List<int> {
						ModContent.ProjectileType<Pygmy1Minion>(),
						ModContent.ProjectileType<Pygmy2Minion>(),
						ModContent.ProjectileType<Pygmy3Minion>(),
						ModContent.ProjectileType<Pygmy4Minion>()
						}
					);

					summonersAssociation.Call(
						"AddMinionInfo",
						ModContent.ItemType<DeadlySphereMinionItem>(),
						ModContent.BuffType<DeadlySphereMinionBuff>(),
						new List<int> {
						ModContent.ProjectileType<DeadlySphereMinion>(),
						ModContent.ProjectileType<DeadlySphereClingerMinion>(),
						ModContent.ProjectileType<DeadlySphereFireMinion>()
						}
					);
				}
			}
		}

		private static bool SharknadoFunc(Projectile p)
		{
			return p.ModProjectile is SharknadoMinion minion && !minion.isBeingUsedAsToken;
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

		public override void AddRecipes()
		{
			// make vanilla minion items craftable from AoMM version and vice versa
			// there is probably a more elegant approach to this
			(int, int)[] pairs = new (int, int)[]
			{
				(ItemID.SlimeStaff,          ModContent.ItemType<BabySlimeMinionItem>()),
				(ItemID.HornetStaff,         ModContent.ItemType<HornetMinionItem>()),
				(ItemID.ImpStaff,            ModContent.ItemType<ImpMinionItem>()),
				(ItemID.SpiderStaff,         ModContent.ItemType<SpiderMinionItem>()),
				(ItemID.PirateStaff,         ModContent.ItemType<PirateMinionItem>()),
				(ItemID.OpticStaff,          ModContent.ItemType<TwinsMinionItem>()),
				(ItemID.PygmyStaff,          ModContent.ItemType<PygmyMinionItem>()),
				(ItemID.RavenStaff,          ModContent.ItemType<RavenMinionItem>()),
				(ItemID.DeadlySphereStaff,   ModContent.ItemType<DeadlySphereMinionItem>()),
				(ItemID.TempestStaff,        ModContent.ItemType<SharknadoMinionItem>()),
				(ItemID.XenoStaff,           ModContent.ItemType<UFOMinionItem>()),
				(ItemID.StardustDragonStaff, ModContent.ItemType<StardustDragonMinionItem>()),
				(ItemID.StardustCellStaff,   ModContent.ItemType<StardustCellMinionItem>()),
			};
			foreach ((int, int) itemPair in pairs)
			{
				for (int i = 0; i < 2; i++)
				{
					int src = i == 0 ? itemPair.Item1 : itemPair.Item2;
					int dst = i == 0 ? itemPair.Item2 : itemPair.Item1;
					Recipe recipe = this.CreateRecipe(dst);
					recipe.AddIngredient(src, 1);
					recipe.AddTile(TileID.DemonAltar);
					recipe.Register();
				}
			}
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			//This should be the only thing in here
			NetHandler.HandlePackets(reader, whoAmI);
		}
	}
}
