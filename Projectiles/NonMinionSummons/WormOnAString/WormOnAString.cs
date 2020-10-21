using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AmuletOfManyMinions.Projectiles.Minions;
using static Terraria.ModLoader.ModContent;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace AmuletOfManyMinions.Projectiles.NonMinionSummons.WormOnAString
{
    public class WormProjectile : TransientMinion
    {
        public const int TIME_TO_LIVE = 60 * 10; // 10 seconds;

        bool hasLanded;
        int framesToTurn;
        readonly Random random = new Random();

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.projFrames[projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            frameSpeed = 15;
            projectile.width = 14;
            projectile.height = 8;
            projectile.tileCollide = true;
            hasLanded = false;
            projectile.timeLeft = TIME_TO_LIVE;
            framesToTurn = 400 + 30 * random.Next(-3, 3);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            fallThrough = false;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if(oldVelocity.Y != 0 && projectile.velocity.Y == 0)
            {
                hasLanded = true;
            }
            if(oldVelocity.X != 0 && projectile.velocity.X == 0)
            {
                projectile.velocity.X = -Math.Sign(oldVelocity.X);
            }
            return false;
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            if(projectile.timeLeft < TIME_TO_LIVE - 30)
            {
                projectile.velocity.Y += .5f;
            }
            if(hasLanded)
            {
                projectile.velocity.X = Math.Sign(projectile.velocity.X);
            }
            if(hasLanded && projectile.timeLeft % framesToTurn == 0) // turn around every so often
            {
                projectile.velocity.X *= -3;
            }
        }

        public override void Kill(int timeLeft)
        {
            for(int i = 0; i < 10; i ++)
            {
                Dust.NewDust(projectile.Center - Vector2.One * 16, 32, 32, DustID.Dirt);
            }
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            if(!hasLanded)
            {
                maxFrame = 1;
                projectile.rotation += 0.2f;
            } else
            {
                maxFrame = 2;
                projectile.rotation = 0;
                projectile.spriteDirection = projectile.direction;
            }
            base.Animate(minFrame, maxFrame);
        }
    }
}
