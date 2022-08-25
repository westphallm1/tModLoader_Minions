using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace AmuletOfManyMinions.CrossModSystem.Internal.AssortedCrazyThings
{
	internal class BookShotCloneProj: MinionShotVanillaCloneProjectile
	{
		internal override int VanillaProjID => ProjectileID.BookStaffShot;
	}

	internal class AmethystBoltCloneProj: MinionShotVanillaCloneProjectile
	{
		internal override int VanillaProjID => ProjectileID.AmethystBolt;
	}

	internal class VortexAcidCloneProj: MinionShotVanillaCloneProjectile
	{
		internal override int VanillaProjID => ProjectileID.VortexAcid;
	}

	internal class GrenadeCloneProj: MinionShotVanillaCloneProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Dynamite;
		internal override int VanillaProjID => ProjectileID.Grenade;
	}

	internal class ElectricBoltCloneProj: MinionShotVanillaCloneProjectile
	{
		internal override int VanillaProjID => ProjectileID.MartianTurretBolt;
	}

	internal class SandBallCloneProj: MinionShotVanillaCloneProjectile
	{
		internal override int VanillaProjID => ProjectileID.SandBallGun;
		public override bool PreKill(int timeLeft) => false; // do not spawn a sandblock
	}

	internal class ShadowflameKnifeCloneProj: MinionShotVanillaCloneProjectile
	{
		internal override int VanillaProjID => ProjectileID.ShadowFlameKnife;
	}

	internal class BeeCloneProj: MinionShotVanillaCloneProjectile
	{
		internal override int VanillaProjID => ProjectileID.Bee;
	}

	internal class CursedFlameCloneProj : MinionShotVanillaCloneProjectile
	{
		internal override int VanillaProjID => ProjectileID.CursedFlameFriendly;

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = 90;
			Projectile.penetrate = 2;
		}
	}

}
