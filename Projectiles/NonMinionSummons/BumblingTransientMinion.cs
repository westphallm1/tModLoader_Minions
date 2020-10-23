
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using AmuletOfManyMinions.Projectiles.Squires.StardustSquire;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.NonMinionSummons
{
    /**
     * Transient minion that stays in the approximate vicinity of the player while there is no enemy around
     */
    public abstract class BumblingTransientMinion : TransientMinion
    {
        protected float maxSpeed = default;
        private Vector2 initialVelocity = Vector2.Zero;
        private int lastHitFrame;
        protected virtual float inertia => default;
        protected virtual float idleSpeed => default;

        protected virtual int timeToLive => default;

        protected virtual float distanceToBumbleBack => default;

        protected virtual float searchDistance => default;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.projFrames[projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.localNPCHitCooldown = 30;
            projectile.timeLeft = timeToLive;
            lastHitFrame = timeToLive + projectile.localNPCHitCooldown;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color translucentColor = transformLight(lightColor);
            Texture2D texture = Main.projectileTexture[projectile.type];


            int height = texture.Height / Main.projFrames[projectile.type];
            Rectangle bounds = new Rectangle(0, projectile.frame * height, texture.Width, height);
            Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);

            SpriteEffects effects = GetSpriteEffects();
            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
                bounds, translucentColor, projectile.rotation,
                origin, 1, effects, 0);
            return false;
        }

        protected virtual SpriteEffects GetSpriteEffects()
        {
            if(projectile.velocity.X > 0)
            {
                return SpriteEffects.FlipHorizontally;
            }
            return 0;
        }

        protected virtual void Move(Vector2 vector2Target, bool isIdle = false)
        {
            vector2Target.SafeNormalize();
            vector2Target *= isIdle ? idleSpeed : maxSpeed;
            projectile.velocity = (projectile.velocity * (inertia - 1) + vector2Target) / inertia;
            base.TargetedMovement(vector2Target);
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            Move(vectorToTargetPosition);
        }
        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            Move(vectorToIdlePosition, true);
        }

        public override Vector2 IdleBehavior()
        {
            if(maxSpeed == default)
            {
                maxSpeed = projectile.velocity.Length();
                initialVelocity = projectile.velocity;
            }
            Vector2 vector2Player = player.Center - projectile.Center;
            if(lastHitFrame - projectile.timeLeft > projectile.localNPCHitCooldown && 
                vector2Player.Length() > distanceToBumbleBack)
            {
                vector2Player.SafeNormalize();
                initialVelocity = vector2Player * maxSpeed;
            }
            return initialVelocity;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if(oldVelocity.Y != 0 && projectile.velocity.Y == 0)
            {
                projectile.velocity.Y = -oldVelocity.Y;
            } if(oldVelocity.X != 0 && projectile.velocity.X == 0)
            {
                projectile.velocity.X = -oldVelocity.X;
            }
            initialVelocity = projectile.velocity;
            return false;
        }

        public override Vector2? FindTarget()
        {
            if(lastHitFrame - projectile.timeLeft < projectile.localNPCHitCooldown)
            {
                return null;
            }
            if(ClosestEnemyInRange(searchDistance, projectile.position, maxRangeFromPlayer: false) is Vector2 closest)
            {
                return closest - projectile.position;
            }
            return null;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            lastHitFrame = projectile.timeLeft;
        }
        protected virtual Color transformLight(Color color)
        {
            return color;
        }
    }
}
