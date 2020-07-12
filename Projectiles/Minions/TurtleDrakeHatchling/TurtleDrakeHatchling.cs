using DemoMod.Projectiles.Minions.MinonBaseClasses;
using log4net.Repository.Hierarchy;
using log4net.Util;
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

    public class TurtleDrakeHatchlingMinionItem: EmpoweredMinionItem<TurtleDrakeHatchlingMinionBuff, TurtleDrakeHatchlingMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Turtle Drake Hatchling Staff");
			Tooltip.SetDefault("Summons a possessed dagger to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Blue;
		}
    }


    public class TurtleDrakeHatchlingMinion : EmpoweredMinion<TurtleDrakeHatchlingMinionBuff>
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
            base.IdleBehavior();
            Vector2 idlePosition = player.Top;
            idlePosition.X += 48 * -player.direction;
            idlePosition.Y += -32;
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            vectorToTargetPosition.Y += -32; // hit with the body instead of the balloon
            base.TargetedMovement(vectorToTargetPosition);
        }

        protected override int ComputeDamage()
        {
            return 8 + 6 * (int)projectile.minionSlots;
        }

        protected override float ComputeSearchDistance()
        {
            return 400 + 50 * projectile.minionSlots;
        }

        protected override float ComputeInertia()
        {
            return Math.Max(10, 40 - 4 * projectile.minionSlots);
        }

        protected override float ComputeTargetedSpeed()
        {
            return 3 + projectile.minionSlots;
        }

        protected override float ComputeIdleSpeed()
        {
            return ComputeTargetedSpeed() + 3;
        }

        protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
        {
            switch(projectile.minionSlots)
            {
                case 1:
                    minFrame = 0;
                    maxFrame = 2;
                    break;
                case 2:
                    minFrame = 2;
                    maxFrame = 4;
                    break;
                default:
                    minFrame = 4;
                    maxFrame = 6;
                    break;
            }
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            base.Animate(minFrame, maxFrame);

            if(Math.Abs(projectile.velocity.X) > 2)
            {
                projectile.spriteDirection = projectile.velocity.X > 0 ? -1 : 1;
            }
            projectile.rotation = projectile.velocity.X * 0.05f;
        }
    }

}
