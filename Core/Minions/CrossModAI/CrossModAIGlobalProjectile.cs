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

		bool IsIdle { get; }

		bool IsAttacking { get; }

		bool IsPathfinding { get; }

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

	public class AoMMVersionMultiBuffWorkaround: ModSystem
	{
		// HashMap from buff type to list of other buff types that should not 
		// be active at the same time
		private static Dictionary<int, HashSet<int>> CrossModCombatPetMutuallyExclusiveBuffs;

		public override void PostSetupRecipes()
		{
			// TODO is this the appropriate location for this hook?

			// Find all items that spawn a cross-mod registered pet, and create a one-to-many 
			// mapping of all the buffs that can spawn that pet
			var projIdToBuffTypeMap = new Dictionary<int, HashSet<int>>();

			CrossModCombatPetMutuallyExclusiveBuffs = ModContent.GetContent<ModItem>()
				.Select(mItem => mItem.Item)
				// enumerable of all items that spawn a cross-mod pet
				.Where(item => 
					CrossModAIGlobalProjectile.CrossModAISuppliers.ContainsKey(item.shoot) &&
					Main.vanityPet[item.buffType])
				// one to many map of a projectile type to all items that spawn it
				.GroupBy(i => i.shoot)
				// many to many map of a buff type to all other buff types that spawn the same pet
				.SelectMany(group => group.Select(g => new
				{
					BuffType = g.buffType,
					OtherBuffTypes = group.Select(g2 => g2.buffType).Where(buffType => buffType != g.buffType).ToHashSet()
				}))
				.ToDictionary(pair=>pair.BuffType, pair=>pair.OtherBuffTypes);
		}

		public override void Load()
		{
			On.Terraria.Player.FreeUpPetsAndMinions += Player_FreeUpPetsAndMinions;
			On.Terraria.Player.AddBuff += Player_AddBuff;
		}

		public override void Unload()
		{
			CrossModCombatPetMutuallyExclusiveBuffs = null;
		}

		private void Player_AddBuff(On.Terraria.Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
		{
			orig.Invoke(self, type, timeToAdd, quiet, foodHack);
			if(CrossModCombatPetMutuallyExclusiveBuffs.TryGetValue(type, out var otherTypes))
			{
				foreach(var otherType in otherTypes)
				{
					self.ClearBuff(otherType);
				}
			}
		}

		private void Player_FreeUpPetsAndMinions(On.Terraria.Player.orig_FreeUpPetsAndMinions orig, Player self, Item sItem)
		{
			// Hack to fix a rather tricky bug that arises from AoMM and non-AoMM versions of cross-mod pet buffs
			// spawning the same projectile. By default, the vanilla pet-free-up code will remove any existing instances of the
			// projectile spawned by either the main buff or AoMM version buff, but not remove the buff themselves. This
			// will leave the player in a state where both pet spawning buffs are active, and no copies of the pet are active.
			// Both copies of the buff will then spawn the pet on the next frame, leaving the player with a duplicate.
			// To circumvent this, temporarily un-flag the item as shooting the pet if the player already has one copy
			// of the pet active, so that the single active copy will not despawn in the first place.
			// We would ideally want to remove one buff as soon as the other version becomes active, but this does not
			// appear to be easily accomplished.
			int origShoot = sItem.shoot;
			
			if(Main.vanityPet[sItem.buffType] && self.ownedProjectileCounts[origShoot] > 0 && 
				CrossModAIGlobalProjectile.CrossModAISuppliers.ContainsKey(sItem.shoot))
			{
				sItem.shoot = ProjectileID.None;
			}
			orig.Invoke(self, sItem);
			sItem.shoot = origShoot;
		}


	}
}
