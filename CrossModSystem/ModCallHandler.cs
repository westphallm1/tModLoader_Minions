using AmuletOfManyMinions.Core.Minions.CrossModAI;
using AmuletOfManyMinions.Core.Minions.CrossModAI.ManagedAI;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.CrossModSystem
{
	internal static class ObjectArrayExtensions
	{
		public static T Get<T>(this object[] args, int idx, T defaultVal = default)
		{
			if(args.Length > idx)
			{
				return (T) args[idx];
			} else
			{
				return defaultVal;
			}
		}
	}

	internal class ArgsUnpacker
	{
		private object[] Args;
		private int Idx;
		public ArgsUnpacker(object [] args, int startIdx = 0)
		{
			Args = args;
			Idx = startIdx;
		}

		public T Arg<T>(T defaultVal = default)
		{
			if(Args.Length > Idx)
			{
				return (T)Args[Idx++] ?? defaultVal;
			} else
			{
				return defaultVal;
			}
		}

	}
	internal class ModCallHandler
	{

		internal static object HandleCall(params object[] args)
		{
			if(args.Length == 0)
			{
				throw new ArgumentException("Mod.Call must have at least one argument");
			}

			if(args[0] is not string)
			{
				throw new ArgumentException("First argument to Mod.Call must be a string");
			}

			var a = new ArgsUnpacker(args, 1);
			switch ((string)args[0])
			{
				// Access the state of a projectile that has been registered for cross mod AI

				// Register projectiles for different configurations of the cross mod AI
				case "RegisterInfoMinion":
					return RegisterInformationalMinion(a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg(600));
				case "RegisterInfoPet":
					return RegisterInformationalPet(a.Arg<ModProjectile>(), a.Arg<ModBuff>());

				case "RegisterPathfindingMinion":
					return RegisterPathfindingMinion(
						a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg(600), a.Arg(8), a.Arg(12));
				case "RegisterPathfindingPet":
					return RegisterPathfindingPet(a.Arg<ModProjectile>(), a.Arg<ModBuff>());

				case "RegisterFlyingMinion":
					return RegisterFlyingMinion(a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg<int?>(), a.Arg(600), a.Arg(8), a.Arg(12));
				case "RegisterFlyingPet":
					return RegisterFlyingPet(a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg<int?>());

				case "RegisterGroundedMinion":
					return RegisterGroundedMinion(a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg<int?>(), a.Arg(600), a.Arg(8), a.Arg(12));
				case "RegisterGroundedPet":
					return RegisterGroundedPet(a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg<int?>());

				default:
					break;
			}
			return default;
		}


		private static void AddBuffMappingIdempotent(ModBuff buff)
		{
			if(!MinionTacticsGroupMapper.TypeToHashDict.ContainsKey(buff.Type))
			{
				MinionTacticsGroupMapper.AddBuffMapping(buff);
			}
		}

		internal static object RegisterInformationalMinion(ModProjectile proj, ModBuff buff, int searchRange)
		{
			AddBuffMappingIdempotent(buff);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
				new BasicCrossModAI(proj, buff.Type,defaultPathfinding: false) { SearchRange = searchRange };
			return default;
		}

		internal static object RegisterInformationalPet(ModProjectile proj, ModBuff buff)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
				new BasicCrossModAI(proj, buff.Type, defaultPathfinding: false) { IsPet = true };
			return default;
		}

		internal static object RegisterPathfindingMinion(ModProjectile proj, ModBuff buff, int searchRange, int travelSpeed, int inertia)
		{
			AddBuffMappingIdempotent(buff);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
				new BasicCrossModAI(proj, buff.Type, defaultPathfinding: true)
				{ 
					SearchRange = searchRange, MaxSpeed = travelSpeed, Inertia = inertia
				};
			return default;
		}

		internal static object RegisterPathfindingPet(ModProjectile proj, ModBuff buff)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
				new BasicCrossModAI(proj, buff.Type, defaultPathfinding: true, true);
			return default;
		}

		internal static object RegisterFlyingPet(ModProjectile proj, ModBuff buff, int? projType)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj => new FlyingCrossModAI(proj, buff.Type, projType, true);
			return default;
		}

		internal static object RegisterFlyingMinion(ModProjectile proj, ModBuff buff, int? projType, int searchRange, int travelSpeed, int inertia)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
				new FlyingCrossModAI(proj, buff.Type, projType, false) 
				{ 
					SearchRange = searchRange, MaxSpeed = travelSpeed, Inertia = inertia
				};
			return default;
		}

		internal static object RegisterGroundedPet(ModProjectile proj, ModBuff buff, int? projType)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj => new GroundedCrossModAI(proj, buff.Type, projType, true);
			return default;
		}

		internal static object RegisterGroundedMinion(ModProjectile proj, ModBuff buff, int? projType, int searchRange, int travelSpeed, int inertia)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
				new GroundedCrossModAI(proj, buff.Type, projType, false) 
				{ 
					SearchRange = searchRange, MaxSpeed = travelSpeed, Inertia = inertia
				};
			return default;
		}
	}
}
