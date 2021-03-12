using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class ImpMinionBuff : MinionBuff
	{
		public ImpMinionBuff() : base(ProjectileType<ImpMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Imp");
			Description.SetDefault("A winged acorn will fight for you!");
		}
	}

	public class ImpMinionItem : MinionItem<ImpMinionBuff, ImpMinion>
	{
		public override string Texture => "Terraria/Item_" + ItemID.ImpStaff;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Imp Staff");
			Tooltip.SetDefault("Summons a winged acorn to fight for you!");
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.ImpStaff);
			base.SetDefaults();
		}
	}

	public class ImpFireball : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.ImpFireball;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.ImpFireball);
			base.SetDefaults();
			projectile.localNPCHitCooldown = 30;
			projectile.usesLocalNPCImmunity = true;
		}

		public override void PostAI()
		{
			for (int i = 0; i < 2; i++)
			{
				int dustId = Dust.NewDust(
					projectile.position, 
					projectile.width, projectile.height, 6, 
					projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 
					100, default, 2f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity.X *= 0.3f;
				Main.dust[dustId].velocity.Y *= 0.3f;
				Main.dust[dustId].noLight = true;
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 240);
		}
	}

	public class ImpMinion : HoverShooterMinion
	{
		protected override int BuffId => BuffType<ImpMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.FlyingImp;

		internal override int? FiredProjectileId => ProjectileType<ImpFireball>();

		internal override LegacySoundStyle ShootSound => SoundID.Item20;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Imp");
			Main.projFrames[projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 26;
			targetSearchDistance = 700;
			drawOffsetX = (projectile.width - 44) / 2;
			attackFrames = 80;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= 4)
				{
					projectile.frame = 0;
				}
			}
			if(vectorToTarget is Vector2 target)
			{
				projectile.spriteDirection = -Math.Sign(target.X);

			} else if(projectile.velocity.X > 1)
			{
				projectile.spriteDirection = -1;
			}
			else if (projectile.velocity.X < -1)
			{
				projectile.spriteDirection = 1;
			}
			projectile.rotation = projectile.velocity.X * 0.05f;

			// vanilla code for sparkly dust
			if (Main.rand.Next(6) == 0)
			{
				int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 6, 0f, 0f, 100, default, 2f);
				Main.dust[dustId].velocity *= 0.3f;
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].noLight = true;
			}

		}
	}
}
