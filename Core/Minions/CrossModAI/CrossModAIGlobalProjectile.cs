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
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI
{

	internal interface ICrossModSimpleMinion : ISimpleMinion
	{
		bool DoVanillaAI();
		void PostAI();

		void SetActiveFlag(IEntitySource source);

		bool IsActive { get; }

		Dictionary<string, object> GetCrossModState();
		Dictionary<string, object> GetCrossModParams();

		void ReleaseControl();

		void PopulateStateObject(object stateObject);

		void PopulateParamsObject(object stateObject);

		void UpdateCrossModParams(object source);

		void UpdateCrossModParams(Dictionary<string, object> source);

		public void OnTileCollide(Vector2 oldVelocity) { }


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

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			CrossModAI?.SetActiveFlag(source);
		}

		public override bool PreAI(Projectile projectile)
		{
			if(CrossModAI == default || !CrossModAI.IsActive) { return true; }
			CrossModAI.Behavior.MainBehavior();
			// This feels a little roundabout
			return CrossModAI.DoVanillaAI();
		}

		public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
		{
			if(CrossModAI == default || !CrossModAI.IsActive) { return true; }
			CrossModAI?.OnTileCollide(oldVelocity);
			return true;
		}

		public override void PostAI(Projectile projectile)
		{
			// Always run post-AI, since we may need to do cleanup from the projectile's
			// active state being switched from true to false
			CrossModAI?.PostAI();
		}


		public override bool MinionContactDamage(Projectile projectile)
		{
			// Ensure that pets that are turned into minions deal contact damage
			return CrossModAI?.IsActive ?? base.MinionContactDamage(projectile);
		}
	}
}
