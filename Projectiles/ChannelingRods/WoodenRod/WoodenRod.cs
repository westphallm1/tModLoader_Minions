using DemoMod.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System.Linq;

namespace DemoMod.Projectiles.ChannelingRods.WoodenRod
{
    class WoodenRodBuff : MinionBuff
    {

    }
    class WoodenRod : MinionItem<WoodenRodBuff, WoodenRodMinion>
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            item.damage = 8;
            item.summon = true;
            item.channel = true;
            item.autoReuse = true;
            item.useStyle = ItemUseStyleID.HoldingUp;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {

            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return false; // don't appear in player's hand
        }
    }

    class WoodenRodMinion : SimpleMinion<WoodenRodBuff>
    {
        public override Vector2? FindTarget()
        {
            throw new NotImplementedException();
        }

        public override Vector2 IdleBehavior()
        {
            throw new NotImplementedException();
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            throw new NotImplementedException();
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            throw new NotImplementedException();
        }
    }
}
