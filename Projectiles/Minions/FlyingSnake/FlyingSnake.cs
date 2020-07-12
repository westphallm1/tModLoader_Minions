using DemoMod.Projectiles.Minions.MinonBaseClasses;
using log4net.Repository.Hierarchy;
using log4net.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.FlyingSnake
{
    public class FlyingSnakeMinionBuff: MinionBuff
    {
        public FlyingSnakeMinionBuff() : base(ProjectileType<FlyingSnakeMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Flying Snake");
			Description.SetDefault("A balloon buddy will fight for you!");
        }
    }

    public class FlyingSnakeMinionItem: EmpoweredMinionItem<FlyingSnakeMinionBuff, FlyingSnakeMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Snake");
			Tooltip.SetDefault("Summons a balloon buddy to fight for you!");
            
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Blue;
		}
    }

    public class FlyingSnakeTailMinion : WormFollowerMinion<FlyingSnakeMinionBuff>
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.projFrames[projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.friendly = false;
            projectile.minionSlots = 0;
            projectile.width = 24;
            projectile.height = 22;
        }

        protected override Vector2 GetPositionAlongPath(ref Vector2 angle)
        {
            var friends = GetMinionsOfType(ProjectileType<FlyingSnakeBodyMinion>());
            var order = friends.Count +2;
            return FlyingSnakeMinion.PositionLog.PositionAlongPath(12*order + 18, ref angle);
        }
    }

    public class FlyingSnakeBodyMinion : WormFollowerMinion<FlyingSnakeMinionBuff>
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.friendly = false;
            projectile.minionSlots = 1;
            projectile.width = 16;
            projectile.height = 18;
        }

        protected override Vector2 GetPositionAlongPath(ref Vector2 angle)
        {
            var friends = GetActiveMinions();
            var order = friends.IndexOf(projectile) + 1;
            return FlyingSnakeMinion.PositionLog.PositionAlongPath(12*order + 24, ref angle);
        }
    }

    public class FlyingSnakeHeadMinion : WormFollowerMinion<FlyingSnakeMinionBuff>
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.projFrames[projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.friendly = false;
            projectile.minionSlots = 1;
            projectile.width = 20;
            projectile.height = 22;
        }

        protected override Vector2 GetPositionAlongPath(ref Vector2 angle)
        {
            return FlyingSnakeMinion.PositionLog.PositionAlongPath(4, ref angle);
        }
        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            projectile.frame = vectorToTarget == null ? 0 : 1;
        }
    }
    public class FlyingSnakeWingsMinion : WormFollowerMinion<FlyingSnakeMinionBuff>
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.projFrames[projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.friendly = false;
            projectile.minionSlots = 0;
            projectile.width = 20;
            projectile.height = 28;
            NormalShift = 4;
        }

        protected override Vector2 GetPositionAlongPath(ref Vector2 angle)
        {
            return FlyingSnakeMinion.PositionLog.PositionAlongPath(22, ref angle);
        }
    }

    public class FlyingSnakeMinion : EmpoweredMinion<FlyingSnakeMinionBuff>
    {
        private static float[] backingArray;
        public static CircularLengthQueue PositionLog = null;
        public int framesSinceLastHit = 0;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Snake");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

        public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 1;
			projectile.height = 1;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<FlyingSnakeMinion>();
            projectile.ai[0] = 0;
            projectile.ai[1] = 0;
            projectile.minionSlots = -1;
            backingArray = new float[255];
            CircularVectorQueue.Initialize(backingArray);
            PositionLog = new CircularLengthQueue(backingArray, queueSize: 32)
            {
                mod = mod
            };
        }

        protected int GetSegmentCount()
        {
            return GetMinionsOfType(ProjectileType<FlyingSnakeBodyMinion>()).Count;
        }

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            if(player.ownedProjectileCounts[ProjectileType<FlyingSnakeBodyMinion>()] == 0)
            {
                OnEmpower();
            }
            if(player.ownedProjectileCounts[ProjectileType<FlyingSnakeTailMinion>()] ==0)
            {
                Projectile.NewProjectile(projectile.position, Vector2.Zero, ProjectileType<FlyingSnakeTailMinion>(), 0, 0, Main.myPlayer);
            }
            if(player.ownedProjectileCounts[ProjectileType<FlyingSnakeWingsMinion>()] ==0)
            {
                Projectile.NewProjectile(projectile.position, Vector2.Zero, ProjectileType<FlyingSnakeWingsMinion>(), 0, 0, Main.myPlayer);
            }
            if(player.ownedProjectileCounts[ProjectileType<FlyingSnakeHeadMinion>()] ==0)
            {
                Projectile.NewProjectile(projectile.position, Vector2.Zero, ProjectileType<FlyingSnakeHeadMinion>(), 0, 0, Main.myPlayer);
            }
            projectile.ai[1] = (projectile.ai[1] + 1) % 240;
            Vector2 idlePosition = player.Top;
            idlePosition.X += 48 * (float)Math.Cos(Math.PI * projectile.ai[1] / 60);
            idlePosition.Y += -48  + 8 * (float)Math.Sin(Math.PI * projectile.ai[1] / 60);
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }
        protected override void OnEmpower()
        {
            Projectile.NewProjectile(projectile.position, Vector2.Zero, ProjectileType<FlyingSnakeBodyMinion>(), 0, 0, Main.myPlayer);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            framesSinceLastHit = 0;
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            base.TargetedMovement(vectorToTargetPosition);
            float inertia = ComputeInertia();
            float speed = ComputeTargetedSpeed();
            vectorToTargetPosition.Normalize();
            vectorToTargetPosition *= speed;
            if(framesSinceLastHit ++ > 4)
            {
                projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
            } else
            {
                projectile.velocity.Normalize();
                projectile.velocity *= speed; // kick it away from enemies that it's just hit
            }
        }

        protected override int ComputeDamage()
        {
            return 40 + 20 * GetSegmentCount();
        }

        protected override float ComputeSearchDistance()
        {
            return 800 + 25 * GetSegmentCount();
        }

        protected override float ComputeInertia()
        {
            return 22 - GetSegmentCount();
        }

        protected override float ComputeTargetedSpeed()
        {
            return 3 + 3 * GetSegmentCount();
        }

        protected override float ComputeIdleSpeed()
        {
            return ComputeTargetedSpeed() + 3;
        }

        protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
        {
            if(vectorToTarget == null)
            {
                minFrame = 0;
                maxFrame = 4;
            } else
            {
                minFrame = 5;
                maxFrame = 8;
            }
        }

        public override void AfterMoving()
        {
            base.AfterMoving();
            PositionLog.AddPosition(projectile.position);
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            base.Animate(minFrame, maxFrame);
            //projectile.spriteDirection = projectile.direction;
            Lighting.AddLight(projectile.position, Color.Green.ToVector3() * 0.5f);
        }
    }
}
