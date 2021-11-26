using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BabySnowmanMinionBuff : CombatPetVanillaCloneBuff
	{
		public BabySnowmanMinionBuff() : base(ProjectileType<BabySnowmanMinion>()) { }
		public override string VanillaBuffName => "BabySnowman";
		public override int VanillaBuffId => BuffID.BabySnowman;
	}

	public class BabySnowmanMinionItem : CombatPetMinionItem<BabySnowmanMinionBuff, BabySnowmanMinion>
	{
		internal override string VanillaItemName => "ToySled";
		internal override int VanillaItemID => ItemID.ToySled;
	}

	public class SnowmanPetSnowballProjectile : WeakPumpkinBomb
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.SnowBallFriendly;
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
			for(int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.position, 16, 16, 76);
			}
		}
	}

	public class BabySnowmanMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.BabySnowman;
		internal override int BuffId => BuffType<BabySnowmanMinionBuff>();
		internal override int? ProjId => ProjectileType<SnowmanPetSnowballProjectile>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -16, -16, -1);
			ConfigureFrames(7, (0, 0), (0, 4), (1, 1), (6, 6));
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			DoSimpleFlyingDust();
		}
	}
}
