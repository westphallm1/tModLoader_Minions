using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Squires.SeaSquire;
using System;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class LilHarpyMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<LilHarpyMinion>() };
		public override string VanillaBuffName => "LilHarpy";
		public override int VanillaBuffId => BuffID.LilHarpy;
	}

	public class LilHarpyMinionItem : CombatPetMinionItem<LilHarpyMinionBuff, LilHarpyMinion>
	{
		internal override string VanillaItemName => "BirdieRattle";
		internal override int VanillaItemID => ItemID.BirdieRattle;
	}

	public class LilHarpyFeather : StingerProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.HarpyFeather;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.HarpyFeather];
		}

		public override void SpawnDust() { } // no-op
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) { } // no-op
	}

	public class LilHarpyMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<LilHarpyMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LilHarpy;
		internal override int? FiredProjectileId => ProjectileType<LilHarpyFeather>();
		internal override LegacySoundStyle ShootSound => SoundID.Item17;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.LilHarpy];
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			forwardDir = -1;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(0, 6);
		}

	}
}
