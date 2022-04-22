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
	// angle and position manipulated by parent projectile
	public abstract class MovableLaser : ModProjectile
	{
		protected int TimeToLive = 4 * 60;
		protected int ChargeTime = 2 * 60;
		protected int maxLength = 200 * 16;
		protected Vector2 endPoint = Vector2.Zero;
		internal Vector2 tangent;
		internal float chargeScale;
		internal int baseTangentSize = 12; // offset draw position of laser from center
		internal bool StopAfterFirstCollision;
		internal int collisionDuration;
		internal int collisionLength;

		protected float firingAngle => Projectile.ai[0];
		protected int animationFrame => TimeToLive - Projectile.timeLeft;

		protected virtual Color LightColor => Color.White;

		protected virtual Rectangle GetFrame(int idx, bool isLast) => default;

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// Todo: Not O(n) solution
			Vector2 direction = endPoint - Projectile.Center;
			float laserLength = direction.Length();
			direction.SafeNormalize();
			for(int i = 0; i < laserLength; i+= 8)
			{
				Vector2 checkPoint = Projectile.Center + direction * i;
				if(targetHitbox.Contains(checkPoint.ToPoint())) 
				{
					if(StopAfterFirstCollision)
					{
						collisionLength = i;
						collisionDuration = Projectile.localNPCHitCooldown + 2;
					}
					return true;
				}
			}
			return false;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = TimeToLive;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			Vector2 travelVector = firingAngle.ToRotationVector2();
			endPoint = Projectile.Center;
			chargeScale = Math.Min(1, MathHelper.Lerp(0, 1, animationFrame / (float)ChargeTime));
			int i;
			int step = 16;
			bool shouldDust = false;
			int checkLength = maxLength;
			if(StopAfterFirstCollision && collisionDuration > 0)
			{
				collisionDuration -= 1;
				maxLength = collisionLength;
				shouldDust = true;
			}
			for(i = step; i < maxLength; i += step)
			{
				Vector2 next = Projectile.Center + travelVector * i;
				if(!Collision.CanHitLine(endPoint, 1, 1, next, 1, 1))
				{
					shouldDust = true;
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
					Lighting.AddLight(next, LightColor.ToVector3() * 0.5f);
					endPoint = next;
					if(Main.rand.NextBool((int)(20 * (3 - 2 * chargeScale)))) {
						SpawnDust(endPoint, Vector2.Zero);
					}
				}
			}
			// LOTs of dust
			Vector2 direction = endPoint - Projectile.Center;
			direction.SafeNormalize();
			tangent = new Vector2(direction.Y, -direction.X);
			int dustFrequency = (int)(5 * (4 - 3 * chargeScale));
			if(shouldDust && animationFrame % dustFrequency != 0)
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

		protected virtual void SpawnDust(Vector2 position, Vector2 velocity)
		{
			// spawn dust where the laser hits a tile to hide the inexact drawing length
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if(chargeScale < 1)
			{
				damage = (int)(damage * chargeScale);
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			if(TimeToLive - Projectile.timeLeft < 2)
			{
				return false;
			}
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			ChainDrawer drawer = new ChainDrawer(GetFrame);
			// extremely arbitrary series of hardcoded ints to make the 
			// beams line up with the tip of the sword
			Vector2 baseTangent = baseTangentSize * this.tangent;
			Vector2 center = Projectile.Center + baseTangent;
			Vector2 end = endPoint + baseTangent;
			drawer.DrawChain(texture, center, end, Color.White * chargeScale);
			return false;
		}
	}

	class SoulboundArsenalLaser : MovableLaser
	{
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

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}

		protected override void SpawnDust(Vector2 position, Vector2 velocity)
		{
			int dustCreated = Dust.NewDust(position, 1, 1, 255, velocity.X, velocity.Y, 50, default, Scale: 1.4f);
			if (Main.rand.NextBool())
			{
				Main.dust[dustCreated].color = new Color(0.75f, 0f, 1f, 1f);
			}
			Main.dust[dustCreated].noGravity = true;
			Main.dust[dustCreated].velocity *= 0.8f;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int duration = (int)Math.Max(30, 60f * (1 - chargeScale/2));
			float angle = MathHelper.TwoPi * (animationFrame % duration) / duration;
			ChainDrawer goodDrawer = new ChainDrawer(GoodFrame);
			ChainDrawer evilDrawer = new ChainDrawer(EvilFrame);
			// extremely arbitrary series of hardcoded ints to make the 
			// beams line up with the tip of the sword
			Vector2 baseTangent = baseTangentSize * this.tangent;
			Vector2 tangent =  (6 * (0.25f + chargeScale) * (float)Math.Sin(angle)) * this.tangent;
			Vector2 center = Projectile.Center + baseTangent;
			Vector2 end = endPoint + baseTangent;
			if(angle > MathHelper.Pi)
			{
				goodDrawer.DrawChain(texture, center  + tangent, end + tangent, Color.White * chargeScale);
				evilDrawer.DrawChain(texture, center - tangent, end - tangent, Color.White * chargeScale);
			} else
			{
				evilDrawer.DrawChain(texture, center - tangent, end - tangent, Color.White * chargeScale);
				goodDrawer.DrawChain(texture, center + tangent, end + tangent, Color.White * chargeScale);
			}
			return false;
		}
	}
}
