using AmuletOfManyMinions.Core.Minions.CrossModAI;
using AmuletOfManyMinions.Core.Minions.Tactics;
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

			switch ((string)args[0])
			{
				case "RegisterPathfinder":
					return RegisterPathfinder(
						args.Get<ModProjectile>(1), args.Get<ModBuff>(2), 
						args.Get(3, 8), args.Get(4, 12), args.Get(5, 600));
				default:
					break;
			}
			return default;
		}



		internal static object RegisterPathfinder(ModProjectile proj, ModBuff buff, int travelSpeed, int inertia, int searchRange)
		{
			MinionTacticsGroupMapper.AddBuffMapping(buff);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
			{
				return new BasicCrossModAI(proj, buff.Type, travelSpeed, inertia, searchRange);
			};
			return default;
		}
	}
}
