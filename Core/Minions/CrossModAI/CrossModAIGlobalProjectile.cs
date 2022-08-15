using AmuletOfManyMinions.Core.Minions.AI;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
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

		public void OnTileCollide(Vector2 oldVelocity)
		{
		}

	}

	internal delegate ICrossModSimpleMinion CrossModAISupplier(Projectile projectile);

	internal class CrossModAIGlobalProjectile : GlobalProjectile
	{

		public override bool InstancePerEntity => true;
		internal static Dictionary<int, CrossModAISupplier> CrossModAISuppliers;

		internal ICrossModSimpleMinion CrossModAI { get; set; }

		public override void Load()
		{
			CrossModAISuppliers = new();
		}

		public override void Unload()
		{
			CrossModAISuppliers = null;
		}


		public override void SetDefaults(Projectile projectile)
		{
			if(CrossModAISuppliers.TryGetValue(projectile.type, out var supplier))
			{
				CrossModAI = supplier.Invoke(projectile);
			}
		}

		public override bool PreAI(Projectile projectile)
		{
			if(CrossModAI == default) { return true; }
			CrossModAI.Behavior.MainBehavior();
			// This feels a little roundabout
			return CrossModAI.DoVanillaAI();
		}

		public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
		{
			CrossModAI?.OnTileCollide(oldVelocity);
			return base.OnTileCollide(projectile, oldVelocity);
		}

		public override void PostAI(Projectile projectile)
		{
			CrossModAI?.PostAI();
		}
	}
}
