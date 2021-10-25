using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BabyPenguinMinionBuff : CombatPetVanillaCloneBuff
	{
		public BabyPenguinMinionBuff() : base(ProjectileType<BabyPenguinMinion>()) { }
		public override string VanillaBuffName => "BabyPenguin";
		public override int VanillaBuffId => BuffID.BabyPenguin;
	}

	public class BabyPenguinMinionItem : CombatPetMinionItem<BabyPenguinMinionBuff, BabyPenguinMinion>
	{
		internal override string VanillaItemName => "Fish";
		internal override int VanillaItemID => ItemID.Fish;
	}

	public class PenguinPetFishProjectile : WeakPumpkinBomb
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.Fish;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			bounces = 1;
		}

		public override void Kill(int timeLeft)
		{
			// TODO dust
		}
	}

	public class BabyPenguinMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Penguin;
		internal override int BuffId => BuffType<BabyPenguinMinionBuff>();
		internal override int? ProjId => ProjectileType<PenguinPetFishProjectile>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(16, 30, 0, 0, -1);
			ConfigureFrames(3, (0, 0), (0, 1), (0, 1), (0, 1));
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			DoSimpleFlyingDust();
		}
	}
}
