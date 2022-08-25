using AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
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
			//if(!ServerConfig.Instance.EnableACTCombatPets)
			//{
			//	return;
			//}
			//Calls = new InternalCrossModCallWrapper(Mod, "AssortedCrazyThings");

			//// Register ACT's flying pets
			//// This would work nicely as a spreadsheet, but alas.
			//Calls.RegisterFlyingPet("AlienHornetProj", "AlienHornetBuff", PT<VortexAcidCloneProj>());
			//Calls.RegisterFlyingPet("AnimatedTomeProj", "AnimatedTomeBuff", PT<BookShotCloneProj>());
			//Calls.RegisterFlyingPet("ChunkyProj", "ChunkyandMeatballBuff", null);
			//Calls.RegisterFlyingPet("DrumstickElementalProj", "DrumstickElementalBuff", null);
			//Calls.RegisterFlyingPet("AnomalocarisProj", "AnomalocarisBuff", null);
			//Calls.RegisterFlyingPet("BabyCrimeraProj", "BabyCrimeraBuff", null);
			//Calls.RegisterFlyingPet("BabyIchorStickerProj", "BabyIchorStickerBuff", null);
			//Calls.RegisterFlyingPet("BabyOcramProj", "BabyOcramBuff", null);
			//Calls.RegisterFlyingPet("BrainofConfusionProj", "BrainofConfusionBuff", null);
			//Calls.RegisterFlyingPet("CursedSkullProj", "CursedSkullBuff", null);
			//Calls.RegisterFlyingPet("DemonHeartProj", "DemonHeartBuff", null);
			//Calls.RegisterFlyingPet("DetachedHungryProj", "DetachedHungryBuff", null);
			//Calls.RegisterFlyingPet("DocileDemonEyeProj", "DocileDemonEyeBuff", null);
			//Calls.RegisterFlyingPet("EnchantedSwordProj", "EnchantedSwordBuff", null);
			//Calls.RegisterFlyingPet("GhostMartianProj", "GhostMartianBuff", PT<ElectricBoltCloneProj>());
			//Calls.RegisterFlyingPet("MeatballProj", "ChunkyandMeatballBuff", null);
			//Calls.RegisterFlyingPet("PetCultistProj", "PetCultistBuff", PT<ElectricBoltCloneProj>());
			//Calls.RegisterFlyingPet("PetFishronProj", "PetFishronBuff", PT<SharkPupBubble>());
			//Calls.RegisterFlyingPet("PetGolemHeadProj", "PetGolemHeadBuff", Calls.FindProj("PetGolemHeadFireball")?.Type);
			//Calls.RegisterFlyingPet("PetHarvesterProj", "PetHarvesterBuff", null);
			//Calls.RegisterFlyingPet("PetQueenSlimeAirProj", "PetQueenSlimeBuff", null);
			//Calls.RegisterFlyingPet("PigronataProj", "PigronataBuff", null);
			//Calls.RegisterFlyingPet("SkeletronHandProj", "SkeletronHandBuff", null);
			//Calls.RegisterFlyingPet("SkeletronPrimeHandProj", "SkeletronPrimeHandBuff", null);
			//Calls.RegisterFlyingPet("QueenLarvaProj", "QueenLarvaBuff", PT<BeeCloneProj>());
			//Calls.RegisterFlyingPet("TinyRetinazerProj", "TinyTwinsBuff", PT<MiniTwinsLaser>());
			//Calls.RegisterFlyingPet("TinySpazmatismProj", "TinyTwinsBuff", PT<CursedFlameCloneProj>());
			//Calls.RegisterFlyingPet("TorturedSoulProj", "TorturedSoulBuff", null);
			//Calls.RegisterFlyingPet("VampireBatProj", "VampireBatBuff", null);
			//Calls.RegisterFlyingPet("YoungHarpyProj", "YoungHarpyBuff", PT<LilHarpyFeather>());
			//Calls.RegisterFlyingPet("PetPlanteraProj", "PetPlanteraBuff", PT<PlanteraSeedlingThornBall>());
			//Calls.RegisterFlyingPet("WallFragmentMouth", "WallFragmentBuff", null);
			//Calls.RegisterFlyingPet("WallFragmentEye1", "WallFragmentBuff", null);
			//Calls.RegisterFlyingPet("WallFragmentEye2", "WallFragmentBuff", null);


			//// Register ACT's grounded pets
			//Calls.RegisterGroundedPet("CuteLamiaPetProj", "CuteLamiaPetBuff", PT<AmethystBoltCloneProj>());
			//Calls.RegisterGroundedPet("DynamiteBunnyProj", "DynamiteBunnyBuff", PT<DynamiteKittenGrenade>());
			//Calls.RegisterGroundedPet("GobletProj", "GobletBuff", PT<ShadowflameKnifeCloneProj>());
			//Calls.RegisterGroundedPet("LilWrapsProj", "LilWrapsBuff", null);
			//Calls.RegisterGroundedPet("MiniAntlionProj", "MiniAntlionBuff", PT<SandBallCloneProj>());
			//Calls.RegisterGroundedPet("MiniMegalodonProj", "MiniMegalodonBuff", PT<SharkPupBubble>());
			//Calls.RegisterGroundedPet("MetroidPetProj", "MetroidPetBuff", PT<SharkPupBubble>());
			//Calls.RegisterGroundedPet("NumberMuncherProj", "NumberMuncherBuff", PT<ElectricBoltCloneProj>());
			//Calls.RegisterGroundedPet("PetGoldfishProj", "PetGoldfishBuff", null);
			//Calls.RegisterGroundedPet("StrangeRobotProj", "StrangeRobotBuff", PT<ElectricBoltCloneProj>());
			//Calls.RegisterGroundedPet("SuspiciousNuggetProj", "SuspiciousNuggetBuff", null);
			//Calls.RegisterGroundedPet("YoungWyvernProj", "YoungWyvernBuff", PT<CloudPuffProjectile>());

			//// Register ACT's slime pets
			//Calls.RegisterSlimePet("AbeeminationProj", "AbeeminationBuff", null, true);
			//Calls.RegisterSlimePet("ChunkySlimeProj", "ChunkySlimeBuff", null);
			//Calls.RegisterSlimePet("FailureSlimeProj", "FailureSlimeBuff", null);
			//Calls.RegisterSlimePet("FairySlimeProj", "FairySlimeBuff", null);
			//Calls.RegisterSlimePet("HornedSlimeProj", "HornedSlimeBuff", null);
			//Calls.RegisterSlimePet("IlluminantSlimeProj", "IlluminantSlimeBuff", null);
			//Calls.RegisterSlimePet("JoyousSlimeProj", "JoyousSlimeBuff", null);
			//Calls.RegisterSlimePet("LifelikeMechanicalFrogProj", "LifelikeMechanicalFrogBuff", null);
			//Calls.RegisterSlimePet("MeatballSlimeProj", "MeatballSlimeBuff", null);
			//Calls.RegisterSlimePet("OceanSlimeProj", "OceanSlimeBuff", null);
			//Calls.RegisterSlimePet("OceanSlimeProj", "OceanSlimeBuff", null);
			//Calls.RegisterSlimePet("PetQueenSlimeGround1Proj", "PetQueenSlimeBuff", null, true);
			//Calls.RegisterSlimePet("PetQueenSlimeGround2Proj", "PetQueenSlimeBuff", null, true);
			//Calls.RegisterSlimePet("PrinceSlimeProj", "PrinceSlimeBuff", null);
			//Calls.RegisterSlimePet("RainbowSlimeProj", "RainbowSlimeBuff", null);
			//Calls.RegisterSlimePet("StingSlimeProj", "StingSlimeBuff", null);
			//Calls.RegisterSlimePet("TurtleSlimeProj", "TurtleSlimeBuff", null);
		}



	}
}
