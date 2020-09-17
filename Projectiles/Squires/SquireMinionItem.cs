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
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if(player.ownedProjectileCounts[item.shoot] > 0)
            {
                return false;
            }
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }

        public override bool CanUseItem(Player player)
        {
            if(player.ownedProjectileCounts[item.shoot] > 0)
            {
                item.useStyle = ItemUseStyleID.HoldingUp;
                item.channel = true;
                item.mana = 0;
                item.UseSound = null;
                return true;
            } else
            {
                item.useStyle = ItemUseStyleID.SwingThrow;
                item.channel = false;
                item.mana = 10;
                item.UseSound = SoundID.Item44;
                return base.CanUseItem(player);
            }
        }
    }
}
