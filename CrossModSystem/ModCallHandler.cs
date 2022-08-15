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
				case "RegisterPathfinder":
					return RegisterPathfinder(
						a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg(8), a.Arg(12), a.Arg(600));
				case "RegisterFlyingPet":
					return RegisterFlyingPet(
						a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg<int?>(), a.Arg(8), a.Arg(12), a.Arg(600));
				case "RegisterGroundedPet":
					return RegisterGroundedPet(
						a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg<int?>(), a.Arg(8), a.Arg(12), a.Arg(600));
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

		internal static object RegisterPathfinder(ModProjectile proj, ModBuff buff, int travelSpeed, int inertia, int searchRange)
		{
			AddBuffMappingIdempotent(buff);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
			{
				return new BasicCrossModAI(proj, buff.Type, travelSpeed, inertia, searchRange, defaultPathfinding: true);
			};
			return default;
		}

		internal static object RegisterFlyingPet(ModProjectile proj, ModBuff buff, int? projType, int travelSpeed, int inertia, int searchRange)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
			{
				return new FlyingCrossModAI(proj, buff.Type, projType);
			};
			return default;
		}

		internal static object RegisterGroundedPet(ModProjectile proj, ModBuff buff, int? projType, int travelSpeed, int inertia, int searchRange)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
			{
				return new GroundedCrossModAI(proj, buff.Type, projType);
			};
			return default;
		}
	}
}
