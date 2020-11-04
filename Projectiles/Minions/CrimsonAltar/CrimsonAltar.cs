using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
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

namespace AmuletOfManyMinions.Projectiles.Minions.CrimsonAltar
{
    public class CrimsonAltarMinionBuff: MinionBuff
    {
        public CrimsonAltarMinionBuff() : base(ProjectileType<CrimsonAltarMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Crimson Cell");
			Description.SetDefault("A crimson cell will fight for you!");
        }
    }

    public class CrimsonAltarMinionItem: EmpoweredMinionItem<CrimsonAltarMinionBuff, CrimsonAltarMinion>
    {
        protected override int dustType => DustID.Blood;

        public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crimson Cell Staff");
			Tooltip.SetDefault("Summons a crimson cell to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
            item.damage = 15;
			item.value = Item.sellPrice(0, 0, 70, 0);
			item.rare = ItemRarityID.Green;
		}
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.CrimtaneBar, 12);
            recipe.AddIngredient(ItemID.TissueSample, 6);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public abstract class CrimsonAltarBaseCrimera : BumblingTransientMinion
    {
        protected override float inertia => 20;
        protected override float idleSpeed => 10;

        protected override int timeToLive => 120;

        protected override float distanceToBumbleBack => 2000f; // don't bumble back

        protected override float searchDistance => 220f;

        protected virtual int dustType => DustID.Blood;
        protected virtual int dustFrequency => 5;
        public override void SetDefaults()
        {
            base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.friendly = true;
			projectile.penetrate = 1;
			projectile.tileCollide = true;
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 2;
        }

        protected override void Move(Vector2 vector2Target, bool isIdle = false)
        {
            base.Move(vector2Target, isIdle);
            projectile.rotation = projectile.velocity.ToRotation() + 3 * (float) Math.PI/ 2;
            if(Main.rand.Next(dustFrequency) == 0)
            {
                Dust.NewDust(projectile.Center, 1, 1, dustType, -projectile.velocity.X / 2, -projectile.velocity.Y / 2);
            }
        }

        public override void Kill(int timeLeft)
        {
            base.Kill(timeLeft);
            for(int i = 0; i < 3; i++)
            {
                Dust.NewDust(projectile.Center, projectile.width, projectile.height, dustType, projectile.velocity.X / 2, projectile.velocity.Y / 2);
            }
        }
    }
    public class CrimsonAltarBigCrimera : CrimsonAltarBaseCrimera
    {
        public override string Texture => "AmuletOfManyMinions/Projectiles/Minions/CrimsonAltar/CrimsonAltarCrimera";

        protected override int dustType => 87;
        public override void SetDefaults()
        {
            base.SetDefaults();
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = GetTexture(Texture);
            Rectangle bounds = new Rectangle(0, 0, 
                texture.Bounds.Width, texture.Bounds.Height / 2);
            Vector2 origin = bounds.Center.ToVector2();
            Vector2 pos = projectile.Center;
            float r = projectile.rotation;
            SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                bounds, lightColor, r,
                origin, 1.5f, 0, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Ichor, 90);
        }
    }

    public class CrimsonAltarCrimera : CrimsonAltarBaseCrimera
    {
    }
    public class CrimsonAltarMinion : EmpoweredMinion<CrimsonAltarMinionBuff>
    {

        private int framesSinceLastHit;
        private int animationFrame;

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crimson Cell");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 4;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 40;
			projectile.height = 40;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<CrimsonAltarMinion>();
            projectile.ai[0] = 0;
            projectile.ai[1] = 0;
            framesSinceLastHit = 0;
            projectile.friendly = true;
            attackThroughWalls = true;
            useBeacon = false;
		}



        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if(projectile.minionSlots < 4)
            {
                return;
            }
            Texture2D texture = GetTexture(Texture + "_Glow");
            Rectangle bounds = texture.Bounds;
            Vector2 origin = bounds.Center.ToVector2();
            Vector2 pos = projectile.Center;
            float r = projectile.rotation;
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                bounds, Color.White, r,
                origin, 1, 0, 0);
        }

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            Vector2 idlePosition = player.Top;
            idlePosition.X += 28 * -player.direction;
            idlePosition.Y += -8;
            animationFrame+= 1;
            if(!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
            {
                idlePosition.X = player.Top.X;
                idlePosition.Y = player.Top.Y - 16;
            }
            idlePosition.Y += 4 * (float) Math.Sin(animationFrame / 32f);
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(ref vectorToIdlePosition, 2000f);
            Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 0.25f);
            return vectorToIdlePosition;
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            base.IdleMovement(vectorToIdlePosition);
        }
        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            // stay floating behind the player at all times
            IdleMovement(vectorToIdle);
            framesSinceLastHit++;
            int rateOfFire = 120;
            if(framesSinceLastHit ++ > rateOfFire)
            {
                framesSinceLastHit = 0;
                for(int i = 0; i < projectile.minionSlots; i++)
                {
                    bool summonBig = projectile.minionSlots >= 4 && Main.rand.Next(4) == 0; 
                    int projType  = summonBig ? ProjectileType<CrimsonAltarBigCrimera>() : ProjectileType<CrimsonAltarCrimera>();
                    float rangeSquare = Math.Min(120, vectorToTargetPosition.Length() / 2);
                    vectorToTargetPosition.X += Main.rand.NextFloat() * rangeSquare - rangeSquare/2; 
                    vectorToTargetPosition.Y += Main.rand.NextFloat() * rangeSquare - rangeSquare/2;
                    int projectileVelocity = summonBig? 8 : 12;
                    vectorToTargetPosition.SafeNormalize();
                    vectorToTargetPosition *= projectileVelocity;
                    Vector2 pos = projectile.Center;
                    framesSinceLastHit = 0;
                    Projectile.NewProjectile(pos, vectorToTargetPosition,
                        projType,
                        projectile.damage,
                        projectile.knockBack,
                        Main.myPlayer);
                }
            }
        }

        protected override int ComputeDamage()
        {
            return baseDamage + (baseDamage/8) * (int)projectile.minionSlots; // only scale up damage a little bit
        }

        private Vector2? GetTargetVector()
        {
            float searchDistance = ComputeSearchDistance();
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
        public override Vector2? FindTarget()
        {
            Vector2? target = GetTargetVector();
            return target;
        }

        protected override float ComputeSearchDistance()
        {
            return 500 + 30 * projectile.minionSlots;
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
            projectile.spriteDirection = 1;
            projectile.frame = Math.Min(4, (int)projectile.minionSlots) - 1;
            projectile.rotation += player.direction / 32f;
            if(Main.rand.Next(120) == 0)
            {
                for(int i = 0; i < 3; i++)
                {
                    Dust.NewDust(projectile.Center, 16, 16, DustID.Blood, Main.rand.Next(6) - 3, Main.rand.Next(6) - 3);
                }
            }
        }
    }
}
