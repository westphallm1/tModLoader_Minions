using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.CrossModSystem.Samples
{
	// Special thanks to the mod authors for open sourcing this
	internal class AssortedCrazyThingsCrossMod : ModSystem
	{
		public override void PostSetupContent()
		{
			ACTRegisterSlimePathfinding();
			ACTRegisterDronePathfinding();
			ACTRegisterFlyingPets();
			ACTRegisterGroundedPets();
		}

		internal ModBuff FindBuff(string buffName)
		{
			if (!ModLoader.TryGetMod("AssortedCrazyThings", out Mod actMod)) { return null; }
			return actMod.Find<ModBuff>(buffName);
		}

		internal ModProjectile FindProj(string projName)
		{
			if (!ModLoader.TryGetMod("AssortedCrazyThings", out Mod actMod)) { return null; }
			return actMod.Find<ModProjectile>(projName);
		}


		/// <summary>
		/// Register ACT's SlimePackMinion for AoMM's pathfinding. AoMM will override the projectile's
		/// position and velocity while the pathfinder is present and the minion is not attacking an enemy,
		/// but will preserve its normal AI otherwise.
		/// </summary>
		internal void ACTRegisterSlimePathfinding()
		{
			if (!ModLoader.TryGetMod("AssortedCrazyThings", out Mod actMod)) { return; }
			var amuletOfManyMinions = Mod; // this mod

			ModProjectile actSlimeMinion = actMod.Find<ModProjectile>("SlimePackMinion");
			ModProjectile actSlimeMinion2 = actMod.Find<ModProjectile>("SlimePackAssortedMinion");
			ModProjectile actSlimeMinion3 = actMod.Find<ModProjectile>("SlimePackSpikedMinion");
			ModBuff actSlimeBuff = actMod.Find<ModBuff>("SlimePackMinionBuff");
			int travelSpeed = 8;
			int inertia = 14;
			int searchRange = 700;

			amuletOfManyMinions.Call("RegisterPathfinder", actSlimeMinion, actSlimeBuff, travelSpeed, inertia, searchRange);
			amuletOfManyMinions.Call("RegisterPathfinder", actSlimeMinion2, actSlimeBuff, travelSpeed, inertia, searchRange);
			amuletOfManyMinions.Call("RegisterPathfinder", actSlimeMinion3, actSlimeBuff, travelSpeed, inertia, searchRange);
		}


		/// <summary>
		/// Register ACT's Drone minions for AoMM's pathfinding. 
		/// </summary>
		internal void ACTRegisterDronePathfinding()
		{
			if (!ModLoader.TryGetMod("AssortedCrazyThings", out Mod actMod)) { return; }
			var amuletOfManyMinions = Mod; // this mod

			ModProjectile actDroneMinion = actMod.Find<ModProjectile>("BasicLaserDrone");
			ModProjectile actDroneMinion2 = actMod.Find<ModProjectile>("HealingDrone");
			ModBuff actDroneBuff = actMod.Find<ModBuff>("DroneControllerBuff");
			int travelSpeed = 8;

			amuletOfManyMinions.Call("RegisterPathfinder", actDroneMinion, actDroneBuff, travelSpeed);
			amuletOfManyMinions.Call("RegisterPathfinder", actDroneMinion2, actDroneBuff, travelSpeed);
			// TODO the rest of the drone types
		}

		/// <summary>
		/// Register ACT's Flying pets as AoMM flying combat pets
		/// </summary>
		private void ACTRegisterFlyingPets()
		{
			if (!ModLoader.TryGetMod("AssortedCrazyThings", out Mod actMod)) { return; }
			var aomm = Mod; // this mod
			aomm.Call("RegisterFlyingPet", FindProj("AnimatedTomeProj"), FindBuff("AnimatedTomeBuff"), (int?)ProjectileID.BookStaffShot);
			aomm.Call("RegisterFlyingPet", FindProj("DrumstickElementalProj"), FindBuff("DrumstickElementalBuff"), null);
		}

		/// <summary>
		/// Register ACT's grounded pets as AoMM grounded combat pets
		/// </summary>
		private void ACTRegisterGroundedPets()
		{
			if (!ModLoader.TryGetMod("AssortedCrazyThings", out Mod actMod)) { return; }
			var aomm = Mod; // this mod
			aomm.Call("RegisterGroundedPet", FindProj("CuteLamiaPetProj"), FindBuff("CuteLamiaPetBuff"), (int?)ProjectileID.AmethystBolt);
		}


	}
}
