using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.TurtleDrakeHatchling
{
    public class TurtleDrakeHatchlingMinionBuff: MinionBuff
    {
        public TurtleDrakeHatchlingMinionBuff() : base(ProjectileType<TurtleDrakeHatchlingMinion>(), ProjectileType<TurtleDrakeHatchlingMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Turtle Drake Hatchling");
			Description.SetDefault("A possessed dagger will fight for you!");
        }
    }

    public class TurtleDrakeHatchlingMinionItem: MinionItem<TurtleDrakeHatchlingMinionBuff, TurtleDrakeHatchlingMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Turtle Drake Hatchling Staff");
			Tooltip.SetDefault("Summons a possessed dagger to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.damage = 20;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Blue;
		}

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if(player.ownedProjectileCounts[item.shoot] == 0)
            {
                player.AddBuff(BuffType<TurtleDrakeHatchlingMinionBuff>(), 2);
                Projectile.NewProjectile(position - new Vector2(5, 0), new Vector2(speedX, speedY), item.shoot, damage, knockBack, Main.myPlayer);
            } else
            {
                for (int i = 0; i < Main.maxProjectiles; i++) {
                    // Fix overlap with other minions
                    Projectile other = Main.projectile[i];
                    if (other.active && other.owner == Main.myPlayer && other.type == item.shoot && other.minionSlots < player.maxMinions)
                    {
                        other.ai[0] = 1;
                        break;
                    }
                }
            }
            return false;
        }
    }


    public class TurtleDrakeHatchlingMinion : SimpleMinion<TurtleDrakeHatchlingMinionBuff>
    {

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Turtle Drake Hatchling");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 6;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 32;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<TurtleDrakeHatchlingMinion>();
            projectile.ai[0] = 0;
		}

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // TODO: don't count the balloon for collisions
            return base.Colliding(projHitbox, targetHitbox);
        }
        public override Vector2 IdleBehavior()
        {
            Main.NewText(projectile.minionSlots+ " " + player.maxMinions);
            if(projectile.ai[0] == 1 && projectile.minionSlots < player.maxMinions)
            {
                projectile.minionSlots += 1;
                projectile.ai[0] = 0;
            }
            projectile.damage = 8 + 12 * (int)projectile.minionSlots;
            Vector2 idlePosition = player.Top;
            idlePosition.X += 48 * -player.direction;
            idlePosition.Y += -32;
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        public override Vector2? FindTarget()
        {
            float searchDistance = 400f + 50 * projectile.minionSlots;
            if (PlayerTargetPosition(searchDistance, player.Center) is Vector2 target)
            {
                return target - projectile.Center;
            }
            else if (ClosestEnemyInRange(searchDistance, player.Center) is Vector2 target2)
            {
                return target2 - projectile.Center;
            }
            else
            {
                return null;
            }
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            vectorToTargetPosition.Y += -32; // hit with the body instead of the balloon
            float inertia = Math.Max(1, 40 - 4 * projectile.minionSlots);
            float speed = 4 * projectile.minionSlots;
            vectorToTargetPosition.Normalize();
            vectorToTargetPosition *= speed;
            projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            // alway clamp to the idle position
            float inertia = Math.Max(1, 40 - 4 * projectile.minionSlots);
            float maxSpeed = 2 + 4 * projectile.minionSlots;
            Vector2 speedChange = vectorToIdlePosition - projectile.velocity;
            if(speedChange.Length() > maxSpeed)
            {
                speedChange.Normalize();
                speedChange *= maxSpeed;
            }
            projectile.velocity = (projectile.velocity * (inertia - 1) + speedChange) / inertia;
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            if(projectile.minionSlots == 1)
            {
                minFrame = 0;
                maxFrame = 2;
            } else if (projectile.minionSlots == 2)
            {
                minFrame = 2;
                maxFrame = 4;
            } else
            {
                minFrame = 4;
                maxFrame = 6;
            }
			int frameSpeed = 15;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed) {
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= (maxFrame ?? Main.projFrames[projectile.type]) ||
                    projectile.frame < minFrame ||
                    Math.Abs(projectile.velocity.X) < 2) {
					projectile.frame = minFrame;
				}
			}
            if(Math.Abs(projectile.velocity.X) > 2)
            {
                projectile.spriteDirection = projectile.velocity.X > 0 ? -1 : 1;
            }
            projectile.rotation = projectile.velocity.X * 0.05f;
        }
    }
}
