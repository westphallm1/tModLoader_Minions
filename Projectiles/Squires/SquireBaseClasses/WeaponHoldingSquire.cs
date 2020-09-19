using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AmuletOfManyMinions.Projectiles.Minions;
using System;
using Microsoft.Xna.Framework.Graphics;


namespace AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses
{
    public abstract class WeaponHoldingSquire<T> : SquireMinion<T> where T : ModBuff
    {
        protected bool usingWeapon = false;
        protected abstract int AttackFrames {
            get;
        }
        protected int attackFrame = 0;
        protected float weaponAngle = 0;
        public WeaponHoldingSquire(int itemID) : base(itemID) { }

        public override void SetDefaults()
        {
            base.SetDefaults();
			projectile.tileCollide = false;
            projectile.friendly = true;
            projectile.usesLocalNPCImmunity = true;
            useBeacon = false;
        }

        protected virtual int WeaponHitboxEnd()
        {
            return 40;
        }

        protected virtual int WeaponHitboxStart()
        {
            return 16;
        }

        public override Vector2 IdleBehavior()
        {
            if(GetSpriteDirection() is int direction)
            {
                projectile.spriteDirection = direction;
            }
            return base.IdleBehavior();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // use a computed weapon hitbox instead of the projectile's natural hitbox
            if(!usingWeapon)
            {
                return false;
            }
            if(!Collision.CanHitLine(projectile.Center, 1, 1, targetHitbox.Center.ToVector2(), 1, 1)) {
                return false;
            }
            Vector2 unitAngle = UnitVectorFromWeaponAngle();
            for(int i = WeaponHitboxStart(); i < WeaponHitboxEnd(); i+= 8)
            {
                Vector2 tipCenter = projectile.Center + i * unitAngle;
                Rectangle tipHitbox = new Rectangle((int)tipCenter.X - 8, (int)tipCenter.Y - 8, 16, 16);
                if(tipHitbox.Intersects(targetHitbox))
                {
                    return true;
                }
            }
            return false;
        }
        protected abstract float GetWeaponAngle();

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Rectangle bounds = new Rectangle(0, 4 * 42, texture.Width, projectile.height);
            Vector2 origin = new Vector2(projectile.width / 2f, bounds.Height / 2f);
            Vector2 pos = projectile.Center;
            float r = projectile.rotation;
            SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
            if(usingWeapon)
            {
                if(weaponAngle > (float)Math.PI / 8)
                {
                    bounds = new Rectangle(0, 5 * 42, texture.Width, projectile.height);
                } else if(weaponAngle > -Math.PI/8)
                {
                    bounds = new Rectangle(0, 6 * 42, texture.Width, projectile.height);
                } else
                {
                    bounds = new Rectangle(0, 7 * 42, texture.Width, projectile.height);
                }
                DrawWeapon(spriteBatch, lightColor);
            }
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                bounds, lightColor, r,
                origin, 1, effects, 0);
        }

        protected virtual Vector2 WeaponCenter()
        {
            return projectile.Center;
        }

        protected Vector2 UnitVectorFromWeaponAngle()
        {
            if(projectile.spriteDirection == 1)
            {
                return new Vector2((float)Math.Cos(-weaponAngle), (float)Math.Sin(-weaponAngle));
            } else
            {
                var reflectedAngle = Math.PI - weaponAngle;
                return new Vector2((float)Math.Cos(-reflectedAngle), (float)Math.Sin(-reflectedAngle));
            }
        }

        // valid for diagonal sprites, eg. swords
        protected virtual float SpriteRotationFromWeaponAngle()
        {
            if(projectile.spriteDirection == 1)
            {
                return (float)Math.PI/4-weaponAngle;
            } else
            {
                return -((float)Math.PI/4-weaponAngle);
            }
        }
        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            usingWeapon = true;
            attackFrame = (attackFrame + 1) % AttackFrames;
            weaponAngle = GetWeaponAngle();
            base.TargetedMovement(vectorToTargetPosition);
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            usingWeapon = false;
            weaponAngle = GetWeaponAngle();
            base.IdleMovement(vectorToIdlePosition);
        }

        private void DrawWeapon(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = GetWeaponTexture();
            Rectangle bounds = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = new Vector2(bounds.Width/2, bounds.Height/2); // origin should hopefully be more or less center of squire
            Vector2 center = UnitVectorFromWeaponAngle() * WeaponOffset();
            float r = SpriteRotationFromWeaponAngle();
            Vector2 pos = WeaponCenter() + center;
            SpriteEffects effects =  projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                bounds, lightColor, r,
                origin, 1, effects, 0);
        }

        protected abstract float WeaponOffset();

        protected virtual int? GetSpriteDirection()
        {
            if(vectorToTarget is Vector2 target)
            {
                return Math.Sign((Main.MouseWorld - player.position).X);
            } else if(projectile.velocity.X < -1)
            {
                return -1;
            } else if (projectile.velocity.X > 1)
            {
                return 1;
            }
            return null;
        }

        protected abstract Texture2D GetWeaponTexture();
        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            projectile.rotation = projectile.velocity.X * 0.05f;
            base.Animate(minFrame, maxFrame);
        }

    }
}
