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
					return RegisterPathfinder((ModProjectile)args[1], (ModBuff)args[2], (int)args[3]);
				default:
					break;
			}
			return default;
		}



		internal static object RegisterPathfinder(ModProjectile proj, ModBuff buff, int travelSpeed)
		{
			MinionTacticsGroupMapper.AddBuffMapping(buff);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
			{
				return new FollowWaypointCrossModAI(proj, buff.Type, travelSpeed);
			};
			return default;
		}
	}
}
