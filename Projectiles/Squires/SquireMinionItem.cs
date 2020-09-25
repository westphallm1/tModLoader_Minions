using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AmuletOfManyMinions.Projectiles.Minions;
using System;

namespace AmuletOfManyMinions.Projectiles.Squires
{
    public abstract class SquireMinionItem<TBuff, TProj> : MinionItem<TBuff, TProj> where TBuff: ModBuff where TProj: SquireMinion<TBuff>
    {

        public override void SetDefaults()
        {
            base.SetDefaults();
            item.autoReuse = true;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.channel = true;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if(player.ownedProjectileCounts[item.shoot] > 0)
            {
                return false;
            }
            player.AddBuff(item.buffType, 2);
            Projectile.NewProjectile(player.Center, Vector2.Zero, item.shoot, item.damage, item.knockBack, Main.myPlayer);
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            base.CanUseItem(player);
            if (player.ownedProjectileCounts[item.shoot] > 0)
            {
                return false;
            }
            return true;
        }
    }

}
