using AmuletOfManyMinions.Core.Minions.Effects;
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
		static int TimeToLive = 4 * 60;
		static int ChargeTime = 2 * 60;
		static int maxLength = 200 * 16;
		protected Vector2 endPoint = Vector2.Zero;
		private Vector2 tangent;
		private float chargeScale;

		protected float firingAngle => projectile.ai[0];
		protected int animationFrame => TimeToLive - projectile.timeLeft;

		protected virtual Color LightColor => Color.White;

		protected Rectangle GoodFrame(int idx, bool isLast)
		{
			int Y = isLast ? 0 : idx == 0 ? 44 : 22;
			return new Rectangle(0, Y, 22, 20);
		}
		protected Rectangle EvilFrame(int idx, bool isLast)
		{
			int Y = isLast ? 66 : idx == 0 ? 110 : 88;
			return new Rectangle(0, Y, 22, 20);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// Todo: Not O(n) solution
			Vector2 direction = endPoint - projectile.Center;
			float laserLength = direction.Length();
			direction.SafeNormalize();
			for(int i = 0; i < laserLength; i+= 8)
			{
				Vector2 checkPoint = projectile.Center + direction * i;
				if(targetHitbox.Contains(checkPoint.ToPoint())) 
				{
					return true;
				}
			}
			return false;
		}

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
			projectile.tileCollide = false;
			projectile.friendly = true;
			projectile.width = 1;
			projectile.height = 1;
		}

		public override void AI()
		{
			Vector2 travelVector = firingAngle.ToRotationVector2();
			endPoint = projectile.Center;
			chargeScale = Math.Min(1, MathHelper.Lerp(0, 1, animationFrame / (float)ChargeTime));
			int i;
			int step = 16;
			for(i = step; i < maxLength; i += step)
			{
				Vector2 next = projectile.Center + travelVector * i;
				if(!Collision.CanHitLine(endPoint, 1, 1, next, 1, 1))
				{
					if(step < 2)
					{
						break;
					} else
					{
						i -= step;
						step /= 2;
					}
				} 
				else
				{
					endPoint = next;
					if(Main.rand.Next((int)(20 * (3 - 2 * chargeScale))) == 0) {
						SpawnDust(endPoint, Vector2.Zero);
					}
				}
			}
			// LOTs of dust
			Vector2 direction = endPoint - projectile.Center;
			direction.SafeNormalize();
			tangent = new Vector2(direction.Y, -direction.X);
			int dustFrequency = (int)(5 * (4 - 3 * chargeScale));
			if(animationFrame % dustFrequency != 0)
			{
				for (i = -8; i <= 8; i += 8)
				{
					for (float j = 0; j < 2 * Math.PI; j += (float)Math.PI / 8)
					{
						Vector2 velocity = 1.5f * j.ToRotationVector2();
						SpawnDust(endPoint + tangent * i, velocity);
					}
				}
			}
			endPoint += travelVector * 16;
		}

		private void SpawnDust(Vector2 position, Vector2 velocity)
		{
			int dustCreated = Dust.NewDust(position, 1, 1, 255, velocity.X, velocity.Y, 50, default, Scale: 1.4f);
			if (Main.rand.NextBool())
			{
				Main.dust[dustCreated].color = new Color(0.75f, 0f, 1f, 1f);
			}
			Main.dust[dustCreated].noGravity = true;
			Main.dust[dustCreated].velocity *= 0.8f;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if(chargeScale < 1)
			{
				damage = (int)(damage * 0.25f * chargeScale);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = Main.projectileTexture[projectile.type];
			int duration = (int)Math.Max(30, 60f * (1 - chargeScale/2));
			float angle = MathHelper.TwoPi * (animationFrame % duration) / duration;
			ChainDrawer goodDrawer = new ChainDrawer(GoodFrame);
			ChainDrawer evilDrawer = new ChainDrawer(EvilFrame);
			// extremely arbitrary series of hardcoded ints to make the 
			// beams line up with the tip of the sword
			Vector2 baseTangent = 12 * this.tangent;
			Vector2 tangent =  (6 * (0.25f + chargeScale) * (float)Math.Sin(angle)) * this.tangent;
			Vector2 center = projectile.Center + baseTangent;
			Vector2 end = endPoint + baseTangent;
			if(angle > MathHelper.Pi)
			{
				goodDrawer.DrawChain(spriteBatch, texture, center  + tangent, end + tangent, Color.White * chargeScale);
				evilDrawer.DrawChain(spriteBatch, texture, center - tangent, end - tangent, Color.White * chargeScale);
			} else
			{
				evilDrawer.DrawChain(spriteBatch, texture, center - tangent, end - tangent, Color.White * chargeScale);
				goodDrawer.DrawChain(spriteBatch, texture, center + tangent, end + tangent, Color.White * chargeScale);
			}
			return false;
		}
	}
}
