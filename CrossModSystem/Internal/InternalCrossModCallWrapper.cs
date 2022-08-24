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
		internal Mod Mod { get; set; }

		public InternalCrossModCallWrapper(string modName)
		{
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


		private void RegisterCombatPet(string method, string projName, string buffName, int? projId)
		{
			if (!ModLoaded) { return; }
			var projInstance = FindProj(projName);
			var buffInstance = FindBuff(buffName);

			// Don't override any cross-mod AI added by the mod itself
			if(CrossModAIGlobalProjectile.CrossModAISuppliers.ContainsKey(projInstance.Type))
			{
				return;
			}
			ModCallHandler.HandleCall(method, projInstance, buffInstance, projId);
		}

		public void RegisterFlyingPet(string projName, string buffName, int? projId)
		{
			RegisterCombatPet("RegisterFlyingPet", projName, buffName, projId);
		}

		public void RegisterGroundedPet(string projName, string buffName, int? projId)
		{
			RegisterCombatPet("RegisterGroundedPet", projName, buffName, projId);
		}

		public void RegisterSlimePet(string projName, string buffName, int? projId)
		{
			RegisterCombatPet("RegisterSlimePet", projName, buffName, projId);
		}
	}
}
