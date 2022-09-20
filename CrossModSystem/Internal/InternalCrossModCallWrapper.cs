using AmuletOfManyMinions.Core.Minions.CrossModAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.CrossModSystem.Internal
{
	internal class InternalCrossModCallWrapper
	{

		// convenience shorthand for ModContent.ProjectileType
		internal static int PT<T>() where T: ModProjectile
		{
			return ModContent.ProjectileType<T>();
		}

		internal bool ModLoaded { get; set; } = false;
		internal Mod Aomm { get; set; }
		internal Mod Mod { get; set; }

		public InternalCrossModCallWrapper(Mod aomm, string modName)
		{
			Aomm = aomm;
			if(ModLoader.TryGetMod(modName, out Mod mod))
			{
				ModLoaded = true;
				Mod = mod;
			}
		}

		internal ModBuff FindBuff(string buffName)
		{
			if (!ModLoaded) { return null; }
			return Mod.Find<ModBuff>(buffName);
		}

		internal ModProjectile FindProj(string projName)
		{
			if (!ModLoaded) { return null; }
			return Mod.Find<ModProjectile>(projName);
		}


		private void RegisterCombatPet(string method, string projName, string buffName, int? projId, params object[] args)
		{
			if (!ModLoaded) { return; }
			// Code this defensively, don't stop mods from loading if cross-mod registration fails
			try
			{
				var projInstance = FindProj(projName);
				var buffInstance = FindBuff(buffName);

				// Don't override any cross-mod AI added by the mod itself
				if(CrossModAIGlobalProjectile.CrossModAISuppliers.ContainsKey(projInstance.Type))
				{
					return;
				}
				object[] allArgs = (new object[] { method, "0.16.1", projInstance, buffInstance, projId }).Concat(args).ToArray();
				ModCallHandler.HandleCall(allArgs);
			} catch(Exception e)
			{
				Aomm.Logger.Error($"Unable to register cross-mod minion for {Mod.Name}: {projName}/{buffName}. Reason: {e.Message}");
			}
		}

		public void RegisterFlyingPet(string projName, string buffName, int? projId, bool defaultIdle = true)
		{
			RegisterCombatPet("RegisterFlyingPet", projName, buffName, projId, defaultIdle);
		}

		public void RegisterGroundedPet(string projName, string buffName, int? projId, bool defaultIdle = true)
		{
			RegisterCombatPet("RegisterGroundedPet", projName, buffName, projId, defaultIdle);
		}

		public void RegisterSlimePet(string projName, string buffName, int? projId, bool defaultIdle = true)
		{
			RegisterCombatPet("RegisterSlimePet", projName, buffName, projId, defaultIdle);
		}
	}
}
