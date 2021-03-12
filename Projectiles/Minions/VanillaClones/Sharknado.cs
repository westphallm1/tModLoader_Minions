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
	public class SharknadoMinionBuff : MinionBuff
	{
		public SharknadoMinionBuff() : base(ProjectileType<SharknadoMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Sharknado");
			Description.SetDefault("A winged acorn will fight for you!");
		}
	}

	public class SharknadoMinionItem : MinionItem<SharknadoMinionBuff, SharknadoMinion>
	{
		public override string Texture => "Terraria/Item_" + ItemID.TempestStaff;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Sharknado Staff");
			Tooltip.SetDefault("Summons a winged acorn to fight for you!");
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.TempestStaff);
			base.SetDefaults();
		}
	}

	public class MiniSharknado : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.MiniSharkron;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
			Main.projFrames[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.MiniSharkron);
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			projectile.localNPCHitCooldown = 30;
			projectile.usesLocalNPCImmunity = true;
			projectile.penetrate = 3;
		}

		public override void AI()
		{
			base.AI();
			projectile.rotation = projectile.velocity.ToRotation();
			projectile.frameCounter++;
			if(projectile.frameCounter >= 5)
			{
				projectile.frame += 1;
				projectile.frame %= 2;
				projectile.frameCounter = 0;
			}
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 15; i++)
			{
				int dustId = Dust.NewDust(projectile.Center - Vector2.One * 10f, 50, 50, 5, 0f, -2f);
				Main.dust[dustId].velocity /= 2f;
			}
			int goreId = Gore.NewGore(projectile.Center, projectile.velocity * 0.8f, 584);
			Main.gore[goreId].timeLeft /= 4;
			goreId = Gore.NewGore(projectile.Center, projectile.velocity * 0.9f, 585);
			Main.gore[goreId].timeLeft /= 4;
			goreId = Gore.NewGore(projectile.Center, projectile.velocity, 586);
			Main.gore[goreId].timeLeft /= 4;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// need to draw sprites manually for some reason
			Color translucentColor = new Color(lightColor.R/5, lightColor.G/5, lightColor.B, 128);
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : 0;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			// motion blur
			for(int i = 0; i < 2; i ++)
			{
				Vector2 blurPos = pos - projectile.velocity * 2 * (2 - i);
				float scale = 0.7f * 0.125f * i;
				spriteBatch.Draw(texture, blurPos - Main.screenPosition,
					bounds, translucentColor, r,
					origin, scale, effects, 0);
			}

			// regular
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
			return false;
		}
	}

	public class SharknadoMinion : HoverShooterMinion
	{
		protected override int BuffId => BuffType<SharknadoMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.Tempest;

		internal override int? FiredProjectileId => ProjectileType<MiniSharknado>();

		internal override LegacySoundStyle ShootSound => SoundID.Item20;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Sharknado");
			Main.projFrames[projectile.type] = 6;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 32;
			drawOffsetX = (projectile.width - 44) / 2;
			attackFrames = 60;
			travelSpeed = 12;
			targetSearchDistance = 900;
			projectileVelocity = 24;
			targetInnerRadius = 108;
			targetOuterRadius = 164;
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
			projectile.rotation = projectile.velocity.X * 0.05f;

			if (Main.rand.Next(5) == 0)
			{
				int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 217, 0f, 0f, 100, default, 2f);
				Main.dust[dustId].velocity *= 0.3f;
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].noLight = true;
			}
		}
	}
}
