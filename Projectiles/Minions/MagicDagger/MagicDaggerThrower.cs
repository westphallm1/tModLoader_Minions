using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.MagicDagger
{
    public class MagicDaggerThrower : SimpleMinion<MagicDaggerMinionBuff>
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            projectile.type = ProjectileType<MagicDaggerThrower>();
            DisplayName.SetDefault("Magic Dagger Thrower");
            // Sets the amount of frames this minion has on its spritesheet
            Main.projFrames[projectile.type] = 1;
        }

        public sealed override void SetDefaults()
        {
            base.SetDefaults();
            projectile.width = 14;
            projectile.height = 14;
            projectile.tileCollide = false;
            projectile.friendly = false;
            projectile.ai[0] = 0;
            projectile.minionSlots = 0f;
        }

        public override Vector2? FindTarget()
        {
            if (PlayerTargetPosition(600f, player.Center) is Vector2 target)
            {
                return target - projectile.Center;
            }
            else if (ClosestEnemyInRange(600f, player.Center) is Vector2 target2)
            {
                return target2 - projectile.Center;
            }
            else
            {
                return null;
            }
        }

        public override Vector2 IdleBehavior()
        {
            Vector2 idlePosition = player.Top;
            idlePosition.X += 30 * -player.direction;
            idlePosition.Y += -5;
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            int inertia = 10;
            int maxSpeed = 16;
            projectile.tileCollide = false;
            Vector2 speedChange = vectorToIdlePosition - projectile.velocity;
            if (speedChange.Length() > maxSpeed)
            {
                speedChange.Normalize();
                speedChange *= maxSpeed;
            }
            projectile.spriteDirection = player.direction;
            projectile.velocity = (projectile.velocity * (inertia - 1) + speedChange) / inertia;
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            int inertia = 15;
            int maxSpeed = 6;
            projectile.tileCollide = true;
            // move towards the enemy, but don't get too far from the player
            projectile.spriteDirection = vectorToTargetPosition.X > 0 ? 1 : -1;
            Vector2 vectorFromPlayer = player.Center - projectile.Center;
            if (vectorFromPlayer.Length() > 150f)
            {
                vectorToTargetPosition = vectorFromPlayer;
            } else if (vectorToTargetPosition.Length() < 200f){
                vectorToTargetPosition *= -1;
            }
            vectorToTargetPosition.Normalize();
            vectorToTargetPosition *= maxSpeed;
            projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            base.Animate(minFrame, maxFrame);
            projectile.rotation -= (2 * (float)Math.PI) / 120;
        }
    }
}
