using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using log4net.Repository.Hierarchy;
using log4net.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.BoneSerpent
{
    public class BoneSerpentMinionBuff: MinionBuff
    {
        public BoneSerpentMinionBuff() : base(ProjectileType<BoneSerpentMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Bone Serpent");
			Description.SetDefault("A skeletal dragon will fight for you!");
        }
    }

    public class BoneSerpentMinionItem: EmpoweredMinionItem<BoneSerpentMinionBuff, BoneSerpentMinion>
    {
        protected override int dustType => 30;
        public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bone Serpent Staff");
			Tooltip.SetDefault("Summons a skeletal dragon to fight for you!");
            
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
            item.damage = 30;
			item.height = 32;
			item.value = Item.buyPrice(0, 1, 0, 0);
			item.rare = ItemRarityID.Orange;
		}
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Bone, 75);
            recipe.AddTile(TileID.Hellforge);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class BoneSerpentMinion : WormMinion<BoneSerpentMinionBuff>
    {

        private int framesInAir;
        private int framesInGround;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bone Serpent");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

        protected bool InTheGround(Vector2 position)
        {
            int x = (int)position.X / 16;
            int y = (int)position.Y / 16;
            Tile tile = Main.tile[x, y];
            return Main.tile[x, y].collisionType == 1 || Main.tileSolidTop[tile.type];
        }

        public sealed override void SetDefaults() {
			base.SetDefaults();
            attackThroughWalls = true;
            projectile.type = ProjectileType<BoneSerpentMinion>();
            framesInAir = 0;
            framesInGround = 0;
        }

        protected override void DrawHead()
        {
            Rectangle head = new Rectangle(56, 0, 48, 36);
            lightColor = new Color(
                Math.Max(lightColor.R, (byte)25),
                Math.Max(lightColor.G, (byte)25),
                Math.Max(lightColor.B, (byte)25));
            AddSprite(2, head);
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            if(framesInAir < 10 || Vector2.Distance(player.Center, projectile.Center) > 300f || Math.Abs(player.Center.Y - projectile.Center.Y) > 80f)
            {
                base.IdleMovement(vectorToIdlePosition);
            }
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            if(framesInAir < 10)
            {
                base.TargetedMovement(vectorToTargetPosition);
            }
        }

        public override Vector2 IdleBehavior()
        {
            if(!InTheGround(projectile.Center))
            {
                framesInAir++;
                framesInGround = 0;
                Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 0.25f);
            } else
            {
                framesInGround ++;
                framesInAir = 0;
            }
            if(framesInAir > 10 && projectile.velocity.Y < 16)
            {
                projectile.velocity.Y += 0.25f;
            }
            return base.IdleBehavior();
        }
        protected override void DrawBody()
        {
            Rectangle body = new Rectangle(28, 6, 24, 30);
            lightColor = new Color(
                Math.Max(lightColor.R, (byte)25),
                Math.Max(lightColor.G, (byte)25),
                Math.Max(lightColor.B, (byte)25));
            for(int i = 0; i < GetSegmentCount() + 1; i++)
            {
                AddSprite(32 + 20*i, body);
            }

        }
        protected override void DrawTail()
        {
            Rectangle tail = new Rectangle(0, 10, 24, 22);
            lightColor = new Color(
                Math.Max(lightColor.R, (byte)25),
                Math.Max(lightColor.G, (byte)25),
                Math.Max(lightColor.B, (byte)25));
            int dist = 32 + 20 * (GetSegmentCount() + 1);
            AddSprite(dist, tail);
        }

        protected override float ComputeSearchDistance()
        {
            return 500 + 25 * GetSegmentCount();
        }

        protected override float ComputeInertia()
        {
            if(framesInAir > 0)
            {
                return 30;
            } else if (framesInGround < 4)
            {
                return 10;
            } else
            {
                return 22 - GetSegmentCount();
            }
        }

        protected override float ComputeTargetedSpeed()
        {
            return Math.Min(12, 4 + 2 * GetSegmentCount());
        }

        protected override float ComputeIdleSpeed()
        {
            return ComputeTargetedSpeed() + 3;
        }

        public override Vector2? FindTarget()
        {
            float searchDistance = ComputeSearchDistance();
            if (PlayerTargetPosition(searchDistance, player.Center, searchDistance) is Vector2 target)
            {
                return target - projectile.Center;
            }
            else if (ClosestEnemyInRange(searchDistance, player.Center, searchDistance) is Vector2 target2)
            {
                return target2 - projectile.Center;
            }
            else
            {
                return null;
            }
        }
    }
}
