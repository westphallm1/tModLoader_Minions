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
			DisplayName.SetDefault("Copper StarSword");
			Description.SetDefault("A possessed copper sword will fight for you!");
        }
    }

    public class CopperSwordMinionItem: MinionItem<CopperSwordMinionBuff, CopperSwordMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Copper StarSword");
			Tooltip.SetDefault("Summons a possessed sword to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.damage = 8;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.value = Item.buyPrice(0, 0, 5, 0);
			item.rare = ItemRarityID.White;
		}
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.CopperShortsword, 1);
            recipe.AddIngredient(ItemID.FallenStar, 3);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
    public class CopperSwordMinion : GroupAwareMinion<CopperSwordMinionBuff>
    {

        private readonly float baseRoation = 3 * (float)Math.PI / 4f;
        private int hitsSinceLastIdle = 0;
        private int framesSinceLastHit = 0;

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Copper StarSword");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<CopperSwordMinion>();
            projectile.ai[0] = 0;
            attackState = AttackState.IDLE;
            attackFrames = 60;
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if(hitsSinceLastIdle++ > 2)
            {
                attackState = AttackState.RETURNING;
            }
            framesSinceLastHit = 0;
            Lighting.AddLight(target.position, Color.LightYellow.ToVector3());
            Dust.NewDust(projectile.Center, projectile.width / 2, projectile.height / 2, DustID.Gold);
        }

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            Vector2 idlePosition = player.Center;
            idlePosition.X += (20 + projectile.minionPos * 10) * -player.direction;
            idlePosition.Y += -5 * projectile.minionPos;
            if(!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
            {
                idlePosition.X = player.Center.X + 20 * -player.direction;
                idlePosition.Y = player.Center.Y -5;
            }
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(ref vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        public override Vector2? FindTarget()
        {
            if(FindTargetInTurnOrder(550f, projectile.Top) is Vector2 target)
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
            int speed = 8;
            vectorToTargetPosition.Normalize();
            vectorToTargetPosition *= speed;
            if(framesSinceLastHit ++ > 10)
            {
                projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
            }
            projectile.rotation += (float)Math.PI / 9;
            Dust.NewDust(projectile.Center, projectile.width / 2, projectile.height / 2, DustID.Copper);
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            // alway clamp to the idle position
            int inertia = 5;
            int maxSpeed = attackState == AttackState.IDLE ? 12 : 8;
            if(vectorToIdlePosition.Length() < 32)
            {
                // return to the attacking state after getting back home
                attackState = AttackState.IDLE;
                hitsSinceLastIdle = 0;
                framesSinceLastHit = 0;
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
