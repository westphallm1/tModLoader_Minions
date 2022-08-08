using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using AmuletOfManyMinions.Core.Minions.Effects;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.Pirate;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Core;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class DynamiteKittenMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<DynamiteKittenMinion>() };
		public override int VanillaBuffId => BuffID.DynamiteKitten;
		public override string VanillaBuffName => "DynamiteKitten";
	}

	public class DynamiteKittenMinionItem : CombatPetMinionItem<DynamiteKittenMinionBuff, DynamiteKittenMinion>
	{
		internal override int VanillaItemID => ItemID.BallOfFuseWire;
		internal override string VanillaItemName => "BallOfFuseWire";
		internal override int AttackPatternUpdateTier => (int)CombatPetTier.Spectre;
	}


	public class DynamiteKittenGrenade: WeakPumpkinBomb
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Grenade;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = 1;
			bounces = 3;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			// damage comes from explosion rather than projectile itself
			damage = 1;
		}

		public override void Kill(int timeLeft)
		{
			PirateCannonball.SpawnSmallExplosionOnProjDeath(Projectile);
		}
	}

	public class DynamiteKittenBullet : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Summon;
			Projectile.timeLeft = 120;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
			Projectile.width = 16;
			Projectile.height = 16;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				texture.Bounds, Color.White, Projectile.rotation, texture.Bounds.Center.ToVector2(), 1, 0, 0);
			return false;
		}
	}

	public class DynamiteKittenRocket : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketI;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Summon;
			Projectile.timeLeft = 120;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
			Projectile.width = 16;
			Projectile.height = 16;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			for(int i = 0; i < 2; i++)
			{
				Vector2 offset = Projectile.velocity * i * 0.5f;
				if (Main.rand.NextBool(2))
				{
					int dustIdx = Dust.NewDust(Projectile.position - offset, Projectile.width, Projectile.height, 6, Alpha: 100);
					Main.dust[dustIdx].scale *= Main.rand.NextFloat(1.4f, 2.4f);
					Main.dust[dustIdx].velocity *= 0.2f;
					Main.dust[dustIdx].noGravity = true;
				}
				if (Main.rand.NextBool(2))
				{
					int dustIdx = Dust.NewDust(Projectile.position - offset, Projectile.width, Projectile.height, 31, Alpha: 100, Scale: 0.5f);
					Main.dust[dustIdx].fadeIn *= Main.rand.NextFloat(0.5f, 1f);
					Main.dust[dustIdx].velocity *= 0.05f;
				}
			}
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			// damage comes from explosion rather than projectile itself
			damage = 1;
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			hitbox.Inflate(8, 8);
		}

		public override void Kill(int timeLeft)
		{
			PirateCannonball.SpawnSmallExplosionOnProjDeath(Projectile);
		}
	}

	public class DynamiteKittenMinion : WeaponHoldingCatMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DynamiteKitten;
		public override int BuffId => BuffType<DynamiteKittenMinionBuff>();

		// scale attack type rather than attack speed
		internal override int GetAttackFrames(ICombatPetLevelInfo info) => Math.Max(45, 60 - 4 * info.Level);

		internal override int? ProjId => levelInfo?.ProjectileId ?? 0;

		private static CatPetLevelInfo[] DynamiteKittenLevelInfo;
		internal override CatPetLevelInfo[] CatPetLevels => DynamiteKittenLevelInfo;

		internal override float ModifyProjectileDamage(ICombatPetLevelInfo info)
		{
			return info.Level >= 3 && info.Level <= 5 ? 0.75f : 1.25f;
		}

		public override void LoadAssets()
		{
			Main.instance.LoadItem(ItemID.Boomstick);
			Main.instance.LoadItem(ItemID.Flamethrower);
			Main.instance.LoadItem(ItemID.RocketLauncher);
			DynamiteKittenLevelInfo = new CatPetLevelInfo[]
			{
				new(0, 0, ProjectileType<DynamiteKittenGrenade>(), 6),
				new(3, ItemID.Boomstick, ProjectileType<DynamiteKittenBullet>(), 12),
				new(5, ItemID.Flamethrower, ProjectileType<ItsyBetsyFire>(), 6),
				// lots of extra updates
				new(6, ItemID.RocketLauncher, ProjectileType<DynamiteKittenRocket>(), 12),
			};
		}

		public override void Unload()
		{
			DynamiteKittenLevelInfo = null;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(30, 24, -12, -14, -1);
			ConfigureFrames(14, (0, 0), (2, 9), (1, 1), (10, 13));
			weaponDrawer.WeaponHoldDistance = 24;
		}

		public override void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			// move flames along with the minion's velocity
			if(levelInfo.ItemId == ItemID.Flamethrower)
			{
				launchVector += Projectile.velocity / 3;
			} else if (levelInfo.ItemId == ItemID.Boomstick)
			{
				for(int i = 0; i < 2; i++)
				{
					base.LaunchProjectile(launchVector.RotatedByRandom(MathHelper.Pi/8), ai0);
				}
			}
			base.LaunchProjectile(launchVector, ai0);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			int attackCycleFrame = AnimationFrame - lastFiredFrame;
			weaponDrawer.AttackDuration = 30;
			weaponDrawer.WeaponOffset = new Vector2(0, 6);
			if(levelInfo.ItemId == ItemID.Flamethrower)
			{
				weaponDrawer.AttackDuration = attackFrames;
				if(weaponDrawer.lastAttackVector != default)
				{
					weaponDrawer.lastAttackVector = vectorToTargetPosition;
				}
				bool shouldLaunch = Player.whoAmI == Main.myPlayer && attackCycleFrame > 1 &&
					attackCycleFrame < attackFrames * 0.75f && attackCycleFrame % 6 == 0;
				if (shouldLaunch)
				{
					Vector2 launchVector = vectorToTargetPosition;
					// todo lead shot
					launchVector.SafeNormalize();
					launchVector *= launchVelocity;
					LaunchProjectile(launchVector, attackCycleFrame % 18);
				}
			} else if (levelInfo.ItemId == ItemID.RocketLauncher)
			{
				weaponDrawer.WeaponOffset = Vector2.Zero;
			}
		}
	}
}
