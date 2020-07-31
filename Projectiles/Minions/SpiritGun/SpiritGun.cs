using DemoMod.Projectiles.Minions.MinonBaseClasses;
using log4net.Repository.Hierarchy;
using log4net.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.SpiritGun
{
    public class SpiritGunMinionBuff: MinionBuff
    {
        public SpiritGunMinionBuff() : base(ProjectileType<SpiritGunMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Spirit Gun");
			Description.SetDefault("A herald of the eclipse will fight for you!");
        }
    }

    public class SpiritGunMinionItem: EmpoweredMinionItem<SpiritGunMinionBuff, SpiritGunMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spirit Gun Staff");
			Tooltip.SetDefault("Can't come to grips \nWith the total eclipse \nJust a slip of your lips \nand you're gone...");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
            item.damage = 50;
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

    public class SpiritGunMinion : EmpoweredMinion<SpiritGunMinionBuff>
    {

        private int framesSinceLastHit;
        private const int AnimationFrames = 120;
        private int animationFrame;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spirit Gun");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 2;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 44;
			projectile.height = 26;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<SpiritGunMinion>();
            projectile.ai[0] = 0;
            animationFrame = 0;
            animationFrame = 0;
            framesSinceLastHit = 0;
            projectile.friendly = true;
		}

        private Color ShadowColor(Color original)
        {
           return new Color(original.R/2, original.G/2, original.B/2);
        }

        private void DrawSpirits(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 pos = projectile.Center;
            float r = (float)(2 * Math.PI * animationFrame) / AnimationFrames;
            Rectangle bounds = new Rectangle(0, projectile.height, 10, 14);
            Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
            Texture2D texture = Main.projectileTexture[projectile.type];
            // main
            for(int i = 0; i <= projectile.minionSlots+1; i++)
            {
                float r1 = r + 2 * (float)Math.PI * i / (projectile.minionSlots + 1);
                Vector2 pos1 = pos + new Vector2((float)Math.Cos(r1), (float)Math.Sin(r1)) * 32;
                spriteBatch.Draw(texture, pos1 - Main.screenPosition,
                    bounds, lightColor , 0,
                    origin, 1, 0, 0);
            }
        }


        private void DrawShadows(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 pos = projectile.Center;
            Rectangle bounds = new Rectangle(0, 0, projectile.width, projectile.height);
            Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
            Color shadowColor = ShadowColor(lightColor);
            Texture2D texture = Main.projectileTexture[projectile.type];
            SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
            // echo 1
            float offset = 2f * (float)Math.Sin(Math.PI * (animationFrame % 60) / 30);
            spriteBatch.Draw(texture, pos - Main.screenPosition + Vector2.One * offset,
                bounds, shadowColor, projectile.rotation, origin, 1, effects, 0);
            // echo 2
            spriteBatch.Draw(texture, pos - Main.screenPosition - Vector2.One * offset,
                bounds, shadowColor, projectile.rotation, origin, 1, effects, 0);

        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            animationFrame = (animationFrame +1) % AnimationFrames;
            DrawSpirits(spriteBatch, lightColor);
            DrawShadows(spriteBatch, lightColor);
            return true;
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
                // TODO
                //Projectile.NewProjectile(pos, vectorToTargetPosition, 
                //    ProjectileType<EclipseSphere>(), 
                //    projectile.damage, 
                //    projectile.knockBack, 
                //    Main.myPlayer,
                //    projectile.minionSlots - 1,
                //    npcIndex);
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
            return baseDamage;
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
            minFrame = 0;
            maxFrame = 0;
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            base.Animate(minFrame, maxFrame);
            if(Math.Abs(projectile.velocity.X) > 2)
            {
                projectile.spriteDirection = projectile.velocity.X > 0 ? 1 : -1;
            }
            projectile.rotation = 0.025f * projectile.velocity.X;
        }
    }
}
