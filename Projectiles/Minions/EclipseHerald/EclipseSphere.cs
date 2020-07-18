using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System.Linq;

namespace DemoMod.Projectiles.Minions.EclipseHerald
{
    class EclipseSphere : ModProjectile {
        private int hitTarget;

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
			projectile.timeLeft = 1800;
            hitTarget = 0;
            ProjectileID.Sets.Homing[projectile.type] = true;
            ProjectileID.Sets.MinionShot[projectile.type] = true;
		}
        public override void AI()
        {
			base.AI();
            projectile.frame = Math.Min(5, (int)projectile.ai[0]);
			projectile.rotation += (float)(Math.PI) / 90;
            if(hitTarget == 0 && targetNPC.active)
            {
                Vector2 vectorToTarget = targetNPC.Center - projectile.Center;
                if(vectorToTarget.Length() < 4)
                {
                    OnHitTarget();
                }
                vectorToTarget.Normalize();
                projectile.velocity = vectorToTarget * (8+ projectile.ai[0]);
            }
            if(targetNPC.active && targetNPC.boss && hitTarget > 0)
            {
                hitTarget--;
            }
            if(!targetNPC.active)
            {
                hitTarget = 1;
            }
            Lighting.AddLight(projectile.position, Color.White.ToVector3() * 0.5f);
            AddDust();
            //DrawInNPCs();
        }

        private void AddDust()
        {
            // dust should come from outside the projectile and go towards the center
            Vector2 velocity = -projectile.velocity;
            for(int i = 0; i < 5; i++)
            {
                Dust.NewDust(projectile.position, projectile.width/2, projectile.height/2, DustID.GoldFlame, velocity.X, velocity.Y);
            }
        }

        private void OnHitTarget()
        {
            hitTarget = 20;
            projectile.timeLeft = Math.Min(projectile.timeLeft, 60);
            projectile.tileCollide = true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if(target == targetNPC)
            {
                OnHitTarget();
            }
            base.OnHitNPC(target, damage, knockback, crit);
        }
    }

}
