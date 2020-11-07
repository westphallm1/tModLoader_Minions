using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.EclipseHerald
{
	class EclipseSphere : ModProjectile
	{
		private bool hitTarget;

		private NPC targetNPC => Main.npc[(int)projectile.ai[1]];
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 6;
		}
		public override void SetDefaults()
		{
			projectile.width = 64;
			projectile.height = 64;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.timeLeft = 300;
			hitTarget = false;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}
		public override void AI()
		{
			base.AI();
			if (hitTarget)
			{
				projectile.frame = Math.Min(5, (int)projectile.ai[0]);
			}
			else
			{
				projectile.frame = 0;
			}
			projectile.rotation += (float)(Math.PI) / 90;
			if (!hitTarget && targetNPC.active)
			{
				Vector2 vectorToTarget = targetNPC.Center - projectile.Center;
				if (vectorToTarget.Length() < 4)
				{
					OnHitTarget();
				}
				vectorToTarget.SafeNormalize();
				projectile.velocity = vectorToTarget * (6 + Math.Min(5, projectile.ai[0]));
			}
			Lighting.AddLight(projectile.position, Color.White.ToVector3() * 0.5f);
			AddDust();
			//DrawInNPCs();
		}

		private void AddDust()
		{
			if (!hitTarget)
			{
				Vector2 velocity = -projectile.velocity;
				int dustSize = (int)(2 + 2 * projectile.ai[0]);
				Dust.NewDust(projectile.Center, dustSize, dustSize, DustID.GoldFlame, velocity.X, velocity.Y);
			}
		}

		private void OnHitTarget()
		{
			hitTarget = true;
			projectile.timeLeft = Math.Min(projectile.timeLeft, 60);
			projectile.position += projectile.velocity;
			projectile.velocity.SafeNormalize();
			projectile.velocity *= 2; // slowly drift from place of impact
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			OnHitTarget();
			base.OnHitNPC(target, damage, knockback, crit);
		}
	}

}
