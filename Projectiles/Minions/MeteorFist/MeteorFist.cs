using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.MeteorFist
{
    public class MeteorFistMinionBuff: MinionBuff
    {
        public MeteorFistMinionBuff() : base(ProjectileType<MeteorFistMinion>(), ProjectileType<MeteorFistMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Meteor Fist");
			Description.SetDefault("A possessed dagger will fight for you!");
        }
    }

    public class MeteorFistMinionItem: MinionItem<MeteorFistMinionBuff, MeteorFistMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Meteor Fist Staff");
			Tooltip.SetDefault("Summons a possessed dagger to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.damage = 18;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Blue;
		}

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
            Projectile.NewProjectile(position + new Vector2(5, 0), new Vector2(speedX, speedY), item.shoot, damage, knockBack, Main.myPlayer);
            Projectile.NewProjectile(position - new Vector2(5, 0), new Vector2(speedX, speedY), item.shoot, damage, knockBack, Main.myPlayer);
            return false;
        }
    }


    public class MeteorFistMinion : GroupAwareMinion<MeteorFistMinionBuff>
    {

        private int framesInAir;
        private float idleAngle;

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Meteor Fist");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 2;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<MeteorFistMinion>();
            projectile.ai[0] = 0;
            attackState = AttackState.IDLE;
            projectile.minionSlots = 0.5f;
            attackFrames = 30;
		}

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if(projectile.tileCollide)
            {
                attackState = AttackState.RETURNING;
                projectile.tileCollide = false;
            }
            return base.OnTileCollide(oldVelocity);
        }

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            List<Projectile> minions = GetActiveMinions();
            Projectile leader = GetFirstMinion(minions);
            if(leader.minionPos == projectile.minionPos && 
                player.ownedProjectileCounts[ProjectileType<MeteorFistHead>()] == 0)
            {
                Projectile.NewProjectile(projectile.position, Vector2.Zero, ProjectileType<MeteorFistHead>(), 0, 0, Main.myPlayer);
            }
            Projectile head = GetMinionsOfType(ProjectileType<MeteorFistHead>()).FirstOrDefault();
            if(head == default)
            {
                // the head got despawned, wait for it to respawn
                return Vector2.Zero;
            }
            Vector2 idlePosition = head.Center;
            int minionCount = minions.Count;
            int order = minions.IndexOf(projectile);
            idleAngle = (float)(2 * Math.PI * order) / minionCount;
            idleAngle += projectile.spriteDirection * 2 * (float)Math.PI * minions[0].ai[1] / animationFrames;
            idlePosition.X += 2 + 30 * (float)Math.Sin(idleAngle);
            idlePosition.Y += 2 + 30 * (float)Math.Cos(idleAngle);
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        public override Vector2? FindTarget()
        {
            Projectile head = GetMinionsOfType(ProjectileType<MeteorFistHead>()).FirstOrDefault();
            if(FindTargetInTurnOrder(400f, head.Center) is Vector2 target)
            {
                projectile.friendly = true;
                return target;
            } else
            {
                projectile.friendly = false;
                return null;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            framesInAir = Math.Max(framesInAir, 12); // force a return shortly after hitting a target
            Dust.NewDust(projectile.position, 16, 16, DustID.Fire, projectile.velocity.X / 2, projectile.velocity.Y /2);
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            int speed = 16;
            projectile.spriteDirection = vectorToTargetPosition.X > 0 ? 1 : -1;
            if(oldVectorToTarget == null && vectorToTarget is Vector2 target)
            {
                target.Y -= Math.Abs(target.X) / 10; // add a bit of vertical increase to target
                target.Normalize();
                target *= speed;
                framesInAir = 0;
                projectile.velocity = target;
                projectile.rotation = (float)(Math.Atan2(projectile.velocity.Y, projectile.spriteDirection * projectile.velocity.X));
            }
            if(framesInAir++ > 15)
            {
                attackState = AttackState.RETURNING;
            }
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            // attack should continue until ground is hit
            if(attackState == AttackState.ATTACKING)
            {
                TargetedMovement(Vector2.Zero);
                return;
            }
            projectile.rotation = (float)Math.PI;
            // alway clamp to the idle position
            projectile.tileCollide = false;
            int inertia = 2;
            int maxSpeed = 20;
            if(vectorToIdlePosition.Length() < 32)
            {
                attackState = AttackState.IDLE;
                Projectile head = GetMinionsOfType(ProjectileType<MeteorFistHead>()).FirstOrDefault();
                projectile.spriteDirection = head.spriteDirection;
                projectile.position += vectorToIdlePosition;
                projectile.velocity = Vector2.Zero;
            } else
            {
                Vector2 speedChange = vectorToIdlePosition - projectile.velocity;
                if(speedChange.Length() > maxSpeed)
                {
                    speedChange.Normalize();
                    speedChange *= maxSpeed;
                }
                projectile.velocity = (projectile.velocity * (inertia - 1) + speedChange) / inertia;
            }
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            base.Animate(minFrame, maxFrame);
            Lighting.AddLight(projectile.position, Color.Red.ToVector3() * 0.75f);
        }
    }
}
