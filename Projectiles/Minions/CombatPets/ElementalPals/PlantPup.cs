using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class PlantPupMinionBuff : CombatPetVanillaCloneBuff
	{
		public PlantPupMinionBuff() : base(ProjectileType<PlantPupMinion>()) { }
		public override string VanillaBuffName => "BabyWerewolf";
		public override int VanillaBuffId => BuffID.BabyWerewolf;
	}

	public class PlantPupMinionItem : CombatPetMinionItem<PlantPupMinionBuff, PlantPupMinion>
	{
		internal override string VanillaItemName => "FullMoonSqueakyToy";
		internal override int VanillaItemID => ItemID.FullMoonSqueakyToy;
	}

	public class PlantPupMinion : CombatPetGroundedRangedMinion
	{
		internal override int BuffId => BuffType<PlantPupMinionBuff>();

		internal override bool ShouldDoShootingMovement => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Skeletal;

		internal override int? ProjId => ProjectileType<SaplingMinionLeafProjectile>();

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(30, 30, -6, -8, -1);
			ConfigureFrames(10, (0, 1), (2, 6), (3, 3), (7, 9));
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.GetAnimationState();
			frameSpeed = (state == GroundAnimationState.STANDING) ? 10 : 5;
			base.Animate(minFrame, maxFrame);
			if(state == GroundAnimationState.JUMPING)
			{
				Projectile.frame = Projectile.velocity.Y > 0 ? 2 : 5;
			}
		}
	}
}
