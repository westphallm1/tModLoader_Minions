using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AmuletOfManyMinions.Projectiles.Minions;
using static Terraria.ModLoader.ModContent;
using System;

namespace AmuletOfManyMinions.Projectiles.NonMinionSummons.CannonOfWorms
{
    class CannonOfWorms
    {
        class CannonOfWormsItem: ModItem
        {
            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Cannon Of Worms");
                Tooltip.SetDefault("Shoots a worm to crawl into enemies.\n" +
                    "Can summon worms for no mana up to your minion count.");
            }

            public override void SetDefaults()
            {
                item.useStyle = ItemUseStyleID.HoldingOut;
                item.width = 34;
                item.height = 24;
                item.summon = true;
                item.noMelee = true;
                item.knockBack = 0.1f;
                item.damage = 7;
                item.shoot = ProjectileType<WormProjectile>();
                item.shootSpeed = 9;
                item.autoReuse = true;
                item.useTime = 30;
                item.mana = 5;
                item.useAnimation = 30;
                item.UseSound = SoundID.Item11;
            }

            public override bool CanUseItem(Player player)
            {
                // only allow a certain number of worms to be spawned at a time
                int sum = 0;
                for(int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if(p.active && p.type == item.shoot && Main.player[p.owner] == player)
                    {
                        sum++;
                    }
                }
                if(sum <= 1 * 2* player.maxMinions)
                {

                    item.mana = 0;
                } else
                {
                    item.mana = 5;
                }
                return true;
            }
        }

        class WormProjectile : NonMinionSummonedProjectile
        {
            bool hasLanded;

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
                projectile.timeLeft = 300;
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
                if(projectile.timeLeft < 270)
                {
                    projectile.velocity.Y += .5f;
                }
                if(hasLanded)
                {
                    projectile.velocity.X = Math.Sign(projectile.velocity.X);
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
