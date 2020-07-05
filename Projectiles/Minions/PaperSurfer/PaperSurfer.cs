using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.PaperSurfer
{
    public class PaperSurferMinionBuff: MinionBuff
    {
        public PaperSurferMinionBuff() : base(ProjectileType<PaperSurferMinion>(), ProjectileType<PaperSurferMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Paper Surfer");
			Description.SetDefault("A possessed copper sword will fight for you!");
        }
    }

    public class PaperSurferMinionItem: MinionItem<PaperSurferMinionBuff, PaperSurferMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paper Surfer Staff");
			Tooltip.SetDefault("Summons a possessed Sword to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.damage = 12;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Blue;
		}
    }
    public class PaperSurferMinion : GroupAwareMinion<PaperSurferMinionBuff>
    {

        protected float idleAngle;
        protected int framesSinceDiveBomb = 0;
        protected int diveBombHeightRequirement = 40;
        protected int diveBombHeightTarget = 120;
        protected int diveBombHorizontalRange = 80;
        protected int diveBombFrameRateLimit = 60;
        protected int diveBombSpeed = 12;
        protected int diveBombInertia = 12;
        protected int approachSpeed = 8;
        protected int approachInertia = 8;

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paper Surfer");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 6;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<PaperSurferMinion>();
            projectile.ai[0] = 0;
            attackState = AttackState.IDLE;
            attackFrames = 30;
            animationFrames = 240;
            projectile.frame = (2 * projectile.minionPos) % 6;
		}

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            List<Projectile> minions = GetActiveMinions();
            Vector2 idlePosition = player.Top;
            int minionCount = minions.Count;
            int order = minions.IndexOf(projectile);
            idleAngle = (2 * PI * order) / minionCount;
            idleAngle += 2 * PI * minions[0].ai[1] / animationFrames;
            idlePosition.X += 2 + 40 * (float)Math.Cos(idleAngle);
            idlePosition.Y += -20 + 10 * (float)Math.Sin(idleAngle);
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            projectile.velocity.Normalize();
            projectile.velocity *= 6; // "kick" it away from the enemy it just hit
            framesSinceDiveBomb = 0;
            base.OnHitNPC(target, damage, knockback, crit);
        }
        public override Vector2? FindTarget()
        {
            if(FindTargetInTurnOrder(800f, player.Center) is Vector2 target)
            {
                projectile.friendly = true;
                return target;
            } else
            {
                projectile.friendly = false;
                return null;
            }
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            // alway clamp to the idle position
            int inertia = 40;
            int speed = 8;
            
            projectile.friendly = framesSinceDiveBomb ++ > 20; // limit rate of attack
            if(framesSinceDiveBomb < 60 || Math.Abs(vectorToTargetPosition.X) > 80)
            {
                // always aim for "above" while approaching
                vectorToTargetPosition.Y -= 120;
                projectile.rotation = 0;
            } else if(vectorToTargetPosition.Y > 40)
            {
                inertia = 15;
                speed = 12;
                projectile.rotation = (projectile.velocity.X > 0 ? 7 : 5) * PI / 4;
            }
            vectorToTargetPosition.Normalize();
            vectorToTargetPosition *= speed;
            projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
            projectile.spriteDirection = projectile.velocity.X > 0 ? -1 : 1;
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            // alway clamp to the idle position
            projectile.tileCollide = false;
            int inertia = 5;
            int maxSpeed = 12;
            if(vectorToIdlePosition.Length() < 32)
            {
                projectile.position += vectorToIdlePosition;
                projectile.velocity = Vector2.Zero;
                projectile.rotation = 0;
                projectile.spriteDirection = (idleAngle % (2* PI)) > PI ? -1 : 1;
            } else
            {
                vectorToIdlePosition.Normalize();
                vectorToIdlePosition *= maxSpeed;
                projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
            }
        }
		public override void Animate(int minFrame = 0, int? maxFrame = null) {

            // This is a simple "loop through all frames from top to bottom" animation
            minFrame = (2 * projectile.minionPos) % 6;
			int frameSpeed = 15;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed) {
                projectile.frameCounter = 0;
                projectile.frame = projectile.frame == minFrame ? minFrame + 1: minFrame;
			}
		}
    }
}
