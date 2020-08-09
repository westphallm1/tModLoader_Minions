using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.CrystalFist
{
    public class CrystalFistMinionBuff: MinionBuff
    {
        public CrystalFistMinionBuff() : base(ProjectileType<CrystalFistMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Crystal Fist");
			Description.SetDefault("A crystal fist will fight for you!");
        }
    }

    public class CrystalFistMinionItem: MinionItem<CrystalFistMinionBuff, CrystalFistMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crystal Fist Staff");
			Tooltip.SetDefault("Summons a crystal fist to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.damage = 42;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 16;
			item.height = 16;
			item.value = Item.buyPrice(0, 12, 0, 0);
			item.rare = ItemRarityID.Pink;
		}

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
            Projectile.NewProjectile(position + new Vector2(5, 0), new Vector2(speedX, speedY), item.shoot, damage, knockBack, Main.myPlayer);
            Projectile.NewProjectile(position - new Vector2(5, 0), new Vector2(speedX, speedY), item.shoot, damage, knockBack, Main.myPlayer);
            return false;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SoulofMight, 10);
            recipe.AddIngredient(ItemID.CrystalShard, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }


    public class CrystalFistMinion : GroupAwareMinion<CrystalFistMinionBuff>
    {

        private int framesInAir;
        private float idleAngle;

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crystal Fist");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 20;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<CrystalFistMinion>();
            projectile.ai[0] = 0;
            attackState = AttackState.IDLE;
            projectile.minionSlots = 0.5f;
            attackThroughWalls = false;
            useBeacon = false;
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
                player.ownedProjectileCounts[ProjectileType<CrystalFistHeadMinion>()] == 0)
            {
                Projectile.NewProjectile(projectile.position, Vector2.Zero, ProjectileType<CrystalFistHeadMinion>(), 0, 0, Main.myPlayer);
            }
            Projectile head = GetHead(ProjectileType<CrystalFistHeadMinion>());
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
            idlePosition.X += 2 + 45 * (float)Math.Sin(idleAngle);
            idlePosition.Y += 2 + 45 * (float)Math.Cos(idleAngle);
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        public override Vector2? FindTarget()
        {
            Projectile head = GetHead(ProjectileType<CrystalFistHeadMinion>());
            if(head == default)
            {
                // the head got despawned, wait for it to respawn
                return null;
            }
            if(FindTargetInTurnOrder(600f, head.Center) is Vector2 target)
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
            Dust.NewDust(projectile.position, 16, 16, DustID.PinkCrystalShard, projectile.velocity.X / 2, projectile.velocity.Y /2);
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            int speed = 16;
            if(oldVectorToTarget == null && vectorToTarget is Vector2 target)
            {
                target.Y -= Math.Abs(target.X) / 10; // add a bit of vertical increase to target
                target.Normalize();
                target *= speed;
                framesInAir = 0;
                projectile.velocity = target;
            }
            projectile.spriteDirection = 1; 
            projectile.rotation = (float)(Math.PI + projectile.velocity.ToRotation());
            if(framesInAir++ > 15)
            {
                attackState = AttackState.RETURNING;
            }
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            // attack should continue until attack timer is up
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
                Projectile head = GetHead(ProjectileType<CrystalFistHeadMinion>());
                if(head != default)
                {
                    // the head got despawned, wait for it to respawn
                    projectile.spriteDirection = head.spriteDirection;
                }
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
            Lighting.AddLight(projectile.position, Color.Pink.ToVector3() * 0.75f);
        }
    }
}
