using AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using static AmuletOfManyMinions.CrossModSystem.Internal.InternalCrossModCallWrapper;

namespace AmuletOfManyMinions.CrossModSystem.Internal.AssortedCrazyThings
{
	// Special thanks to the mod authors for open sourcing this
	internal class AssortedCrazyThingsCrossMod : ModSystem
	{

		private InternalCrossModCallWrapper Calls;
		public override void PostSetupContent()
		{
			Calls = new InternalCrossModCallWrapper("AssortedCrazyThings");

			// Register ACT's flying pets
			// This would work nicely as a spreadsheet, but alas.
			Calls.RegisterFlyingPet("AlienHornetProj", "AlienHornetBuff", PT<VortexAcidCloneProj>());
			Calls.RegisterFlyingPet("AnimatedTomeProj", "AnimatedTomeBuff", PT<BookShotCloneProj>());
			Calls.RegisterFlyingPet("PetGolemHeadProj", "PetGolemHeadBuff", Calls.FindProj("PetGolemHeadFireball")?.Type);
			Calls.RegisterFlyingPet("DrumstickElementalProj", "DrumstickElementalBuff", null);
			Calls.RegisterFlyingPet("PigronataProj", "PigronataBuff", null);
			Calls.RegisterFlyingPet("AnomalocarisProj", "AnomalocarisBuff", null);
			Calls.RegisterFlyingPet("BabyCrimeraProj", "BabyCrimeraBuff", null);
			Calls.RegisterFlyingPet("BabyIchorStickerProj", "BabyIchorStickerBuff", null);
			Calls.RegisterFlyingPet("BabyOcramProj", "BabyOcramBuff", null);
			Calls.RegisterFlyingPet("BrainofConfusionProj", "BrainofConfusionBuff", null);
			Calls.RegisterFlyingPet("CursedSkullProj", "CursedSkullBuff", null);
			Calls.RegisterFlyingPet("DemonHeartProj", "DemonHeartBuff", null);
			Calls.RegisterFlyingPet("DetachedHungryProj", "DetachedHungryBuff", null);
			Calls.RegisterFlyingPet("DocileDemonEyeProj", "DocileDemonEyeBuff", null);
			Calls.RegisterFlyingPet("EnchantedSwordProj", "EnchantedSwordBuff", null);
			Calls.RegisterFlyingPet("GhostMartianProj", "GhostMartianBuff", PT<ElectricBoltCloneProj>());
			Calls.RegisterFlyingPet("PetCultistProj", "PetCultistBuff", PT<ElectricBoltCloneProj>());
			Calls.RegisterFlyingPet("PetFishronProj", "PetFishronBuff", PT<SharkPupBubble>());
			Calls.RegisterFlyingPet("PetHarvesterProj", "PetHarvesterBuff", null);
			Calls.RegisterFlyingPet("QueenLarvaProj", "QueenLarvaBuff", PT<BeeCloneProj>());
			Calls.RegisterFlyingPet("TorturedSoulProj", "TorturedSoulBuff", null);
			Calls.RegisterFlyingPet("VampireBatProj", "VampireBatBuff", null);
			Calls.RegisterFlyingPet("YoungHarpyProj", "YoungHarpyBuff", PT<LilHarpyFeather>());


			// Register ACT's grounded pets
			Calls.RegisterGroundedPet("CuteLamiaPetProj", "CuteLamiaPetBuff", PT<AmethystBoltCloneProj>());
			Calls.RegisterGroundedPet("DynamiteBunnyProj", "DynamiteBunnyBuff", PT<GrenadeCloneProj>());
			Calls.RegisterGroundedPet("GobletProj", "GobletBuff", PT<ShadowflameKnifeCloneProj>());
			Calls.RegisterGroundedPet("LilWrapsProj", "LilWrapsBuff", null);
			Calls.RegisterGroundedPet("LifelikeMechanicalFrogProj", "LifelikeMechanicalFrogBuff", null);
			Calls.RegisterGroundedPet("MiniAntlionProj", "MiniAntlionBuff", PT<SandBallCloneProj>());
			Calls.RegisterGroundedPet("MiniMegalodonProj", "MiniMegalodonBuff", PT<SharkPupBubble>());
			Calls.RegisterGroundedPet("MetroidPetProj", "MetroidPetBuff", PT<SharkPupBubble>());
			Calls.RegisterGroundedPet("NumberMuncherProj", "NumberMuncherBuff", PT<ElectricBoltCloneProj>());
			Calls.RegisterGroundedPet("PetGoldfishProj", "PetGoldfishBuff", null);
			Calls.RegisterGroundedPet("StrangeRobotProj", "StrangeRobotBuff", PT<ElectricBoltCloneProj>());
			Calls.RegisterGroundedPet("SuspiciousNuggetProj", "SuspiciousNuggetBuff", null);
			Calls.RegisterGroundedPet("YoungWyvernProj", "YoungWyvernBuff", PT<CloudPuffProjectile>());
		}



	}
}
