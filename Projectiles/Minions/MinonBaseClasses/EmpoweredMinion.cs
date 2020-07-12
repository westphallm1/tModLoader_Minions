using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.MinonBaseClasses
{
    public abstract class EmpoweredMinionItem<TBuff, TMinion> : MinionItem<TBuff, TMinion> where TBuff: ModBuff where TMinion: EmpoweredMinion<TBuff>
    {
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if(player.ownedProjectileCounts[item.shoot] == 0)
            {
                player.AddBuff(BuffType<TBuff>(), 2);
                Projectile.NewProjectile(position - new Vector2(5, 0), new Vector2(speedX, speedY), item.shoot, damage, knockBack, Main.myPlayer);
            } else
            {
                for (int i = 0; i < Main.maxProjectiles; i++) {
                    Projectile other = Main.projectile[i];
                    if (other.active && other.owner == Main.myPlayer && other.type == item.shoot && other.minionSlots < player.maxMinions)
                    {
                        other.ai[0] = 1;
                        break;
                    }
                }
            }
            return false;
        }

    }

    public abstract class EmpoweredMinion<T> : SimpleMinion<T> where T: ModBuff
    {
        protected abstract int ComputeDamage();
        protected abstract float ComputeSearchDistance();
        protected abstract float ComputeInertia();
        protected abstract float ComputeTargetedSpeed();
        protected abstract float ComputeIdleSpeed();

        protected abstract void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame);

        protected virtual void OnEmpower()
        {
            if(projectile.minionSlots < player.maxMinions)
            {
                projectile.minionSlots += 1;
            }
        }

        public override Vector2 IdleBehavior()
        {
            if(projectile.ai[0] == 1 )
            {
                OnEmpower();
                projectile.ai[0] = 0;
            }
            projectile.damage = ComputeDamage();
            return Vector2.Zero;
        }

        public override Vector2? FindTarget()
        {
            float searchDistance = ComputeSearchDistance();
            if (PlayerTargetPosition(searchDistance, player.Center) is Vector2 target)
            {
                return target - projectile.Center;
            }
            else if (ClosestEnemyInRange(searchDistance, player.Center) is Vector2 target2)
            {
                return target2 - projectile.Center;
            }
            else
            {
                return null;
            }
        }


        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            float inertia = ComputeInertia();
            float speed = ComputeTargetedSpeed();
            vectorToTargetPosition.Normalize();
            vectorToTargetPosition *= speed;
            projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
        }


        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            // alway clamp to the idle position
            float inertia = ComputeInertia();
            float maxSpeed = ComputeIdleSpeed();
            Vector2 speedChange = vectorToIdlePosition - projectile.velocity;
            if(speedChange.Length() > maxSpeed)
            {
                speedChange.Normalize();
                speedChange *= maxSpeed;
            }
            projectile.velocity = (projectile.velocity * (inertia - 1) + speedChange) / inertia;
        }


        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            int max = 0;
            SetMinAndMaxFrames(ref minFrame, ref max);
            maxFrame = max;
			int frameSpeed = 15;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed) {
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= (maxFrame ?? Main.projFrames[projectile.type]) ||
                    projectile.frame < minFrame) {
					projectile.frame = minFrame;
				}
			}
        }
    }
}
