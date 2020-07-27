using DemoMod.Projectiles.Minions.MinonBaseClasses;
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

namespace DemoMod.Projectiles.Minions.BoneSerpent
{
    public class BoneSerpentMinionBuff: MinionBuff
    {
        public BoneSerpentMinionBuff() : base(ProjectileType<BoneSerpentMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Bone Serpent");
			Description.SetDefault("A flying snake will fight for you!");
        }
    }

    public class BoneSerpentMinionItem: EmpoweredMinionItem<BoneSerpentMinionBuff, BoneSerpentMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bone Serpent Staff");
			Tooltip.SetDefault("Summons a flying snake to fight for you!");
            
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
            item.damage = 18;
			item.height = 32;
			item.value = Item.buyPrice(0, 15, 0, 0);
			item.rare = ItemRarityID.Lime;
		}
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Bone, 30);
            recipe.AddIngredient(ItemID.HellstoneBar, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class BoneSerpentMinion : EmpoweredMinion<BoneSerpentMinionBuff>
    {
        private float[] backingArray;
        public CircularLengthQueue PositionLog = null;
        public int framesSinceLastHit = 0;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Snake");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

        public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 1;
			projectile.height = 1;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<BoneSerpentMinion>();
            projectile.ai[0] = 0;
            projectile.ai[1] = 0;
            projectile.minionSlots = 1;
            backingArray = new float[255];
            CircularVectorQueue.Initialize(backingArray);
            PositionLog = new CircularLengthQueue(backingArray, queueSize: 32)
            {
                mod = mod
            };
        }

        private SpriteEffects GetEffects(float angle)
        {
            SpriteEffects effects = SpriteEffects.FlipHorizontally;
            angle = (angle + 2 * (float)Math.PI) % (2 * (float)Math.PI); // get to (0, 2PI) range
            if(angle > Math.PI/2 && angle < 3  * Math.PI /2)
            {
                effects |= SpriteEffects.FlipVertically;
            }
            return effects;

        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            DrawTail(texture, spriteBatch, lightColor);
            DrawBody(texture, spriteBatch, lightColor);
            DrawHead(texture, spriteBatch, lightColor);

            return false;
        }

        private void DrawHead(Texture2D texture, SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle head = new Rectangle(0, 0, 34, 28);
            Vector2 angle = new Vector2();
            Vector2 pos = PositionLog.PositionAlongPath(2, ref angle);
            Vector2 origin = new Vector2(head.Width / 2f, head.Height / 2f);
            float r = angle.ToRotation();
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                head, Color.White, r,
                origin, 1, GetEffects(r), 0);
        }
        private void DrawBody(Texture2D texture, SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle body = new Rectangle(0, 28, 16, 28);
            Vector2 angle = new Vector2();
            Vector2 origin = new Vector2(body.Width / 2f, body.Height / 2f);
            for(int i = 0; i < GetSegmentCount() + 1; i++)
            {
                Vector2 pos = PositionLog.PositionAlongPath(26 + 15*i, ref angle);
                float r = angle.ToRotation();
                spriteBatch.Draw(texture, pos - Main.screenPosition,
                    body, Color.White, r,
                    origin, 1, GetEffects(r), 0);
            }

        }
        private void DrawTail(Texture2D texture, SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle tail = new Rectangle(0, 56, 14, 28);
            Vector2 angle = new Vector2();
            Vector2 origin = new Vector2(tail.Width / 2f, tail.Height / 2f);
            int dist = 26 + 15 * (GetSegmentCount() + 1);
            Vector2 pos = PositionLog.PositionAlongPath(dist, ref angle);
            float r = angle.ToRotation();
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                tail, Color.White, r,
                origin, 1, GetEffects(r), 0);
        }

        protected int GetSegmentCount()
        {
            return (int)projectile.minionSlots;
        }

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            projectile.ai[1] = (projectile.ai[1] + 1) % 240;
            Vector2 idlePosition = player.Top;
            idlePosition.X += 48 * (float)Math.Cos(Math.PI * projectile.ai[1] / 60);
            idlePosition.Y += -48  + 8 * (float)Math.Sin(Math.PI * projectile.ai[1] / 60);
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }
        protected override void OnEmpower()
        {
            base.OnEmpower();
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            framesSinceLastHit = 0;
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            base.TargetedMovement(vectorToTargetPosition);
            float inertia = ComputeInertia();
            float speed = ComputeTargetedSpeed();
            vectorToTargetPosition.Normalize();
            vectorToTargetPosition *= speed;
            if(framesSinceLastHit ++ > 4)
            {
                projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
            } else
            {
                projectile.velocity.Normalize();
                projectile.velocity *= speed; // kick it away from enemies that it's just hit
            }
        }

        protected override int ComputeDamage()
        {
            return baseDamage/2 + (baseDamage / 2) * GetSegmentCount();
        }

        protected override float ComputeSearchDistance()
        {
            return 600 + 25 * GetSegmentCount();
        }

        protected override float ComputeInertia()
        {
            return 22 - GetSegmentCount();
        }

        protected override float ComputeTargetedSpeed()
        {
            return 4 + 2 * GetSegmentCount();
        }

        protected override float ComputeIdleSpeed()
        {
            return ComputeTargetedSpeed() + 3;
        }

        protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
        {
            minFrame = 0;
            maxFrame = 0;
        }

        public override void AfterMoving()
        {
            base.AfterMoving();
            PositionLog.AddPosition(projectile.position);
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            base.Animate(minFrame, maxFrame);
        }
    }
}
