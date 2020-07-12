using Microsoft.Xna.Framework;
using System;

namespace DemoMod.Projectiles.Minions.MinonBaseClasses
{
    public abstract class WormFollowerMinion<T>: GroupAwareMinion<T> where T: MinionBuff
    {
        protected int NormalShift = 0;
        protected abstract Vector2 GetPositionAlongPath(ref Vector2 angle);
        public override Vector2? FindTarget()
        {
            return null;
        }

        public override Vector2 IdleBehavior()
        {
            // don't do anything
            return Vector2.Zero;
        }


        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            Vector2 angle = new Vector2();
            Vector2 trail = GetPositionAlongPath(ref angle);
            if(NormalShift != 0)
            {
                Vector2 normal = new Vector2(angle.Y, -angle.X);
                normal.Normalize();
                projectile.position = trail + Math.Sign(angle.X) * NormalShift*normal;
            } else
            {
                projectile.position = trail;
            }
            projectile.rotation = (float)Math.Atan2(angle.Y, angle.X) + (float)Math.PI;
            projectile.velocity = Vector2.Zero;
            // todo calc more efficiently
            if(angle.X > 0)
            {
                projectile.rotation -= (float)Math.PI;
                projectile.spriteDirection = -1;
            } else
            {
                projectile.spriteDirection = 1;
            }
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            // no-op, just follow the leader
        }
    }
}
