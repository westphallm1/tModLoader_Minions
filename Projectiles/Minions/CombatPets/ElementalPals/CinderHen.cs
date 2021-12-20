using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class CinderHenMinionBuff : CombatPetVanillaCloneBuff
	{
		public CinderHenMinionBuff() : base(ProjectileType<CinderHenMinion>()) { }
		public override string VanillaBuffName => "BabyWerewolf";
		public override int VanillaBuffId => BuffID.BabyWerewolf;
	}

	public class CinderHenMinionItem : CombatPetMinionItem<CinderHenMinionBuff, CinderHenMinion>
	{
		internal override string VanillaItemName => "FullMoonSqueakyToy";
		internal override int VanillaItemID => ItemID.FullMoonSqueakyToy;
	}

	public class CinderHenMinion : CombatPetGroundedRangedMinion
	{
		internal override int BuffId => BuffType<CinderHenMinionBuff>();
		internal override bool ShouldDoShootingMovement => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Skeletal;

		internal override int? ProjId => ProjectileType<ImpFireball>();

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -4, -6, -1);
			ConfigureFrames(14, (0, 1), (2, 9), (2, 2), (10, 13));
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.GetAnimationState();
			frameSpeed = (state == GroundAnimationState.STANDING) ? 10 : 5;
			base.Animate(minFrame, maxFrame);
		}
	}
}
