using DemoMod.Projectiles.Minions.MinonBaseClasses;
using log4net.Repository.Hierarchy;
using log4net.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.EclipseHerald
{
    public class EclipseHeraldMinionBuff: MinionBuff
    {
        public EclipseHeraldMinionBuff() : base(ProjectileType<EclipseHeraldMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Eclipse Herald");
			Description.SetDefault("A herald of the eclipse will fight for you!");
        }
    }

    public class EclipseHeraldMinionItem: EmpoweredMinionItem<EclipseHeraldMinionBuff, EclipseHeraldMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Eclipse Herald Staff");
			Tooltip.SetDefault("Can't come to grips \nWith the total eclipse \nJust a slip of your lips \nand you're gone...");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
            item.damage = 55;
			item.value = Item.buyPrice(0, 20, 0, 0);
			item.rare = ItemRarityID.Red;
		}
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FragmentSolar, 18);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class EclipseHeraldMinion : EmpoweredMinion<EclipseHeraldMinionBuff>
    {

        private int framesSinceLastHit;
        private const int AnimationFrames = 120;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Eclipse Herald");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 9;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 66;
			projectile.height = 60;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<EclipseHeraldMinion>();
            projectile.ai[0] = 0;
            projectile.ai[1] = 0;
            framesSinceLastHit = 0;
            projectile.friendly = true;
            attackThroughWalls = true;
            useBeacon = false;
            frameSpeed = 5;
		}

        private Color ShadowColor(Color original)
        {
           return new Color(original.R/2, original.G/2, original.B/2);
        }

        private void DrawSuns(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 pos = projectile.Center;
            pos.Y -= 24;
            pos.X -= 8 * projectile.spriteDirection;
            float r = (float)(2 * Math.PI * projectile.ai[1]) / AnimationFrames;
            int index = Math.Min(5, (int)projectile.minionSlots - 1);
            Rectangle bounds = new Rectangle(0, 64 * index, 64, 64);
            Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
            Texture2D texture = Main.projectileTexture[ProjectileType<EclipseSphere>()];
            // main
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                bounds, lightColor , r,
                origin, 1, 0, 0);
        }


        private void DrawShadows(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 pos = projectile.Center;
            pos.Y -= 4; // don't know why this offset needs to exist
            Rectangle bounds = new Rectangle(0, 52 * projectile.frame, 66, 52);
            Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
            Color shadowColor = ShadowColor(lightColor);
            Texture2D texture = Main.projectileTexture[projectile.type];
            SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
            // echo 1
            float offset = 2f * (float)Math.Sin(Math.PI * (projectile.ai[1] % 60) / 30);
            spriteBatch.Draw(texture, pos - Main.screenPosition + Vector2.One * offset,
                bounds, shadowColor, projectile.rotation, origin, 1, effects, 0);
            // echo 2
            spriteBatch.Draw(texture, pos - Main.screenPosition - Vector2.One * offset,
                bounds, shadowColor, projectile.rotation, origin, 1, effects, 0);

        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            projectile.ai[1] = (projectile.ai[1] +1) % AnimationFrames;
            DrawSuns(spriteBatch, lightColor);
            DrawShadows(spriteBatch, lightColor);
            return true;
        }

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            Vector2 idlePosition = player.Top;
            idlePosition.X += 48 * -player.direction;
            idlePosition.Y += -32;
            if(!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
            {
                idlePosition.X = player.Top.X;
                idlePosition.Y = player.Top.Y - 16;
            }
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(ref vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            base.IdleMovement(vectorToIdlePosition);
            Lighting.AddLight(projectile.position, Color.White.ToVector3() * 0.5f);
        }
        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            // stay floating behind the player at all times
            IdleMovement(vectorToIdle);
            framesSinceLastHit++;
            if(framesSinceLastHit ++ > 60 && targetNPCIndex is int npcIndex)
            {
                vectorToTargetPosition.Normalize();
                vectorToTargetPosition *= 8;
                Vector2 pos = projectile.Center;
                pos.Y -= 24;
                Projectile.NewProjectile(pos, vectorToTargetPosition, 
                    ProjectileType<EclipseSphere>(), 
                    projectile.damage, 
                    projectile.knockBack, 
                    Main.myPlayer,
                    projectile.minionSlots - 1,
                    npcIndex);
                framesSinceLastHit = 0;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            base.OnHitNPC(target, damage, knockback, crit);
            framesSinceLastHit = 0;
        }
        protected override int ComputeDamage()
        {
            return baseDamage/2 + (baseDamage/2) * (int)projectile.minionSlots;
        }

        private Vector2? GetTargetVector()
        {
            float searchDistance = ComputeSearchDistance();
            if (PlayerTargetPosition(searchDistance, player.Center, searchDistance/2) is Vector2 target)
            {
                return target - projectile.Center;
            }
            else if (ClosestEnemyInRange(searchDistance, player.Center, searchDistance/2) is Vector2 target2)
            {
                return target2 - projectile.Center;
            }
            else
            {
                return null;
            }
        }
        public override Vector2? FindTarget()
        {
            Vector2? target = GetTargetVector();
            return target;
        }

        protected override float ComputeSearchDistance()
        {
            return 700 + 100 * projectile.minionSlots;
        }

        protected override float ComputeInertia()
        {
            return 5;
        }

        protected override float ComputeTargetedSpeed()
        {
            return 16;
        }

        protected override float ComputeIdleSpeed()
        {
            return 16;
        }

        protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
        {
            if(vectorToTarget != null)
            {
                minFrame = 5;
                maxFrame = 9;
            } else if (projectile.velocity.Y < 3)
            {
                minFrame = 0;
                maxFrame = 4;
            } else
            {
                minFrame = 4;
                maxFrame = 4;
            }
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            base.Animate(minFrame, maxFrame);
            if(Math.Abs(projectile.velocity.X) > 2)
            {
                projectile.spriteDirection = projectile.velocity.X > 0 ? 1 : -1;
            }
            projectile.rotation = projectile.velocity.X * 0.05f;
        }
    }
}
