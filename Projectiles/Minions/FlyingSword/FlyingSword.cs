using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.FlyingSword
{
    public class FlyingSwordMinionBuff: MinionBuff
    {
        public FlyingSwordMinionBuff() : base(ProjectileType<FlyingSwordMinion>(), ProjectileType<FlyingSwordMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Flying Sword");
			Description.SetDefault("A flying sword will fight for you!");
        }
    }

    public class FlyingSwordMinionItem: MinionItem<FlyingSwordMinionBuff, FlyingSwordMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Sword");
			Tooltip.SetDefault("Summons a flying sword to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.damage = 48;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.value = Item.buyPrice(0, 12, 0, 0);
			item.rare = ItemRarityID.Pink;
		}
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SoulofFlight, 10);
            recipe.AddIngredient(ItemID.HallowedBar, 12);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
    public class FlyingSwordMinion : GroupAwareMinion<FlyingSwordMinionBuff>
    {

        int hitCount = 0;
        int maxHitCount = 8;
        int framesInAir = 0;
        int maxFramesInAir = 90;
        int enemyHitFrame = 0;


		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Sword");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 4;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<FlyingSwordMinion>();
            projectile.ai[0] = 0;
            attackState = AttackState.IDLE;
            attackFrames = 120;
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if(hitCount++ >= maxHitCount)
            {
                attackState = AttackState.RETURNING;
            }
            enemyHitFrame = framesInAir;
            Dust.NewDust(projectile.Center, projectile.width / 2, projectile.height / 2, DustID.Platinum);
        }

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            List<Projectile> minions = GetActiveMinions();
            int minionCount = minions.Count;
            int order = minions.IndexOf(projectile);
            float idleAngle = (float)(2 * Math.PI * order) / minionCount;
            if(minions.Count > 0)
            {
                idleAngle += (2 * (float)Math.PI * minions[0].ai[1]) / animationFrames;
            }
            Vector2 idlePosition = player.Center;
            idlePosition.X += (30 + projectile.minionPos * 20) * -player.direction;
            idlePosition.Y += -35 + 5 * (float) Math.Sin(idleAngle);
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        public override Vector2? FindTarget()
        {
            if(FindTargetInTurnOrder(950f, projectile.Center) is Vector2 target)
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
            int speed = 15;
            framesInAir++;
            if((enemyHitFrame == 0 || enemyHitFrame + 9 < framesInAir) && vectorToTargetPosition.Length() > 8 )
            {
                vectorToTargetPosition.Normalize();
                vectorToTargetPosition *= speed;
                projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
                projectile.rotation += (float)Math.PI / 9;
            } else
            {
                projectile.velocity.Normalize();
                projectile.velocity *= speed; // travel straight away from the impact
            }
            if(framesInAir >= maxFramesInAir)
            {
                attackState = AttackState.RETURNING;
            }
            Lighting.AddLight(projectile.position, Color.LightGray.ToVector3() * 0.5f);
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            // alway clamp to the idle position
            int inertia = 5;
            int maxSpeed = 32;
            if(vectorToIdlePosition.Length() < 32)
            {
                // return to the attacking state after getting back home
                attackState = AttackState.IDLE;
                hitCount = 0;
                framesInAir = 0;
                enemyHitFrame = 0;
            }
            Vector2 speedChange = vectorToIdlePosition - projectile.velocity;
            if(speedChange.Length() > maxSpeed)
            {
                speedChange.Normalize();
                speedChange *= maxSpeed;
            }
            projectile.velocity = (projectile.velocity * (inertia - 1) + speedChange) / inertia;

			float intendedRotation = projectile.velocity.X * 0.05f;
            projectile.rotation = intendedRotation;
        }
    }
}
