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

namespace DemoMod.Projectiles.Minions.BalloonBuddy
{
    public class BalloonBuddyMinionBuff: MinionBuff
    {
        public BalloonBuddyMinionBuff() : base(ProjectileType<BalloonBuddyMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Balloon Buddy");
			Description.SetDefault("A balloon buddy will fight for you!");
        }
    }

    public class BalloonBuddyMinionItem: EmpoweredMinionItem<BalloonBuddyMinionBuff, BalloonBuddyMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Balloon Buddy");
			Tooltip.SetDefault("Summons a balloon buddy to fight for you!");
            
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
            item.damage = 10;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Blue;
		}
    }

    public class BalloonBuddyTailMinion : GroupAwareMinion<BalloonBuddyMinionBuff>
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.friendly = false;
            projectile.minionSlots = 0;
            projectile.width = 24;
            projectile.height = 22;
        }
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
            // no-op, just follow the leader
            var friends = GetMinionsOfType(ProjectileType<BalloonBuddyBodyMinion>());
            var order = friends.Count;
            var attachedTo = friends[friends.Count - 1];
            Vector2 angle = new Vector2();
            Vector2 trail = BalloonBuddyMinion.PositionLog.PositionAlongPath(12*order, ref angle);
            projectile.position = trail;
            projectile.spriteDirection = angle.X > 0 ? -1 : 1; 

            projectile.velocity = Vector2.Zero;
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            // no-op, just follow the leader
        }
    }

    public class BalloonBuddyBodyMinion : GroupAwareMinion<BalloonBuddyMinionBuff>
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.friendly = false;
            projectile.minionSlots = 1;
            projectile.width = 14;
            projectile.height = 12;
        }
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
            // no-op, just follow the leader
            var friends = GetActiveMinions();
            var order = friends.IndexOf(projectile);
            Vector2 angle = new Vector2();
            Vector2 trail = BalloonBuddyMinion.PositionLog.PositionAlongPath(12*order, ref angle);
            projectile.position = trail;
            projectile.rotation = (float)Math.Atan2(angle.Y, angle.X);
            projectile.velocity = Vector2.Zero;
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            // no-op, just follow the leader
        }
    }

    public class BalloonBuddyMinion : EmpoweredMinion<BalloonBuddyMinionBuff>
    {
        private static float[] backingArray;
        public static CircularLengthQueue PositionLog = null;
        public int framesSinceLastHit = 0;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Balloon Buddy");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

        public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 28;
			projectile.height = 36;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<BalloonBuddyMinion>();
            projectile.ai[0] = 0;
            projectile.ai[1] = 0;
            projectile.minionSlots = -1;
            backingArray = new float[255];
            CircularVectorQueue.Initialize(backingArray);
            PositionLog = new CircularLengthQueue(backingArray, queueSize: 32);
            PositionLog.mod = mod;
		}

        protected int GetSegmentCount()
        {
            return GetMinionsOfType(ProjectileType<BalloonBuddyBodyMinion>()).Count;
        }

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            if(player.ownedProjectileCounts[ProjectileType<BalloonBuddyBodyMinion>()] < 2)
            {
                OnEmpower();
            }
            if(player.ownedProjectileCounts[ProjectileType<BalloonBuddyTailMinion>()] ==0)
            {
                Projectile.NewProjectile(projectile.position, Vector2.Zero, ProjectileType<BalloonBuddyTailMinion>(), 0, 0, Main.myPlayer);
            }
            projectile.ai[1] = (projectile.ai[1] + 1) % 240;
            Vector2 idlePosition = player.Top;
            idlePosition.X += 48 * (float)Math.Cos(Math.PI * projectile.ai[1] / 60);
            idlePosition.Y += -48 + 8 * (float)Math.Sin(Math.PI * projectile.ai[1] / 120);
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }
        protected override void OnEmpower()
        {
            Projectile.NewProjectile(projectile.position, Vector2.Zero, ProjectileType<BalloonBuddyBodyMinion>(), 0, 0, Main.myPlayer);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            framesSinceLastHit = 0;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // TODO: don't count the balloon for collisions
            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            base.TargetedMovement(vectorToTargetPosition);
            float inertia = ComputeInertia();
            float speed = ComputeTargetedSpeed();
            vectorToTargetPosition.Normalize();
            vectorToTargetPosition *= speed;
            if(framesSinceLastHit ++ > 8)
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
            return (baseDamage /2) + baseDamage/4 * GetSegmentCount();
        }

        protected override float ComputeSearchDistance()
        {
            return 400 + 30 * GetSegmentCount();
        }

        protected override float ComputeInertia()
        {
            return 22 - GetSegmentCount();
        }

        protected override float ComputeTargetedSpeed()
        {
            return 6 + GetSegmentCount();
        }

        protected override float ComputeIdleSpeed()
        {
            return ComputeTargetedSpeed() + 2;
        }

        protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
        {
            minFrame = 0;
            maxFrame = 1;
        }

        public override void AfterMoving()
        {
            base.AfterMoving();
            Vector2 positionOffset = projectile.Center;
            positionOffset.Y -= 2;
            if(projectile.oldDirection != projectile.direction)
            {
            } else if(projectile.direction == -1)
            {
                positionOffset.X += 6;
            } else
            {
                positionOffset.X -= 18;
            }
            PositionLog.AddPosition(positionOffset);
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            base.Animate(minFrame, maxFrame);
            projectile.spriteDirection = projectile.velocity.X > 0 ? 1 : -1;
        }
    }
}
