using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.EclipseHerald
{
	/// <summary>
	/// Uses ai[0] to track the frame/size, and ai[1] to track the target npc
	/// </summary>
	class EclipseSphere : ModProjectile
	{
		private bool hitTarget;

		private NPC targetNPC => Main.npc[(int)Projectile.ai[1]];
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 6;
			// ProjectileID.Sets.CountsAsHoming[Projectile.type] = true;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}
		public override void SetDefaults()
		{
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 300;
			hitTarget = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}
		public override void AI()
		{
			base.AI();
			if (hitTarget)
			{
				Projectile.frame = Math.Min(5, (int)Projectile.ai[0]);
			}
			else
			{
				Projectile.frame = 0;
			}
			Projectile.rotation += (float)(Math.PI) / 90;
			if (!hitTarget && targetNPC.active)
			{
				Vector2 vectorToTarget = targetNPC.Center - Projectile.Center;
				if (vectorToTarget.Length() < 32)
				{
					OnHitTarget();
				}
				vectorToTarget.SafeNormalize();
				Projectile.velocity = vectorToTarget * (6 + Math.Min(5, Projectile.ai[0]));
			}
			Lighting.AddLight(Projectile.position, Color.White.ToVector3() * 0.5f);
			AddDust();
			//DrawInNPCs();
		}

		private void AddDust()
		{
			if (!hitTarget)
			{
				Vector2 velocity = -Projectile.velocity;
				int dustSize = (int)(2 + 2 * Projectile.ai[0]);
				Dust.NewDust(Projectile.Center, dustSize, dustSize, DustID.GoldFlame, velocity.X, velocity.Y);
			}
		}

		private void OnHitTarget()
		{
			hitTarget = true;
			Projectile.timeLeft = Math.Min(Projectile.timeLeft, 60);
			Projectile.position += Projectile.velocity;
			Projectile.velocity.SafeNormalize();
			Projectile.velocity *= 2; // slowly drift from place of impact
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			OnHitTarget();
			base.OnHitNPC(target, damage, knockback, crit);
		}
	}

}
