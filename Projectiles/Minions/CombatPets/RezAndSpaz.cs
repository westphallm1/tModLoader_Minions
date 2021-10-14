using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Squires.SeaSquire;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets
{
	public class RezAndSpazMinionBuff : CombatPetVanillaCloneBuff
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// the occasional case where it doesn't work on load
			projectileTypes = new int[] { ProjectileType<RezMinion>(), ProjectileType<SpazMinion>() };
		}

		public override int VanillaBuffId => BuffID.TwinsPet;

		public override string VanillaBuffName => "TwinsPet";
	}

	public class RezAndSpazMinionItem : CombatPetMinionItem<RezAndSpazMinionBuff, RezMinion>
	{
		internal override int VanillaItemID => ItemID.TwinsPetItem;

		internal override string VanillaItemName => "TwinsPetItem";
	}

	public class RezMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<RezAndSpazMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.TwinsPet;
		internal override int? FiredProjectileId => ProjectileType<MiniTwinsLaser>();
		internal override LegacySoundStyle ShootSound => new LegacySoundStyle(2, 10).WithVolume(.5f);

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 36;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			circleHelper.idleBumbleFrames = 90;
			circleHelper.idleBumbleRadius = 96;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			int attackCycleFrame = animationFrame - hsHelper.lastShootFrame;
			if(attackCycleFrame == attackFrames / 3)
			{
				Vector2 lineOfFire = vectorToTargetPosition;
				lineOfFire.SafeNormalize();
				lineOfFire *= hsHelper.projectileVelocity;
				if(player.whoAmI == Main.myPlayer)
				{
					hsHelper.FireProjectile(lineOfFire, (int)FiredProjectileId, 0);
				}
				AfterFiringProjectile();
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(18, 24);
			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}
	}

	public class SpazMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<RezAndSpazMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.TwinsPet;
		internal override int? FiredProjectileId => ProjectileType<MiniEyeFire>();
		internal override LegacySoundStyle ShootSound => new LegacySoundStyle(2, 34).WithVolume(.5f);

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 36;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			circleHelper.idleBumbleFrames = 90;
			circleHelper.idleBumbleRadius = 96;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			hsHelper.projectileVelocity = 6;
			base.TargetedMovement(vectorToTargetPosition);
			int attackCycleFrame = animationFrame - hsHelper.lastShootFrame;
			if(attackCycleFrame < attackFrames / 2 && attackFrames % 6 == 0)
			{
				Vector2 lineOfFire = vectorToTargetPosition;
				lineOfFire.SafeNormalize();
				lineOfFire *= hsHelper.projectileVelocity;
				lineOfFire += Projectile.velocity / 3;
				if(player.whoAmI == Main.myPlayer)
				{
					hsHelper.FireProjectile(lineOfFire, (int)FiredProjectileId, attackCycleFrame % 18);
				}
				AfterFiringProjectile();
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(0, 6);
			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}
	}
}
