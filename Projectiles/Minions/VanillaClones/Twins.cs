using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class TwinsMinionBuff : MinionBuff
	{
		public TwinsMinionBuff() : base(ProjectileType<MiniRetinazerMinion>(), ProjectileType<MiniSpazmatismMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Twins");
			Description.SetDefault("A winged acorn will fight for you!");
		}
	}

	public class TwinsMinionItem : MinionItem<TwinsMinionBuff, MiniRetinazerMinion>
	{
		public override string Texture => "Terraria/Item_" + ItemID.OpticStaff;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Twins Staff");
			Tooltip.SetDefault("Summons a winged acorn to fight for you!");
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.OpticStaff);
			base.SetDefaults();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
			Projectile.NewProjectile(position, Vector2.Zero, ProjectileType<MiniSpazmatismMinion>(), damage, knockBack, player.whoAmI);
			return true;
		}
	}

	public class MiniEyeFire : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.EyeFire;
		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.Flames);
			projectile.aiStyle = 0; // unset default flames AI
			base.SetDefaults();
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 10;
			projectile.timeLeft = 42;
		}
		public override void AI()
		{
			base.AI();
			projectile.localAI[0]++;
			if(projectile.localAI[0] < 8 || Main.rand.Next(2) != 0)
			{
				return;
			}
			float dustScale = Math.Min(1, 0.25f * (projectile.localAI[0] - 7));
			int dustType = 75;
			int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100);
			Main.dust[dustId].scale *= 1.5f * dustScale;
			Main.dust[dustId].velocity.X *= 1.2f;
			Main.dust[dustId].velocity.Y *= 1.2f;
			Main.dust[dustId].noGravity = true;
			if (Main.rand.Next(3) == 0)
			{
				Main.dust[dustId].scale *= 3f;
				Main.dust[dustId].velocity.X *= 2f;
				Main.dust[dustId].velocity.Y *= 2f;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.CursedInferno, 300);
		}
	}

	public class MiniTwinsLaser : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.MiniRetinaLaser;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
			Main.projFrames[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.MiniRetinaLaser);
			base.SetDefaults();
		}

		public override void AI()
		{
			base.AI();
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			Texture2D texture = GetTexture(Texture);
			Rectangle bounds = texture.Bounds;
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, Color.White, r,
				origin, 1, 0, 0);
			return false;
		}
	}

	public class MiniRetinazerMinion : HoverShooterMinion
	{
		protected override int BuffId => BuffType<TwinsMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.Retanimini;

		internal override int? FiredProjectileId => ProjectileType<MiniTwinsLaser>();

		internal override LegacySoundStyle ShootSound => SoundID.Item33.WithVolume(.5f);
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Twins");
			Main.projFrames[projectile.type] = 3;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.minionSlots = 0.5f;
			projectile.width = 24;
			projectile.height = 24;
			attackFrames = 90;
			travelSpeed = 12;
			targetSearchDistance = 850;
			projectileVelocity = 24;
			targetInnerRadius = 208;
			targetOuterRadius = 264;
		}
		public override void OnSpawn()
		{
			// cut down damage since it's got such a high rate of fire
			projectile.damage = (int)(projectile.damage * 0.75f);
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= Main.projFrames[projectile.type])
				{
					projectile.frame = 0;
				}
			}
			projectile.spriteDirection = 0;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			projectile.rotation = vectorToTargetPosition.ToRotation() + MathHelper.Pi;
			if(animationFrame - lastShootFrame == 16 || animationFrame - lastShootFrame == 32)
			{
				Vector2 lineOfFire = vectorToTargetPosition;
				lineOfFire.SafeNormalize();
				lineOfFire *= projectileVelocity;
				if(player.whoAmI == Main.myPlayer)
				{
					FireProjectile(lineOfFire, (int)FiredProjectileId);
				}
				AfterFiringProjectile();
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi;
		}
	}
	public class MiniSpazmatismMinion : HoverShooterMinion
	{
		private bool isDashing;
		private Vector2 dashVector;

		protected override int BuffId => BuffType<TwinsMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.Spazmamini;

		internal override int? FiredProjectileId => ProjectileType<MiniEyeFire>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Twins");
			Main.projFrames[projectile.type] = 3;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.minionSlots = 0.5f;
			projectile.width = 24;
			projectile.height = 24;
			attackFrames = 90;
			travelSpeed = 14;
			targetSearchDistance = 850;
			projectileVelocity = 6;
			targetInnerRadius = 96;
			targetOuterRadius = 160;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= Main.projFrames[projectile.type])
				{
					projectile.frame = 0;
				}
			}
			projectile.spriteDirection = 0;
		}

		public override void OnSpawn()
		{
			// cut down damage since it's got such a high rate of fire
			projectile.damage = (int)(projectile.damage * 0.67f);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int framesSinceShoot = animationFrame - lastShootFrame;
			if((framesSinceShoot > 45 && framesSinceShoot < 55) || (framesSinceShoot > 75 && framesSinceShoot < 85))
			{
				// dash at the target
				isDashing = true;
				if(dashVector == default)
				{
					dashVector = vectorToTargetPosition;
					dashVector.SafeNormalize();
					dashVector *= (travelSpeed + 2);
					projectile.velocity = dashVector;
				}
			} else
			{
				dashVector = default;
				isDashing = false;
				base.TargetedMovement(vectorToTargetPosition);
			}
			projectile.rotation = vectorToTargetPosition.ToRotation() + MathHelper.Pi;
			if(framesSinceShoot < 45 && framesSinceShoot % 6 == 0)
			{
				Vector2 lineOfFire = vectorToTargetPosition;
				lineOfFire.SafeNormalize();
				lineOfFire *= projectileVelocity;
				if(player.whoAmI == Main.myPlayer)
				{
					FireProjectile(lineOfFire, (int)FiredProjectileId);
				}
				AfterFiringProjectile();
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			isDashing = false;
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// need to draw sprites manually for some reason
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 128);
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : 0;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			// motion blur
			if(isDashing)
			{
				// lifted from ExampleMod's ExampleBullet
				for (int k = 0; k < projectile.oldPos.Length; k++)
				{
					Vector2 blurPos = projectile.oldPos[k] - Main.screenPosition + origin + new Vector2(0f, projectile.gfxOffY);
					Color color = projectile.GetAlpha(lightColor) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
					spriteBatch.Draw(texture, blurPos,
						bounds, color, r,
						origin, 1, effects, 0);
				}
			}
			// regular version
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r, origin, 1, effects, 0);
			return false;
		}
	}
}
