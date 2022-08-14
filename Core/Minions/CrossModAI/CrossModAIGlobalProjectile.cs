using AmuletOfManyMinions.Core.Minions.AI;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI
{

	internal interface ICrossModSimpleMinion : ISimpleMinion
	{
		bool DoVanillaAI();
		void PostAI();

		// This is a little roundabout circular reference. Should still be garbage collected (maybe)
		SimpleMinionBehavior Behavior { get; }

	}

	internal delegate ICrossModSimpleMinion CrossModAISupplier(Projectile projectile);

	internal class CrossModAIGlobalProjectile : GlobalProjectile
	{

		// TODO it might make sense to use InstancePerEntity rather than manually managing these lifecycles
		internal static Dictionary<int, CrossModAISupplier> CrossModAISuppliers;

		internal static Dictionary<int, ICrossModSimpleMinion> CrossModAIs;

		public override void Load()
		{
			CrossModAISuppliers = new();
			CrossModAIs = new();
		}

		public override void Unload()
		{
			CrossModAISuppliers = null;
			CrossModAIs = null;
		}

		public override bool PreAI(Projectile projectile)
		{
			if(!CrossModAIs.TryGetValue(projectile.whoAmI, out var crossModMinion))
			{
				// TODO this might be too computationally intense
				if(CrossModAISuppliers.TryGetValue(projectile.type, out var supplier))
				{
					CrossModAIs[projectile.whoAmI] = supplier.Invoke(projectile);
				}
				return true;
			}
			crossModMinion.Behavior.MainBehavior();
			// This feels a little roundabout
			return crossModMinion.DoVanillaAI();
		}

		public override void PostAI(Projectile projectile)
		{
			// TODO
		}

		public override void Kill(Projectile projectile, int timeLeft)
		{
			if(CrossModAIs.ContainsKey(projectile.whoAmI))
			{
				CrossModAIs.Remove(projectile.whoAmI);
			}
		}

	}
}
