using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SoulboundBow;
using AmuletOfManyMinions.Projectiles.Squires.SoulboundSword;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.SoulboundArsenal
{
	// uses ai[0] to track firing angle
	// angle and position manipulated by SoulboundArsenal
	class SoulboundArsenalLaser : ModProjectile
	{
		static int TimeToLive = 6 * 60;
		static int ChargeTime = 2 * 60;
		static int maxLength = 200 * 16;
		protected Vector2 endPoint = Vector2.Zero;

		protected float firingAngle => projectile.ai[0];
		protected int animationFrame => TimeToLive - projectile.timeLeft;

		protected virtual Color LightColor => Color.White;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.timeLeft = TimeToLive;
			projectile.penetrate = -1;
			projectile.friendly = true;
			projectile.width = 8;
			projectile.height = 8;
		}

		public override void AI()
		{
			Vector2 travelVector = firingAngle.ToRotationVector2();
			Vector2 current = projectile.Center;
			for(int i = 16; i < maxLength; i+= 16)
			{
				Vector2 next = projectile.Center + travelVector * i;
				if(!Collision.CanHitLine(current, 1, 1, next, 1, 1))
				{
					break;
				}
				current = next;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = Main.projectileTexture[projectile.type];
			float colorIntensity = Math.Min(1, MathHelper.Lerp(0, 1, animationFrame / (float)TimeToLive));
			return false;
		}
	}
}
