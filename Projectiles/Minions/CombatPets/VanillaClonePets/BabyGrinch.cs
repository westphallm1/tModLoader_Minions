using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Core.BackportUtils;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BabyGrinchMinionBuff : CombatPetVanillaCloneBuff
	{
		public BabyGrinchMinionBuff() : base(ProjectileType<BabyGrinchMinion>()) { }
		public override string VanillaBuffName => "BabyGrinch";
		public override int VanillaBuffId => BuffID.BabyGrinch;
	}

	public class BabyGrinchMinionItem : CombatPetMinionItem<BabyGrinchMinionBuff, BabyGrinchMinion>
	{
		internal override string VanillaItemName => "BabyGrinchMischiefWhistle";
		internal override int VanillaItemID => ItemID.BabyGrinchMischiefWhistle;
	}

	public class BabyGrinchOrnament : WeakPumpkinBomb
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.OrnamentFriendly;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.frame = Main.rand.Next(4);
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
			for (int i = 0; i < 10; i++)
			{
				int dustType = 90 - Projectile.frame;
				int dustIdx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType);
				Main.dust[dustIdx].noLight = true;
				Main.dust[dustIdx].scale = 0.8f;
			}
		}
	}
	public class BabyGrinchMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.BabyGrinch;
		internal override int BuffId => BuffType<BabyGrinchMinionBuff>();
		internal override int? ProjId => ProjectileType<BabyGrinchOrnament>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(16, 32, -16, -32, -1);
			ConfigureFrames(14, (0, 0), (2, 9), (1, 1), (10, 14));
		}
	}
}
