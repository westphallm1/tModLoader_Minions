using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.VoidKnife
{
    public class VoidKnifeMinionBuff: MinionBuff
    {
        public VoidKnifeMinionBuff() : base(ProjectileType<VoidKnifeMinion>(), ProjectileType<VoidKnifeMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Void Knife");
			Description.SetDefault("A possessed dagger will fight for you!");
        }
    }

    public class VoidKnifeMinionItem: MinionItem<VoidKnifeMinionBuff, VoidKnifeMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Void Knife Staff");
			Tooltip.SetDefault("Summons a possessed dagger to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.damage = 35;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.value = Item.buyPrice(0, 7, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
            Projectile.NewProjectile(position - new Vector2(5, 0), new Vector2(speedX, speedY), item.shoot, damage, knockBack, Main.myPlayer);
            return false;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SoulofNight, 10);
            recipe.AddIngredient(ItemID.ThrowingKnife, 50);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }


    public class VoidKnifeMinion : GroupAwareMinion<VoidKnifeMinionBuff>
    {

        private int framesInAir;
        private float idleAngle;
        private int maxFramesInAir = 35;
        private bool hasHitEnemy = false;
        private float travelVelocity;
        private Random random = new Random();

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Void Knife");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<VoidKnifeMinion>();
            projectile.ai[0] = 0;
            attackState = AttackState.IDLE;
            projectile.minionSlots = 1;
            attackFrames = 120;
            animationFrames = 120;
            attackThroughWalls = true;
            travelVelocity = 8;
		}

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            List<Projectile> minions = GetActiveMinions();
            Vector2 idlePosition = player.Center;
            int minionCount = minions.Count;
            int order = minions.IndexOf(projectile);
            idleAngle = (float)(2 * Math.PI * order) / minionCount;
            idleAngle += (2 * (float)Math.PI * minions[0].ai[1]) / animationFrames;
            idlePosition.X += 2 + 20 * (float)Math.Cos(idleAngle);
            idlePosition.Y += 2 + 40 * (float)Math.Sin(idleAngle);
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        public override Vector2? FindTarget()
        {
            if(FindTargetInTurnOrder(800f, projectile.Center, 400f) is Vector2 target)
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
            base.OnHitNPC(target, damage, knockback, crit);
            hasHitEnemy = true;
        }

        private void WarpWithDust(Vector2 vectorToNewPosition, bool bigDustBefore)
        {
            // don't spam with dust while moving close to the player 
            if(Vector2.Distance(projectile.position + vectorToNewPosition, player.Center) < 120)
            {
                projectile.position += vectorToNewPosition;
                return;
            }
            Vector2 dustIncrement = new Vector2(vectorToNewPosition.X, vectorToNewPosition.Y);
            dustIncrement.Normalize();
            if(bigDustBefore)
            {
                for(int i = 0; i < 10; i ++)
                {
                    Dust.NewDust(projectile.position, 32, 32, DustID.Shadowflame);
                }
            }
            for(int i = 0; i < vectorToNewPosition.Length(); i +=32)
            {
                Dust.NewDust(projectile.position + dustIncrement * i, 16, 16, DustID.Shadowflame);
            }
            projectile.position += vectorToNewPosition;
            if(!bigDustBefore)
            {
                for(int i = 0; i < 10; i ++)
                {
                    Dust.NewDust(projectile.position, 32, 32, DustID.Shadowflame);
                }
            }
        }
        private void TeleportToEnemy(Vector2 target)
        {
            float distanceFromFoe = 80 + random.Next(-20, 20);
            float teleportAngle = (float)(random.NextDouble() * 2 * Math.PI);
            Vector2 teleportDirection = new Vector2((float)Math.Cos(teleportAngle), (float)Math.Sin(teleportAngle));
            projectile.velocity = -travelVelocity * teleportDirection;
            framesInAir = 0;
            WarpWithDust(target + distanceFromFoe * teleportDirection, false);
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            if(oldVectorToTarget == null && vectorToTarget is Vector2 target)
            {
                TeleportToEnemy(target);
            }
            if(framesInAir++ > maxFramesInAir)
            {
                attackState = AttackState.RETURNING;
            }
            else if (!hasHitEnemy && vectorToTargetPosition != Vector2.Zero)
            {
                vectorToTargetPosition.Normalize();
                projectile.velocity = vectorToTargetPosition * travelVelocity;
            }
            else
            {
                projectile.rotation += (float)Math.PI / 9;
                projectile.velocity.Y += 0.2f;
            }
            Lighting.AddLight(projectile.position, Color.Purple.ToVector3() * 0.75f);
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            if (framesInAir < maxFramesInAir && attackState != AttackState.IDLE)
            {
                TargetedMovement(Vector2.Zero);
                return;
            }
            projectile.rotation = (float)Math.PI;
            // alway clamp to the idle position
            projectile.tileCollide = false;
            
            if(vectorToIdlePosition.Length() > 32)
            {
                WarpWithDust(vectorToIdlePosition, true);
            } else
            {
                attackState = AttackState.IDLE;
                projectile.rotation = idleAngle + (float) Math.PI/2;
                projectile.position += vectorToIdlePosition;
                projectile.velocity = Vector2.Zero;
                hasHitEnemy = false;
            }
        }
    }
}
