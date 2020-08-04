using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System.Linq;

namespace DemoMod.Projectiles.Minions
{
    public abstract class MinionItem <TBuff, TProj> : ModItem where TBuff : ModBuff where TProj: Minion<TBuff>
    {
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
		}

		public override void SetDefaults() {
			item.useTime = 36;
			item.useAnimation = 36;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.UseSound = SoundID.Item44;
			// These below are needed for a minion weapon
			item.noMelee = true;
			item.summon = true;
			item.buffType = BuffType<TBuff>();
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ProjectileType<TProj>();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			player.AddBuff(item.buffType, 2);

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;
			return true;
		}

        public override bool AltFunctionUse(Player player)
        {
			return true;
        }

		private void AddWaypoint()
        {
			if(Main.player[Main.myPlayer].ownedProjectileCounts[ProjectileType<MinionWaypoint>()] == 0)
            {
				Projectile.NewProjectile(Main.MouseWorld, Vector2.Zero, ProjectileType<MinionWaypoint>(), 0, 0, Main.myPlayer);
            } 
			else
            {
                foreach(Projectile p in Main.projectile) 
                {
                    if(p.owner == Main.myPlayer && p.active && p.type == ProjectileType<MinionWaypoint>())
                    {
                        p.position = Main.MouseWorld;
                        p.timeLeft = MinionWaypoint.duration;
                    }
                }
            }
        }
        public override bool CanUseItem(Player player)
        {
			if(player.altFunctionUse == 2)
            {
				AddWaypoint();	
            }
            return base.CanUseItem(player);
        }
    }
}
