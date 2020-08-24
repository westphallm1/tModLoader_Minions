using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AmuletOfManyMinions.Projectiles.Minions;
using static Terraria.ModLoader.ModContent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmuletOfManyMinions.Projectiles.NonMinionSummons.CannonOfWorms
{
    class CannonOfWorms
    {
        class CannonOfWormsItem: ModItem
        {
            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Cannon Of Worms");
                Tooltip.SetDefault("Shoots a worm to bump into enemies.\n" +
                    "Can have more worms active the more minion slots you have\n" +
                    "(worms do not occupy slots)");
            }

            public override void SetDefaults()
            {
                item.useStyle = ItemUseStyleID.HoldingOut;
                item.width = 34;
                item.height = 24;
                item.summon = true;
                item.noMelee = true;
                item.knockBack = 0.1f;
                item.damage = 12;
                item.shoot = ProjectileType<WormProjectile>();
                item.shootSpeed = 9;
                item.autoReuse = true;
                item.useTime = 30;
                item.mana = 3;
                item.useAnimation = 30;
                item.UseSound = SoundID.Item11;
            }

            public override bool CanUseItem(Player player)
            {
                // only allow a certain number of worms to be spawned at a time
                var currentProjectiles = new List<Projectile>();
                for(int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if(p.active && p.type == item.shoot && Main.player[p.owner] == player)
                    {
                        currentProjectiles.Add(p);
                    }
                }
                if(currentProjectiles.Count >= 1 + player.maxMinions)
                {
                    Projectile oldest = currentProjectiles.OrderBy(p => p.timeLeft).FirstOrDefault();
                    if(oldest != default)
                    {
                        oldest.Kill();
                    }
                }
                return true;
            }
        }

        class WormProjectile : NonMinionSummonedProjectile
        {
            const int TIME_TO_LIVE = 60 * 60; // 1 minute;

            bool hasLanded;
            int framesToTurn;
            Random random = new Random();

            public override void SetStaticDefaults()
            {
                base.SetStaticDefaults();
                Main.projFrames[projectile.type] = 2;
            }
            public override void SetDefaults()
            {
                base.SetDefaults();
                frameSpeed = 15;
                projectile.width = 14;
                projectile.height = 8;
                projectile.tileCollide = true;
                hasLanded = false;
                projectile.timeLeft = TIME_TO_LIVE;
                framesToTurn = 400 + 30 * random.Next(-3, 3);
            }

            public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
            {
                fallThrough = false;
                return true;
            }

            public override bool OnTileCollide(Vector2 oldVelocity)
            {
                if(oldVelocity.Y != 0 && projectile.velocity.Y == 0)
                {
                    hasLanded = true;
                }
                if(oldVelocity.X != 0 && projectile.velocity.X == 0)
                {
                    projectile.velocity.X = -Math.Sign(oldVelocity.X);
                }
                return false;
            }

            public override void IdleMovement(Vector2 vectorToIdlePosition)
            {
                if(projectile.timeLeft < TIME_TO_LIVE - 30)
                {
                    projectile.velocity.Y += .5f;
                }
                if(hasLanded)
                {
                    projectile.velocity.X = Math.Sign(projectile.velocity.X);
                }
                if(hasLanded && projectile.timeLeft % framesToTurn == 0) // turn around every so often
                {
                    projectile.velocity.X *= -3;
                }
            }

            public override void Animate(int minFrame = 0, int? maxFrame = null)
            {
                if(!hasLanded)
                {
                    maxFrame = 1;
                    projectile.rotation += 0.2f;
                } else
                {
                    maxFrame = 2;
                    projectile.rotation = 0;
                    projectile.spriteDirection = projectile.direction;
                }
                base.Animate(minFrame, maxFrame);
            }
        }
    }
}
