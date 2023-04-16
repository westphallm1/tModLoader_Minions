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
using AmuletOfManyMinions.Items.Accessories.CombatPetAccessories;
using AmuletOfManyMinions.CrossModSystem;
using System;
using AmuletOfManyMinions.CrossModClient.SummonersShine;

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
			LandChunkConfigs.Load();
			SpriteCompositionManager.Load();
			CritterConfigs.Load();

			CycleTacticHotKey = KeybindLoader.RegisterKeybind(this, "CycleMinionTactic", "K");
			CycleTacticsGroupHotKey = KeybindLoader.RegisterKeybind(this, "CycleTacticsGroup", "L");
			QuickDefendHotKey = KeybindLoader.RegisterKeybind(this, "MinionQuickDefend", "V");
		}

		public override void PostSetupContent()
		{
			CrossMod.AddSummonersAssociationMetadata(this);
			CrossModSetup.AddSummonersShineMetadata(this);
			// add Journey Mode support to any item which doesn't explicitly reference it
			var catalog = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId;
			IEnumerable<ModItem> items = GetContent<ModItem>().Where(i=>!catalog.ContainsKey(i.Type));
			foreach(var item in items)
			{
				catalog[item.Type] = 1;
			}
			
		}

		public override object Call(params object[] args)
		{
			try
			{
				return ModCallHandler.HandleCall(args);
			} catch(Exception e)
			{
				Logger.Error("Exception in mod.Call", e);
				return default;
			}
		}
		public override void Unload()
		{
			NetHandler.Unload();
			LandChunkConfigs.Unload();
			SpriteCompositionManager.Unload();
			CritterConfigs.Unload();

			CycleTacticHotKey = null;
			CycleTacticsGroupHotKey = null;
			QuickDefendHotKey = null;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			//This should be the only thing in here
			NetHandler.HandlePackets(reader, whoAmI);
		}
	}
}
