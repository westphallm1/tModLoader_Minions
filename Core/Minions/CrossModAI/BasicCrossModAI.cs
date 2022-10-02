using AmuletOfManyMinions.Core.Minions.AI;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI
{
	/// <summary>
	/// Basic cross-mod AI that allows for exposing AoMM-tracked state to other mods. Also include
	/// the basic "follow waypoint" code here, since it's pretty simple.
	/// </summary>
	internal class BasicCrossModAI : ICrossModSimpleMinion
	{

		public Projectile Projectile { get; set; }

		public Player Player => Main.player[Projectile.owner];
		public int BuffId { get; set; }

		public SimpleMinionBehavior Behavior { get; set; }

		[CrossModParam]
		[CrossModState]
		public virtual int MaxSpeed { get; internal set; }

		[CrossModParam]
		[CrossModState]
		public virtual int Inertia { get; internal set; }

		[CrossModParam]
		[CrossModState]
		public int SearchRange { get; internal set; }

		// For combat pets, apply a fixed multipier to the automatically scaling
		// movement parameters
		[CrossModParam]
		[CrossModState]
		public virtual float MaxSpeedScaleFactor { get; internal set; } = 1f;

		[CrossModParam]
		[CrossModState]
		public virtual float InertiaScaleFactor { get; internal set; } = 1f;

		internal bool UseDefaultPathfindingMovement { get; set; }

		internal ProjectileStateCache ProjCache { get; set; } = new();

		internal ProjectileDefaultsCache ProjDefaultsCache { get; set; }

		public WaypointMovementStyle WaypointMovementStyle => WaypointMovementStyle.IDLE;


		// Track whether the projectile was active at the start of the frame,
		// so we can run rollback/cleanup in the case that the projectile became
		// inactive during this frame
		private bool wasActive;

		private bool spawnedFromCrossModBuff;

		// Tri-state backing variable for the active flag. While undefined, determine
		// active state automatically. Otherwise, return non-null value directly 
		private bool? isActiveTriState;

		private bool DefaultIsActiveValue => Player.HasBuff(BuffId) && (IsPet || spawnedFromCrossModBuff);

		// Check for whether cross mod AI should be applied to this specific projectile
		// - For pets, just check that the cross mod buff is active
		// - For minions, also check that this projecile was spawned specifically from the cross
		// mod buff
		[CrossModParam]
		[CrossModState]
		public bool IsActive 
		{ 
			get => isActiveTriState ?? DefaultIsActiveValue;
			// To prevent the default `UpdateParams` call from automatically updating IsActive,
			// ensure the call specifically changes the value of IsActive first
			set { if (isActiveTriState != default || value != DefaultIsActiveValue) { isActiveTriState = value; } }
		} 

		// A block of properties used exclusively for generating the cross mod state dictionary
		// TODO it is a bit roundabout to implement this

		[CrossModState]
		public Vector2? NextPathfindingTaret => 
			Behavior.NextPathfindingTarget is Vector2 target ? Projectile.Center + target : null;

		[CrossModState]
		public Vector2? PathfindingDestination => Behavior.PathfindingDestination;

		[CrossModState]
		public NPC TargetNPC => Behavior.TargetNPCIndex is int idx ? Main.npc[idx] : default;

		// TODO this is a lazy implementation, doesn't sort wholly correctly
		// TODO enemies will gradually disappear from this list as they die, until a full refresh is done
		[CrossModState]
		public List<NPC> PossibleTargetNPCs => Behavior.PossibleTargets?
			.Where(npc=>npc.active)
			.OrderBy(npc => Vector2.DistanceSquared(npc.Center, TargetNPC?.Center ?? Player.Center))
			.ToList();

		[CrossModState]
		public bool IsPet { get; set; }

		[CrossModState]
		public int PetLevel => Player.GetModPlayer<LeveledCombatPetModPlayer>().PetLevelInfo.Level;

		[CrossModState]
		public int PetDamage => Player.GetModPlayer<LeveledCombatPetModPlayer>().PetLevelInfo.BaseDamage;

		// Basic state variables for the three things a minion can do (attack, pathfind, and idle)
		[CrossModState]
		public bool IsPathfinding => Behavior.IsFollowingBeacon;

		[CrossModState]
		public bool IsAttacking => TargetNPC != default && Behavior.VectorToTarget != default && !IsPathfinding;

		[CrossModState]
		public bool IsIdle => !IsAttacking && !IsPathfinding;


		// Cache the names of cross mod state properties for faster lookup
		private Dictionary<string, PropertyInfo> CrossModStateProperties { get; set; }

		// Cache for the values of cross mod state properties, reset every frame
		private Dictionary<string, object> CrossModStateDict { get; set; }

		// Cache the names of cross mod parameter properties for faster lookup
		private Dictionary<string, PropertyInfo> CrossModParamProperties { get; set; }

		// Cache for the values of cross mod parameter properties, reset every frame
		private Dictionary<string, object> CrossModParamDict { get; set; }




		public BasicCrossModAI(
			Projectile projectile, 
			int buffId, bool defaultPathfinding = false, bool isPet = false)
		{
			Projectile = projectile;
			BuffId = buffId;
			UseDefaultPathfindingMovement = defaultPathfinding;
			IsPet = isPet;
			Behavior = new(this);
			FindCrossModProperties();
		}

		private void FindCrossModProperties()
		{
			CrossModStateProperties = GetType().GetProperties()
				.Where(p => p.IsDefined(typeof(CrossModState), false))
				.ToDictionary(p => p.Name, p => p);

			CrossModParamProperties = GetType().GetProperties()
				.Where(p => p.IsDefined(typeof(CrossModParam), false))
				.ToDictionary(p => p.Name, p => p);
		}



		internal void FakePlayerFlyingHeight()
		{
			ProjCache.Cache(Projectile);
			Player.position.Y = Projectile.position.Y - 320;
		}

		internal virtual void ApplyPetDefaults()
		{
			ProjDefaultsCache ??= new ProjectileDefaultsCache(Projectile);
			Projectile.minion = true;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.usesIDStaticNPCImmunity = false;
			Projectile.localNPCHitCooldown = 20;
		}

		internal virtual void UpdatePetState()
		{
			var leveledPetPlayer = Player.GetModPlayer<LeveledCombatPetModPlayer>();
			var info = CombatPetLevelTable.PetLevelTable[leveledPetPlayer.PetLevel];
			SearchRange = info.BaseSearchRange;
			Inertia = (int)(InertiaScaleFactor * (info.Level < 6 ? 10 : 15 - info.Level));
			MaxSpeed = (int)(MaxSpeedScaleFactor * info.BaseSpeed);
			Projectile.originalDamage = leveledPetPlayer.PetDamage;
		}

		public virtual void OnSpawn()
		{
			//if(IsPet) { ApplyPetDefaults(); }
		}

		public virtual Vector2 IdleBehavior()
		{
			CrossModStateDict = null;
			CrossModParamDict = null;
			ProjCache.CacheInitial(Projectile);
			if(IsPet && IsActive && !wasActive) 
			{ 
				ApplyPetDefaults(); 
			}
			if(IsPet) 
			{ 
				UpdatePetState(); 
			}
			// no op
			return default;
		}

		public virtual void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// Only take over idle movement if pathfinding
			if(!Behavior.IsFollowingBeacon || !UseDefaultPathfindingMovement) { return; }

			if(vectorToIdlePosition.LengthSquared() > MaxSpeed * MaxSpeed)
			{
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= MaxSpeed;
			} 
			if (!Behavior.Pathfinder.InTransit && vectorToIdlePosition.LengthSquared() < MaxSpeed * MaxSpeed)
			{
				Projectile.velocity = vectorToIdlePosition;
			} else
			{
				Projectile.velocity = (Projectile.velocity * (Inertia - 1) + vectorToIdlePosition) / Inertia;
			}
			// trick to force many vanilla-styled minions into their flying animation
			// Would be surprised if this did not have unintended side effects
			FakePlayerFlyingHeight();
		}
		
		public virtual Vector2? FindTarget()
		{
			return Behavior.SelectedEnemyInRange(SearchRange) is Vector2 target ? target - Projectile.Center : null;
		}


		public virtual void PostAI()
		{
			// Uncache one last time after unsetting IsActive to prevent the
			// case where a cross-mod computed state is only partially applied
			if(!IsActive && !wasActive) { return; }
			if(IsPet && wasActive && !IsActive)
			{
				// Before deactivating, restore the original values of SetDefaults to the
				// projectile
				ProjDefaultsCache?.RestoreDefaults(Projectile);
			}
			wasActive = IsActive;
			ProjCache.Uncache(Projectile);
		}

		public virtual void AfterMoving() 
		{
			// no op
		}

		public virtual bool DoVanillaAI() => true;


		public virtual bool MinionContactDamage() => false;

		public virtual void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// No op
		}

		// Utility method that can be called from within the AI() of a cross mod minion
		// to prevent AoMM from overriding changes made to its velocity this frame
		public void ReleaseControl()
		{
			ProjCache.Rollback(Projectile);
		}

		// Based on the projectile's spawn conditions, determine whether it should have cross-mod
		// AI applied. Check both that the cross-mod buff registered to the minion is present,
		// and that this projectile was spawned by that buff, or an item that creates that buff.
		// Can be manually updated by another mod via SetParameters for more complicated use cases
		public void SetActiveFlag(IEntitySource source)
		{
			spawnedFromCrossModBuff = (source is EntitySource_Buff buff && buff.BuffId == BuffId) ||
				(source is EntitySource_ItemUse_WithAmmo item && item.Item.buffType == BuffId);
		}

		// Set of methods for getting/setting read-only and read/write variables from mod.Calls
		public Dictionary<string, object> GetCrossModState()
		{
			// TODO evaluate the efficiency of using reflection here
			CrossModStateDict ??= CrossModStateProperties.ToDictionary(kv => kv.Key, kv => kv.Value.GetValue(this, null));
			return CrossModStateDict;
		}

		public Dictionary<string, object> GetCrossModParams()
		{
			// TODO evaluate the efficiency of using reflection here
			CrossModParamDict ??= CrossModParamProperties.ToDictionary(kv => kv.Key, kv => kv.Value.GetValue(this, null));
			return CrossModParamDict;
		}

		public void UpdateCrossModParams(Dictionary<string, object> source)
		{
			// TODO check that the properties being set are actually CrossModParams
			foreach(var kv in source)
			{
				if(!CrossModParamProperties.TryGetValue(kv.Key, out var modProperty)) { continue; }
				if(modProperty?.PropertyType == kv.Value.GetType())
				{
					modProperty.SetValue(this, kv.Value);
				}
			}
		}

		public void UpdateCrossModParams(object source)
		{
			foreach (var property in source.GetType().GetProperties())
			{
				if(!CrossModParamProperties.TryGetValue(property.Name, out var modProperty)) { continue; }
				if(modProperty?.PropertyType == property.PropertyType && property?.GetValue(source) is var propertyState)
				{
					modProperty.SetValue(this, propertyState);
				}
			}
		}

		public void PopulateStateObject(object destination)
		{
			foreach (var property in destination.GetType().GetProperties())
			{
				if(!CrossModStateProperties.TryGetValue(property.Name, out var modProperty)) { continue; }
				if(modProperty?.PropertyType == property.PropertyType && modProperty?.GetValue(this) is var propertyState)
				{
					property.SetValue(destination, propertyState);
				}
			}
		}

		public void PopulateParamsObject(object destination)
		{
			foreach (var property in destination.GetType().GetProperties())
			{
				if(!CrossModParamProperties.TryGetValue(property.Name, out var modProperty)) { continue; }
				if(modProperty?.PropertyType == property.PropertyType && modProperty?.GetValue(this) is var propertyState)
				{
					property.SetValue(destination, propertyState);
				}
			}
		}
	}
}
