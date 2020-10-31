using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.BeeQueen
{
    public class HoneySlime: BumblingTransientMinion
    {
        protected override int timeToLive => 60 * 3; // 3 seconds;
        protected override float inertia => didLand ? 1 : 12;

        protected override bool onAttackCooldown => false;
        protected override float idleSpeed => maxSpeed * 0.75f;
        protected override float searchDistance => 350f;
        protected override float noLOSSearchDistance => 350f;
        protected override float distanceToBumbleBack => 8000f; // don't bumble back

        int defaultMaxSpeed = 4;
        int defaultJumpVelocity = 6;
        int minFrame;
        bool didLand = false;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ProjectileID.Sets.MinionShot[projectile.type] = true;
            Main.projFrames[projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.width = 16;
            projectile.height = 16;
            projectile.penetrate = -1;
            attackThroughWalls = false;
            projectile.tileCollide = true;
            minFrame = 2 * Main.rand.Next(3);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            fallThrough = false;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if(projectile.velocity.Y == 0 && oldVelocity.Y >= 0)
            {
                didLand = true;
            }
            if(projectile.velocity.Y != 0 && projectile.velocity.X == 0 && oldVelocity.X != 0)
            {
                initialVelocity = -initialVelocity;
            }
            return false;
        }

        public override Vector2 IdleBehavior()
        {
            if(maxSpeed == default)
            {
                maxSpeed = defaultMaxSpeed;
                initialVelocity = new Vector2(Math.Sign(projectile.velocity.X) * maxSpeed, 0);
            }
            projectile.velocity.Y += 0.5f;
            return base.IdleBehavior();
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            if(!didLand)
            {
                return;
            }
            if(vectorToTargetPosition.Y > 32 && DropThroughPlatform())
            {
                didLand = false; // now falling through the air again
                return;
            }
            if(vectorToTargetPosition.Y < -4 * defaultJumpVelocity)
            {
                projectile.velocity.Y = Math.Max(-12, vectorToTargetPosition.Y/4);
            } else
            {
                projectile.velocity.Y = -defaultJumpVelocity;
            }
            base.TargetedMovement(vectorToTargetPosition);
            didLand = false;
        }

        protected override void Move(Vector2 vector2Target, bool isIdle = false)
        {
            float oldY = projectile.velocity.Y;
            float oldMaxSpeed = maxSpeed;
            maxSpeed = Math.Min(maxSpeed, Math.Abs(vector2Target.X / 6));
            maxSpeed = Math.Max(maxSpeed, 2);
            base.Move(vector2Target, isIdle);
            projectile.velocity.Y = oldY; // y can only be modified by jump & gravity
            maxSpeed = oldMaxSpeed; // may accidentally jump over enemies if we let it move too fast
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            int direction = Math.Sign(projectile.velocity.X);
            direction = direction == 0 ? 1 : direction;
            initialVelocity = new Vector2( direction * maxSpeed, 0);
            if(Main.rand.Next(10) == 0)
            {
                target.AddBuff(BuffID.Slow, 120);
            }
            base.OnHitNPC(target, damage, knockback, crit);
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            if(didLand)
            {
                projectile.velocity.Y = -defaultJumpVelocity;
                didLand = false;
            }
            base.IdleMovement(vectorToIdlePosition);
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            minFrame = this.minFrame;
            maxFrame = minFrame + 2;
            base.Animate(minFrame, maxFrame);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 128);
            float r = projectile.rotation;
            Vector2 pos = projectile.Center;
            SpriteEffects effects = projectile.velocity.X < 0 ? 0 : SpriteEffects.FlipHorizontally;
            Texture2D texture = GetTexture(Texture);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                bounds, translucentColor, r,
                origin, 0.75f, effects, 0);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            for(int i = 0; i < 3; i++)
            {
                Dust.NewDust(projectile.Center, 16, 16, 153);
            }
        }
    }
}
