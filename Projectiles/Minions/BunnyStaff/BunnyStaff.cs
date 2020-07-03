using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.BunnyStaff
{
    public class BunnyMinionBuff: MinionBuff
    {
        public BunnyMinionBuff() : base(ProjectileType<BunnyMinion>(), ProjectileType<BunnyMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Rabbit in a Hat");
			Description.SetDefault("A magical bunny will fight for you!");
        }
    }

    public class BunnyMinionItem: MinionItem<BunnyMinionBuff, BunnyMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Rabbit in a Hat");
			Tooltip.SetDefault("Summons a magical bunny to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.damage = 5;
			item.knockBack = 2f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Blue;
		}
    }

    public class BunnyMinion : SimpleMinion<BunnyMinionBuff>
    {
        // number of times we've tried jumping out of the current situation
        private int escapeAttempts = 0;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bunny Minion");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 7;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 48;
			projectile.height = 40;
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Jump(-24); // jump slightly on hitting an NPC to prevent latching on
            //base.OnHitNPC(target, damage, knockback, crit);
        }
        public override Vector2 IdleBehavior()
        {
            Vector2 idlePosition = player.Bottom;
            idlePosition.X += (10 + projectile.minionPos * 40) * -player.direction;
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        private void Jump(float targetHeightDifference, Vector2? velocity = null, Vector2? target = null)
        {
            // if not falling
            if (projectile.velocity.Y == 0)
            {
                if (targetHeightDifference < -48f)
                {
                    // big jump
                    projectile.velocity.Y = -12f;
                }
                else if (targetHeightDifference < -16f)
                {
                    // small jump
                    projectile.velocity.Y = -6f;
                } else if (velocity is Vector2 vel && target is Vector2 targ && 
                    Math.Abs(vel.X) < 0.1 && Math.Abs(targ.X) > 80f) {
                    // stopgap to try to get unstuck from slopes
                    projectile.velocity.Y = -6 + -6 * escapeAttempts;
                    escapeAttempts = 1;
                } else
                {
                    escapeAttempts = 0;
                }
            }
        }

        public override Vector2? FindTarget()
        {
			projectile.friendly = true;
            if (PlayerTargetPosition(1500f) is Vector2 target)
            {
                return target - projectile.Center;
            }
            else if (ClosestEnemyInRange(700f) is Vector2 target2)
            {
                return target2 - projectile.Center;
            }
            else
            {
				projectile.friendly = false;
                return null;
            }
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
			// Default movement parameters (here for attacking)
            Jump(vectorToTargetPosition.Y, projectile.velocity, vectorToTargetPosition);
			float speed = 4f;
			float inertia = 1f;
            // Minion has a target: attack (here, fly towards the enemy)
            if (vectorToTargetPosition.Length() > 40f) {
                // The immediate range around the target (so it doesn't latch onto it when close)
                vectorToTargetPosition.Normalize();
                vectorToTargetPosition *= speed;
                projectile.velocity.X = (projectile.velocity.X * (inertia - 1) + vectorToTargetPosition.X) / inertia;
            }
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            Jump(vectorToIdlePosition.Y, projectile.velocity, vectorToIdlePosition);
			float speed;
			float inertia;
            // Minion doesn't have a target: return to player and idle
            if (vectorToIdlePosition.Length() > 600f) {
                // Speed up the minion if it's away from the player
                speed = 12f;
                inertia = 1f;
            }
            else {
                // Slow down the minion if closer to the player
                speed = 4f;
                inertia = 1f;
            }
            if (vectorToIdlePosition.Length() > 20f) {
                // The immediate range around the player (when it passively floats about)

                // This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
                vectorToIdlePosition.Normalize();
                vectorToIdlePosition *= speed;
                projectile.velocity.X = (projectile.velocity.X * (inertia - 1) + vectorToIdlePosition.X) / inertia;
            }
        }

        public override void Animate(int minRange = 0, int? maxRange = null)
        {
            if(projectile.velocity.Length() < 0.25)
            {
                base.Animate(0, 2);
            } else
            {
                base.Animate();
            }
			projectile.spriteDirection = projectile.velocity.X > 0 ? -1: 1;
			// Some visuals here
			//Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 0.78f);
        }

        public override void AfterMoving()
        {
            base.AfterMoving();
            // something is blocking our movement
            projectile.velocity.Y += 0.55f; // hack: use an odd number to prevent air jumping
        }
    }
}
