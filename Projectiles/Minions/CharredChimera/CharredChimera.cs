using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CharredChimera
{
    public class CharredChimeraMinionBuff: MinionBuff
    {
        public CharredChimeraMinionBuff() : base(ProjectileType<CharredChimeraMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Charred Chimera");
			Description.SetDefault("A winged acorn will fight for you!");
        }
    }

    public class CharredChimeraMinionItem: EmpoweredMinionItem<CharredChimeraMinionBuff, CharredChimeraMinion>
    {
        protected override int dustType => 54;
        public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Charred Chimera Staff");
			Tooltip.SetDefault("Summons a winged acorn to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.damage = 48;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 2, 0);
			item.rare = ItemRarityID.Yellow;
		}
    }

    public class CharredChimeraMinionHead :GroupAwareMinion<CharredChimeraMinionBuff>
    {

        public override string Texture => "Terraria/Item_0";
        int speed = 8;
        int inertia = 16;
        int framesSinceLastHit;
        int hitsSinceRetreat;
        Projectile body = default;
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.projFrames[projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.minion = false;
            projectile.minionSlots = 0;
            useBeacon = false;
            projectile.width = 16;
            projectile.height = 16;
            attackFrames = 180;
            ProjectileID.Sets.Homing[projectile.type] = true;
            ProjectileID.Sets.MinionShot[projectile.type] = true;
            framesSinceLastHit = 5;
            hitsSinceRetreat = 0;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            return false;
        }

        public override Vector2? FindTarget()
        {
            int maxHits = Math.Max(1, 10 - GetActiveMinions().Count());
            if(vectorToIdle.Length() > 300f || projectile.ai[1] == 0 || hitsSinceRetreat > maxHits)
            {
                return null;
            }
            if(ClosestEnemyInRange(300f, projectile.Center, 0f, false) is Vector2 target)
            {
                projectile.friendly = true;
                return target - projectile.Center;
            }
            return null;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            framesSinceLastHit = 0;
            projectile.friendly = false;
            projectile.velocity = -projectile.oldVelocity;
            hitsSinceRetreat++;
        }

        public override Vector2 IdleBehavior()
        {
            body = GetMinionsOfType(ProjectileType<CharredChimeraMinion>()).FirstOrDefault();
            framesSinceLastHit++;
            projectile.ai[0] = (projectile.ai[0] + 1) % attackFrames;
            if(body == default)
            {
                return Vector2.Zero;
            } else
            {
                return body.Center - projectile.Center;
            }
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            int speed = 16;
            int inertia = 4;
            if(vectorToIdlePosition.Length() < 16)
            {
                // the body is responsible for controlling the heads when they're "attached"
                projectile.ai[1] = 0;
                hitsSinceRetreat = 0;
                return;
            } else if (vectorToIdlePosition.Length() > 300f)
            {
                speed = 20;
                inertia = 8;
            } 
            vectorToIdlePosition.Normalize();
            vectorToIdlePosition *= speed;
            projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            if(framesSinceLastHit < 5)
            {
                return;
            }
            projectile.ai[1] = 1;
            DistanceFromGroup(ref vectorToTargetPosition, closeDistance: 300);
            int speed = this.speed + (vectorToTargetPosition.Length() < 48 ? 4 : 0);
            vectorToTargetPosition.Normalize();
            vectorToTargetPosition *= speed;
            if(body != default)
            {
                projectile.spriteDirection = -Math.Sign((body.Center - projectile.Center).X);
            }
            projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            // no op
        }
    }
    public class CharredChimeraMinion : EmpoweredMinion<CharredChimeraMinionBuff>
    {
        protected int targetedInertia = 15;
        protected int targetedSpeed = 12;
        protected int maxDistanceFromPlayer = 850;
        protected int minDistanceToEnemy = 200;
        protected int animationFrames = 120;
        protected int animationFrame = 0;

        protected List<Projectile> allHeads = default;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            projectile.type = ProjectileType<CharredChimeraMinion>();
            DisplayName.SetDefault("Charred Chimera");
            // Sets the amount of frames this minion has on its spritesheet
            Main.projFrames[projectile.type] = 4;
        }

        public sealed override void SetDefaults()
        {
            base.SetDefaults();
            projectile.width = 32;
            projectile.height = 32;
            drawOriginOffsetX = (56-32) / 2;
            drawOriginOffsetY = (52 - 32) / 2;
            projectile.tileCollide = false;
            projectile.friendly = false;
            attackThroughWalls = false;
            frameSpeed = 5;
            animationFrame = 0;
        }

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            Vector2 idlePosition = player.Top;
            animationFrame++;
            float idleAngle = (float)Math.PI * 2 * (animationFrame %animationFrames)/ animationFrames;
            idlePosition.X += 70 * -player.direction;
            idlePosition.Y += -5 + 8 * (float)Math.Sin(idleAngle);
            if(!Collision.CanHit(idlePosition, 1, 1, player.Top, 1, 1))
            {
                idlePosition.X = player.Top.X;
            }
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(ref vectorToIdlePosition, 2000f);
            Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 0.5f);
            int headType = ProjectileType<CharredChimeraMinionHead>();
            int currentHeadCount = player.ownedProjectileCounts[headType];
            for(int i = currentHeadCount; i < projectile.minionSlots + 1; i++)
            {
                Projectile.NewProjectile(projectile.Center, projectile.velocity, headType, projectile.damage, projectile.knockBack, player.whoAmI);
            }
            allHeads = GetMinionsOfType(ProjectileType<CharredChimeraMinionHead>());
            TellHeadsToAttack();
            PositionHeads();
            return vectorToIdlePosition;
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            int inertia = 10;
            int maxSpeed = 16;
            Vector2 speedChange = vectorToIdlePosition - projectile.velocity;
            if (speedChange.Length() > maxSpeed)
            {
                speedChange.SafeNormalize();
                speedChange *= maxSpeed;
            }
            projectile.spriteDirection = player.direction;
            projectile.velocity = (projectile.velocity * (inertia - 1) + speedChange) / inertia;
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            int inertia = targetedInertia;
            int maxSpeed = targetedSpeed;
            // move towards the enemy, but don't get too far from the player
            projectile.spriteDirection = vectorToTargetPosition.X > 0 ? 1 : -1;
            if (vectorToTargetPosition.Length() < minDistanceToEnemy){
                vectorToTargetPosition *= -1;
            }
            if(Math.Abs((vectorToTargetPosition.Length() - minDistanceToEnemy)) < 16)
            {
                //projectile.position += vectorToTargetPosition;
                inertia = 40; // don't jitter
            } 
            vectorToTargetPosition.SafeNormalize();
            vectorToTargetPosition *= maxSpeed;
            projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
        }

        private static Vector2[,] IdleHeadOffsets = {
            { Vector2.Zero, default, default, default },
            { new Vector2(4, 0), new Vector2(-8, 4), default, default },
            { new Vector2(12, 4), new Vector2(4, -4), new Vector2(-8, 4), default},
            { new Vector2(8, 0), new Vector2(0, 8), new Vector2(0, -8), new Vector2(-8, 0) }
        };

        private void TellHeadsToAttack()
        {
            if(allHeads.Count == 0)
            {
                return;
            }
            int attackFrames = (int)(Math.Min(20, 60 - 5 * projectile.minionSlots) * projectile.minionSlots);
            int attackFrame = animationFrame % attackFrames;
            int interval = (attackFrames / allHeads.Count) % attackFrames;
            for(int i = 0; i < allHeads.Count; i++)
            {
               if(allHeads[i].ai[1] == 0 && attackFrame == i * interval)
               {
                    allHeads[i].ai[1] = 2;
               } else if (allHeads[i].ai[1] == 2 && attackFrame != i * interval)
                {
                    allHeads[i].ai[1] = 0;
                }
            }
        }
        private void PositionHeads()
        {
            var heads = allHeads.Where(h=>h.ai[1] != 1).ToList();
            Vector2 spinalCordEndOffset = new Vector2(28 * projectile.spriteDirection, -4);
            Vector2 headBasePosition = projectile.Top + spinalCordEndOffset + new Vector2(-13, -14) + projectile.velocity;
            if(heads.Count() == 0)
            {
                return;
            } else if(allHeads.Count() <= IdleHeadOffsets.GetLength(0))
            {
                for(int i = 0; i < heads.Count(); i++)
                {
                    Vector2 headOffset = IdleHeadOffsets[allHeads.Count() - 1, i];
                    headOffset.X *= projectile.spriteDirection;
                    heads[i].position = headBasePosition +  headOffset;
                    heads[i].velocity = Vector2.Zero;
                    heads[i].spriteDirection = projectile.spriteDirection;
                }
            } else
            {
                float baseIdleAngle = (float)Math.PI * 2 * (animationFrame %animationFrames)/ animationFrames;
                float idleAngleStep =  (float)(2 * Math.PI) / (allHeads.Count() - 1);
                int headRadius = 4 + 4* heads.Count();
                for(int i = 0; i < heads.Count() - 1; i++)
                {
                    float headAngle = baseIdleAngle + idleAngleStep * i;
                    Vector2 headOffset = headRadius * new Vector2((float)Math.Cos(headAngle), (float)Math.Sin(headAngle));
                    heads[i].position = headBasePosition + headOffset;
                    heads[i].velocity = Vector2.Zero;
                    heads[i].spriteDirection = projectile.spriteDirection;
                }
                heads.Last().position = headBasePosition;
                heads.Last().velocity = Vector2.Zero;
                heads.Last().spriteDirection = projectile.spriteDirection;
            }
        }

        private void DrawVertibrae(SpriteBatch spriteBatch, Color lightColor, Projectile head)
        {
            Vector2 spinalCordEndOffset = new Vector2(24 * projectile.spriteDirection, -4);
            Vector2 endPosition = (projectile.Top + spinalCordEndOffset) - head.Center;
            Vector2 center = head.Center;
            Rectangle bounds = new Rectangle(6, 36, 12, 14);
            Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
            Vector2 pos;
            float r;
            if (endPosition.Length() > 16)
            {
                Vector2 unitToIdle = endPosition;
                unitToIdle.Normalize();
                Texture2D vertibraeTexture = GetTexture(Texture + "_Head");
                r = (float)Math.PI / 2 + endPosition.ToRotation();
                int i;
                for (i = bounds.Height / 2; i < endPosition.Length(); i += bounds.Height)
                {
                    if (endPosition.Length() - i < bounds.Height / 2)
                    {
                        i = (int)(endPosition.Length() - bounds.Height / 2);
                    }
                    pos = center + unitToIdle * i;
                    lightColor = Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16);
                    spriteBatch.Draw(vertibraeTexture, pos - Main.screenPosition,
                        bounds, lightColor, r,
                        origin, 1, SpriteEffects.None, 0);
                }
            }
            
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float r = projectile.rotation;
            Vector2 pos = projectile.Center;
            Texture2D texture = GetTexture(Texture);
            SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                bounds, lightColor, r,
                origin, 1, effects, 0);
            foreach(Projectile head in allHeads)
            {
                DrawVertibrae(spriteBatch, lightColor, head);
            }
            return false;
        }

        private void DrawHeads(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = GetTexture(Texture + "_Head");
            foreach(Projectile head in allHeads)
            {
                float r = head.rotation;
                Vector2 pos = head.Center;
                SpriteEffects effects = head.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
                int frameHeight = texture.Height / Main.projFrames[head.type];
                Rectangle bounds = new Rectangle(0, 0, texture.Width, frameHeight);
                Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
                spriteBatch.Draw(texture, pos - Main.screenPosition,
                    bounds, lightColor, r,
                    origin, 1, effects, 0);
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            DrawHeads(spriteBatch, lightColor);
        }

        protected override int ComputeDamage()
        {
            return baseDamage;
        }

        protected override float ComputeSearchDistance()
        {
            return 950f;
        }

        protected override float ComputeInertia()
        {
            return 15;
        }

        protected override float ComputeTargetedSpeed()
        {
            return 14;
        }

        protected override float ComputeIdleSpeed()
        {
            return ComputeIdleSpeed() + 3;
        }

        protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
        {
            minFrame = 0;
            maxFrame = 4;
        }
    }
}
