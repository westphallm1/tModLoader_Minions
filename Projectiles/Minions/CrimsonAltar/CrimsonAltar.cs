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
			DisplayName.SetDefault("Crimson Altar");
			Description.SetDefault("A goblin gunner will fight for you!");
        }
    }

    public class CrimsonAltarMinionItem: EmpoweredMinionItem<CrimsonAltarMinionBuff, CrimsonAltarMinion>
    {
        protected override int dustType => DustID.Blood;

        public override string Texture => "Terraria/Item_" + ItemID.CrimsonHeart;
        public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crimson Altar");
			Tooltip.SetDefault("Summons a crimson altar to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
            item.damage = 15;
			item.value = Item.buyPrice(0, 15, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SoulofNight, 6);
            recipe.AddIngredient(ItemID.TitaniumBar, 12);
            recipe.AddTile(TileID.MythrilAnvil);
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
            Dust.NewDust(projectile.Center, 1, 1, DustID.Blood, -projectile.velocity.X / 2, -projectile.velocity.Y / 2);
        }

        public override void Kill(int timeLeft)
        {
            base.Kill(timeLeft);
            for(int i = 0; i < 3; i++)
            {
                Dust.NewDust(projectile.Center, projectile.width, projectile.height, DustID.Blood, projectile.velocity.X / 2, projectile.velocity.Y / 2);
            }
        }
    }
    public class CrimsonAltarBigCrimera : CrimsonAltarBaseCrimera
    {
        public override string Texture => "AmuletOfManyMinions/Projectiles/Minions/CrimsonAltar/CrimsonAltarCrimera";
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.scale = 2f;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Ichor, 120);
        }
    }

    public class CrimsonAltarCrimera : CrimsonAltarBaseCrimera
    {
    }
    public class CrimsonAltarMinion : EmpoweredMinion<CrimsonAltarMinionBuff>
    {

        private int framesSinceLastHit;
        public override string Texture => "Terraria/Projectile_" + ProjectileID.CrimsonHeart;

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Goblin Gunner");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 4;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 44;
			projectile.height = 44;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<CrimsonAltarMinion>();
            projectile.ai[0] = 0;
            projectile.ai[1] = 0;
            framesSinceLastHit = 0;
            projectile.friendly = true;
            attackThroughWalls = true;
            useBeacon = false;
            frameSpeed = 5;
		}



        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 196);
            Texture2D texture = GetTexture("Terraria/Item_" + ItemID.Ichor);
            Rectangle bounds = texture.Bounds;
            Vector2 origin = bounds.Center.ToVector2();
            bounds.Height = (int)(Math.Min(projectile.minionSlots, 4) * bounds.Height / 4f);
            Vector2 pos = projectile.Center;
            float r = projectile.rotation;
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                bounds, translucentColor, r,
                origin, 1, 0, 0);
        }

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            Vector2 idlePosition = player.Top;
            idlePosition.X += 16 * -player.direction;
            idlePosition.Y += -8;
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
                    float rangeSquare = Math.Min(200, vectorToTargetPosition.Length() / 2);
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
            return 500 + 10 * projectile.minionSlots;
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
            if(Math.Abs(projectile.velocity.X) > 2 && vectorToTarget is null)
            {
                projectile.spriteDirection = projectile.velocity.X > 0 ? -1 : 1;
            }
            projectile.rotation = projectile.velocity.X * 0.05f;
        }
    }
}
