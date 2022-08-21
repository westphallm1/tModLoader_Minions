using AmuletOfManyMinions.CrossModSystem.SampleMod.Pets.SampleFlyingPet;
using AmuletOfManyMinions.CrossModSystem.SampleMod.Pets.SampleFlyingRangedPet;
using AmuletOfManyMinions.CrossModSystem.SampleMod.Pets.SampleGroundedPet;
using AmuletOfManyMinions.CrossModSystem.SampleMod.Pets.SampleGroundedRangedPet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.CrossModSystem.SampleMod
{
	internal class SampleModSystem : ModSystem
	{

		public override void PostSetupContent()
		{
			RegisterPets();
		}

		private static void RegisterPets()
		{
			// Register melee, flying cross mod pet
			AmuletOfManyMinionsApi.RegisterFlyingPet(
				GetInstance<SampleFlyingPetProjectile>(), GetInstance<SampleFlyingPetBuff>(), null);

			// To add a projectile attack to the combat pet, pass in a non-null third parameter
			AmuletOfManyMinionsApi.RegisterFlyingPet(
				GetInstance<SampleFlyingRangedPetProjectile>(), GetInstance<SampleFlyingRangedPetBuff>(), ProjectileID.FrostDaggerfish);

			// Register melee, grounded cross mod pet
			AmuletOfManyMinionsApi.RegisterGroundedPet(
				GetInstance<SampleGroundedPetProjectile>(), GetInstance<SampleGroundedPetBuff>(), null);

			// Add a ranged attack
			AmuletOfManyMinionsApi.RegisterGroundedPet(
				GetInstance<SampleGroundedRangedPetProjectile>(), GetInstance<SampleGroundedRangedPetBuff>(), ProjectileID.PoisonDartBlowgun);

		}

	}
}
