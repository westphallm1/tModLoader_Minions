using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions.AI;
using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Squires.EmpressSquire
{
	class AIColorTransfer
	{
		public static Color FromFloat(float floatVal)
		{
			int intVal = (int)floatVal;
			byte r = (byte)(intVal >> 16);
			byte g = (byte)((intVal & 0xFF00) >> 8);
			byte b = (byte)(intVal & 0x00FF);
			return new(r, g, b);
		}

		public static float FromColor(Color color)
		{
			return (color.R << 16) + (color.G << 8) + color.B;
		}
	}
	abstract class BaseEmpressStarlightProjectile : ModProjectile
	{
		MotionBlurDrawer blurDrawer;
		internal Color projColor;
		internal float baseScale;
		internal Texture2D solidTexture;

		public override void SetDefaults()
		{
			base.SetDefaults();
			blurDrawer = new MotionBlurDrawer(10);
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		internal void SpawnDust()
		{
				int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 279, 0f, 0f, 100, default, 1);
				Main.dust[dustId].color = projColor;
				Main.dust[dustId].velocity *= 0.3f;
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].noLight = true;
				Main.dust[dustId].fadeIn = 1f;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			blurDrawer.DrawBlur(texture, projColor, texture.Bounds, r, baseScale, 0.9f);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				texture.Bounds, projColor, r,
				texture.Bounds.Center(), baseScale, 0, 0);
			return false;
		}
		public override void AI()
		{
			base.AI();
			if(projColor == default)
			{
				projColor = AIColorTransfer.FromFloat(Projectile.ai[0]);
			}
			blurDrawer.Update(Projectile.Center);
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 10; i++)
			{
				SpawnDust();
			}
		}
	}

	/// <summary>
	/// Uses ai[0] for rotation index, rest of AI controlled by parent
	/// </summary>
	class EmpressSpecialOrbitProjectile : BaseEmpressStarlightProjectile
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 900;
			baseScale = 1f;
		}

		public override void AI()
		{
			base.AI();
			projColor = EmpressSquireMinion.SpecialColors[(int)Projectile.ai[0]];
		}
	}

	/// <summary>
	/// Uses ai[0] for rotation index, ai[1] for draw scale
	/// </summary>
	class EmpressStarlightProjectile : BaseEmpressStarlightProjectile
	{
		NPC target;
		float baseVelocity;
		int TimeToLive = 60;

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.timeLeft = TimeToLive;
		}

		public override void AI()
		{
			base.AI();
			baseScale = Projectile.ai[1];
			if(baseVelocity == default)
			{
				baseVelocity = Projectile.velocity.Length();
			}

			if((target == null || !target.active) && MinionBehavior.GetClosestEnemyToPosition(Projectile.Center, 400f) is NPC npc)
			{
				target = npc;
			}

			if(target?.active ?? false)
			{
				Vector2 vectorToTarget = target.Center - Projectile.Center;
				float distanceToTarget = vectorToTarget.Length();
				if(distanceToTarget > baseVelocity)
				{
					vectorToTarget.SafeNormalize();
					vectorToTarget *= baseVelocity;
				}
				int inertia = Projectile.timeLeft > TimeToLive - 15 ? 12 : 4;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTarget) / inertia;
			} else if (Projectile.timeLeft < TimeToLive - 15)
			{
				Projectile.velocity *= 0.9f;
			}
			Projectile.rotation += Projectile.velocity.X * 0.01f;
			if(Main.rand.NextBool(6) || (Projectile.velocity.LengthSquared() > 2 && Main.rand.NextBool()))
			{
				SpawnDust();
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return Projectile.BounceOnCollision(oldVelocity);
		}

	}
}
