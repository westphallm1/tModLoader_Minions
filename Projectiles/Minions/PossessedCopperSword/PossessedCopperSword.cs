using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.PossessedCopperSword
{
    public class CopperSwordMinionBuff: MinionBuff
    {
        public CopperSwordMinionBuff() : base(ProjectileType<CopperSwordMinion>(), ProjectileType<CopperSwordMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Possessed Copper Sword");
			Description.SetDefault("A possessed copper sword will fight for you!");
        }
    }

    public class CopperSwordMinionItem: MinionItem<CopperSwordMinionBuff, CopperSwordMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Possessed Copper Sword");
			Tooltip.SetDefault("Summons a possessed Sword to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.damage = 19;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Blue;
		}
    }
    public class CopperSwordMinion : GroupAwareMinion<CopperSwordMinionBuff>
    {

        private readonly float baseRoation = 3 * (float)Math.PI / 4f;

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Copper Sword");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 32;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<CopperSwordMinion>();
            projectile.ai[0] = 0;
            attackState = AttackState.IDLE;
            attackFrames = 45;
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            attackState = AttackState.RETURNING;
            Lighting.AddLight(projectile.Center, Color.Aqua.ToVector3());
        }

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            Vector2 idlePosition = player.Center;
            idlePosition.X += (20 + projectile.minionPos * 10) * -player.direction;
            idlePosition.Y += -5 * projectile.minionPos;
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        public override Vector2? FindTarget()
        {
            if(FindTargetInTurnOrder(600f, player.Center) is Vector2 target)
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
            int inertia = 5;
            int speed = 12;
            vectorToTargetPosition.Normalize();
            vectorToTargetPosition *= speed;
            projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
            projectile.rotation += (float)Math.PI / 9;
            Dust.NewDust(projectile.Center, projectile.width / 2, projectile.height / 2, 42);
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            // alway clamp to the idle position
            int inertia = 5;
            int maxSpeed = attackState == AttackState.IDLE ? 20 : 12;
            if(vectorToIdlePosition.Length() < 16)
            {
                // return to the attacking state after getting back home
                attackState = AttackState.IDLE;
            }
            Vector2 speedChange = vectorToIdlePosition - projectile.velocity;
            if(speedChange.Length() > maxSpeed)
            {
                speedChange.Normalize();
                speedChange *= maxSpeed;
            }
            projectile.velocity = (projectile.velocity * (inertia - 1) + speedChange) / inertia;

            float intendedRotation = baseRoation + player.direction * (projectile.minionPos * (float)Math.PI / 12);
			intendedRotation += projectile.velocity.X * 0.05f;
            projectile.rotation = intendedRotation;
        }
    }
}
