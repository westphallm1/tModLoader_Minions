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
		private readonly object[] Args;
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
			if(args.Length <= 2)
			{
				throw new ArgumentException("Mod.Call must have at least two arguments");
			}

			if(args[0] is not string message)
			{
				throw new ArgumentException("First argument to Mod.Call must be a message string");
			}

			//Future-proofing. Allowing new info to be returned or old info to be handled differently while maintaining backwards compat if necessary
			if(args[1] is not string versionString)
			{
				throw new ArgumentException("Second argument to Mod.Call must be a version string");
			}

			Version latestVersion = ModContent.GetInstance<AmuletOfManyMinions>().Version;
			Version apiVersion = new Version(versionString);
			//Example usage:
			//if (apiVersion < new Version(1, 3, 4))
			//propagate to submethods if needed

			var a = new ArgsUnpacker(args, 2);
			switch (message)
			{
				// Access the state of a projectile that has been registered for cross mod AI
				case "GetState":
					return GetState(a.Arg<ModProjectile>());
				case "GetStateDirect":
					return GetStateDirect(a.Arg<ModProjectile>(), a.Arg<object>());

				// Revert any AoMM-made changes to the state of the projectile made this frame
				case "ReleaseControl":
					return ReleaseControl(a.Arg<ModProjectile>());

				// Access the constructor params of a projectile that has been registered for cross mod AI
				case "GetParams":
					return GetParams(a.Arg<ModProjectile>());
				case "GetParamsDirect":
					return GetParamsDirect(a.Arg<ModProjectile>(), a.Arg<object>());
				case "UpdateParams":
					return UpdateParams(a.Arg<ModProjectile>(), a.Arg<Dictionary<string, object>>());
				case "UpdateParamsDirect":
					return UpdateParamsDirect(a.Arg<ModProjectile>(), a.Arg<object>());

				// Register projectiles for different configurations of the cross mod AI
				case "RegisterInfoMinion":
					return RegisterInfoMinion(a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg(600));
				case "RegisterInfoPet":
					return RegisterInfoPet(a.Arg<ModProjectile>(), a.Arg<ModBuff>());

				case "RegisterPathfindingMinion":
					return RegisterPathfindingMinion(
						a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg(600), a.Arg(8), a.Arg(12));
				case "RegisterPathfindingPet":
					return RegisterPathfindingPet(a.Arg<ModProjectile>(), a.Arg<ModBuff>());

				case "RegisterFlyingMinion":
					return RegisterFlyingMinion(a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg<int?>(), a.Arg(600), a.Arg(8), a.Arg(12), a.Arg(30));
				case "RegisterFlyingPet":
					return RegisterFlyingPet(a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg<int?>());

				case "RegisterGroundedMinion":
					return RegisterGroundedMinion(a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg<int?>(), a.Arg(600), a.Arg(8), a.Arg(12), a.Arg(30));
				case "RegisterGroundedPet":
					return RegisterGroundedPet(a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg<int?>());

				case "RegisterSlimePet":
					return RegisterSlimePet(a.Arg<ModProjectile>(), a.Arg<ModBuff>(), a.Arg<int?>(null), a.Arg(false));

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

		/// <summary>
		/// Get the entire <key, object> mapping of the projectile's cross-mod exposed state (read-only variables),
		/// if it has one. Cross mod state variables are annotated with [CrossModState]
		/// </summary>
		/// <param name="proj">The ModProjectile to access the state for</param>
		internal static Dictionary<string, object> GetState(ModProjectile proj)
		{
			if(!proj.Projectile.TryGetGlobalProjectile<CrossModAIGlobalProjectile>(out var result))
			{
				return default;
			}
			return result.CrossModAI?.GetCrossModState();
		}


		/// <summary>
		/// Copy the projectile's entire cross-mod exposed state directly into another object using 
		/// reflection. The object is expected to have properties of the correct types.
		/// Cross mod state variables are annotated with [CrossModProperty].
		/// </summary>
		/// <param name="proj">The ModProjectile to access the state for</param>
		/// <param name="destination">The object to copy AoMM's state into</param>
		internal static object GetStateDirect(ModProjectile proj, object destination)
		{
			if(!proj.Projectile.TryGetGlobalProjectile<CrossModAIGlobalProjectile>(out var result))
			{
				return default;
			}
			result.CrossModAI?.PopulateStateObject(destination);
			return default;
		}

		/// <summary>
		/// Get the entire <key, object> mapping of the projectile's cross-mod exposed params (read/write variables),
		/// if it has one. Cross mod param variables are annotated with [CrossModParam]
		/// </summary>
		/// <param name="proj">The ModProjectile to access the state for</param>
		internal static Dictionary<string, object> GetParams(ModProjectile proj)
		{
			if(!proj.Projectile.TryGetGlobalProjectile<CrossModAIGlobalProjectile>(out var result))
			{
				return default;
			}
			return result.CrossModAI?.GetCrossModParams();
		}


		/// <summary>
		/// Copy the projectile's entire cross-mod exposed state directly into another object using 
		/// reflection. The object is expected to have properties of the correct types.
		/// Cross mod state variables are annotated with [CrossModProperty].
		/// </summary>
		/// <param name="proj">The ModProjectile to access the state for</param>
		/// <param name="destination">The object to copy AoMM's state into</param>
		internal static object GetParamsDirect(ModProjectile proj, object destination)
		{
			if(!proj.Projectile.TryGetGlobalProjectile<CrossModAIGlobalProjectile>(out var result))
			{
				return default;
			}
			result.CrossModAI?.PopulateParamsObject(destination);
			return default;
		}

		/// <summary>
		/// Update theprojectile's cross-mod exposed params (read/write variables) based on a
		/// given dictionary of <key, object> pairs.
		/// </summary>
		/// <param name="proj">The ModProjectile to access the state for</param>
		internal static object UpdateParams(ModProjectile proj, Dictionary<string, object> values)
		{
			if(!proj.Projectile.TryGetGlobalProjectile<CrossModAIGlobalProjectile>(out var result))
			{
				return default;
			}
			result.CrossModAI?.UpdateCrossModParams(values);
			return default;
		}

		/// <summary>
		/// Update the projectile's cross-mod exposed params via reflection based on an
		/// arbitrary object that contains properties with matching names and types.
		/// </summary>
		/// <param name="proj">The ModProjectile to update the parameters for</param>
		/// <param name="source">The object containing </param>
		internal static object UpdateParamsDirect(ModProjectile proj, object source)
		{
			if(!proj.Projectile.TryGetGlobalProjectile<CrossModAIGlobalProjectile>(out var result))
			{
				return default;
			}
			result.CrossModAI?.UpdateCrossModParams(source);
			return default;
		}

		/// <summary>
		/// For the following frame, do not apply AoMM's pre-calculated position and velocity changes 
		/// to the projectile in PostAI(). Used to temporarily override behavior in fully managed minion AIs
		/// </summary>
		/// <param name="proj">The ModProjectile to release for this frame</param>
		internal static object ReleaseControl(ModProjectile proj)
		{
			if(proj.Projectile.TryGetGlobalProjectile<CrossModAIGlobalProjectile>(out var result))
			{
				result.CrossModAI?.ReleaseControl();
			}
			return default;
		}

		/// <summary>
		/// Register a read-only cross mod minion. AoMM will run its state calculations for this minion every frame,
		/// but will not perform any actions based on those state calculations. The ModProjectile may read AoMM's 
		/// calculated state using mod.Call("GetState",this), and act on that state as it pleases.
		/// </summary>
		/// <param name="proj">The singleton instance of the ModProjectile for this minion type</param>
		/// <param name="buff">The singleton instance of the ModBuff associated with the minion</param>
		/// <param name="searchRange">The range (in pixels) over which the tactic enemy selection should search.</param>
		/// <returns></returns>
		internal static object RegisterInfoMinion(ModProjectile proj, ModBuff buff, int searchRange)
		{
			AddBuffMappingIdempotent(buff);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
				new BasicCrossModAI(proj, buff.Type,defaultPathfinding: false) { SearchRange = searchRange };
			return default;
		}

		/// <summary>
		/// Register a read-only cross mod combat pet. AoMM will run its state calculations for this combat pet every frame,
		/// but will not perform any actions based on those state calculations. The ModProjectile may read AoMM's 
		/// calculated state using mod.Call("GetState",this), and act on that state as it pleases.
		/// </summary>
		/// <param name="proj">The singleton instance of the ModProjectile for this combat pet type</param>
		/// <param name="buff">The singleton instance of the ModBuff associated with the pet</param>
		/// <param name="searchRange">The range (in pixels) over which the tactic enemy selection should search.</param>
		/// <returns></returns>
		internal static object RegisterInfoPet(ModProjectile proj, ModBuff buff)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
				new BasicCrossModAI(proj, buff.Type, defaultPathfinding: false) { IsPet = true };
			return default;
		}

		/// <summary>
		/// Register a basic cross mod minion. AoMM will run its state calculations for this minion every frame,
		/// and take over its position and velocity while the pathfinding node is present.
		/// </summary>
		/// <param name="proj">The singleton instance of the ModProjectile for this minion type</param>
		/// <param name="buff">The singleton instance of the ModBuff associated with the minion</param>
		/// <param name="searchRange">
		/// The range (in pixels) over which the tactic enemy selection should search. AoMM will release the 
		/// minion from the pathfinding AI as soon as an enemy is detected in range.
		/// </param>
		/// <param name="travelSpeed">The speed at which the minion should travel while following the pathfinder</param>
		/// <param name="inertia">
		/// How quickly the minion should change directions while following the pathfinder. Higher values lead to
		/// slower turning.
		/// </param>
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


		/// <summary>
		/// Register a basic cross mod combat pet. AoMM will run its state calculations for this minion every frame,
		/// and take over its position and velocity while the pathfinding node is present.
		/// The pet's movement speed and search range will automatically scale with the player's combat
		/// pet level.
		/// </summary>
		/// <param name="proj">The singleton instance of the ModProjectile for this minion type</param>
		/// <param name="buff">The singleton instance of the ModBuff associated with the minion</param>
		/// </param>
		internal static object RegisterPathfindingPet(ModProjectile proj, ModBuff buff)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
				new BasicCrossModAI(proj, buff.Type, defaultPathfinding: true, true);
			return default;
		}

		/// <summary>
		/// Register a fully managed flying cross mod combat pet. AoMM will take over this projectile's 
		/// AI every frame, and will cause it to behave like a basic flying minion (eg. the Raven staff).
		/// The pet's damage, movement speed, and search range will automatically scale with the player's combat
		/// pet level.
		/// </summary>
		/// <param name="proj">The singleton instance of the ModProjectile for this minion type</param>
		/// <param name="buff">The singleton instance of the ModBuff associated with the minion</param>
		/// <param name="projType">Which projectile the minion should shoot. If null, the minion will do a melee attack</param>
		internal static object RegisterFlyingPet(ModProjectile proj, ModBuff buff, int? projType)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj => new FlyingCrossModAI(proj, buff.Type, projType, true);
			return default;
		}

		/// <summary>
		/// Register a fully managed flying cross mod minion. AoMM will take over this projectile's 
		/// AI every frame, and will cause it to behave like a basic flying minion (eg. the Raven staff).
		/// </summary>
		/// <param name="proj">The singleton instance of the ModProjectile for this minion type</param>
		/// <param name="buff">The singleton instance of the ModBuff associated with the minion</param>
		/// <param name="projType">Which projectile the minion should shoot. If null, the minion will do a melee attack</param>
		/// <param name="searchRange">The range (in pixels) over which the tactic enemy selection should search.</param>
		/// <param name="travelSpeed">The speed at which the minion should travel while following the pathfinder</param>
		/// <param name="inertia">
		/// How quickly the minion should change directions while following the pathfinder. Higher values lead to
		/// slower turning.
		/// </param>
		internal static object RegisterFlyingMinion(ModProjectile proj, ModBuff buff, int? projType, int searchRange, int travelSpeed, int inertia, int attackFrames)
		{
			AddBuffMappingIdempotent(buff);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
				new FlyingCrossModAI(proj, buff.Type, projType, false) 
				{ 
					SearchRange = searchRange, MaxSpeed = travelSpeed, Inertia = inertia, AttackFrames = attackFrames
				};
			return default;
		}

		/// <summary>
		/// Register a fully managed grounded cross mod combat pet. AoMM will take over this projectile's 
		/// AI every frame, and will cause it to behave like a basic flying minion (eg. the Flinx staff).
		/// The pet's damage, movement speed, and search range will automatically scale with the player's combat
		/// pet level.
		/// </summary>
		/// <param name="proj">The singleton instance of the ModProjectile for this minion type</param>
		/// <param name="buff">The singleton instance of the ModBuff associated with the minion</param>
		/// <param name="projType">Which projectile the minion should shoot. If null, the minion will do a melee attack</param>
		internal static object RegisterGroundedPet(ModProjectile proj, ModBuff buff, int? projType)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj => new GroundedCrossModAI(proj, buff.Type, projType, true);
			return default;
		}

		/// <summary>
		/// Register a fully managed grounded cross mod minion. AoMM will take over this projectile's 
		/// AI every frame, and will cause it to behave like a basic flying minion (eg. the Flinx staff).
		/// </summary>
		/// <param name="proj">The singleton instance of the ModProjectile for this minion type</param>
		/// <param name="buff">The singleton instance of the ModBuff associated with the minion</param>
		/// <param name="projType">Which projectile the minion should shoot. If null, the minion will do a melee attack</param>
		/// <param name="searchRange">The range (in pixels) over which the tactic enemy selection should search.</param>
		/// <param name="travelSpeed">The speed at which the minion should travel while following the pathfinder</param>
		/// <param name="inertia">
		/// How quickly the minion should change directions while following the pathfinder. Higher values lead to
		/// slower turning.
		/// </param>
		internal static object RegisterGroundedMinion(ModProjectile proj, ModBuff buff, int? projType, int searchRange, int travelSpeed, int inertia, int attackFrames)
		{
			AddBuffMappingIdempotent(buff);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj =>
				new GroundedCrossModAI(proj, buff.Type, projType, false) 
				{ 
					SearchRange = searchRange, MaxSpeed = travelSpeed, Inertia = inertia, AttackFrames = attackFrames
				};
			return default;
		}

		/// <summary>
		/// Register a fully managed slime cross mod combat pet. AoMM will take over this projectile's 
		/// AI every frame, and will cause it to behave like a basic slime minion (eg. the Baby Slime staff).
		/// The pet's damage, movement speed, and search range will automatically scale with the player's combat
		/// pet level.
		/// TODO: This currently doesn't support projectiles
		/// </summary>
		/// <param name="proj">The singleton instance of the ModProjectile for this minion type</param>
		/// <param name="buff">The singleton instance of the ModBuff associated with the minion</param>
		/// <param name="projType">Which projectile the minion should shoot. Currently unused</param>
		/// <param name="alwaysBounce">Whether to always bounce while idling</param>
		internal static object RegisterSlimePet(ModProjectile proj, ModBuff buff, int? projType, bool alwaysBounce)
		{
			AddBuffMappingIdempotent(buff);
			CombatPetBuff.CombatPetBuffTypes.Add(buff.Type);
			CrossModAIGlobalProjectile.CrossModAISuppliers[proj.Type] = proj => 
				new SlimeCrossModAI(proj, buff.Type, projType, true) { AlwaysBounce = alwaysBounce };
			return default;
		}
	}
}
