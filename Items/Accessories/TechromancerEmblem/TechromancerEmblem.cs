using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using AmuletOfManyMinions.Projectiles.Squires.StardustSquire;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.TechromancerEmblem
{
    class TechromancerEmblem : NecromancerAccessory
    {
        public override string Texture => "Terraria/Item_" + ItemID.NecromanticScroll;
        protected override float baseDamage => 34;
        protected override int bossLifePerSpawn => 1000;
        protected override int maxTransientMinions => 3;
        protected override float onKillChance => .2f;

        protected override int projType => ProjectileType<TechromancerSkull>();

        protected override float spawnVelocity => 9;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Technomancer Emblem");
            Tooltip.SetDefault("Increases your minion damage by 12%\n" +
                "Mechanical Skulls will rise from your fallen foes!");
        }

        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;
            item.accessory = true;
            item.value = Item.sellPrice(gold: 5);
            item.rare = ItemRarityID.LightRed;

        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<NecromancerAccessoryPlayer>().techromancerAccessoryEquipped = true;
            player.minionDamageMult += 0.12f;
        }

        internal override bool IsEquipped(NecromancerAccessoryPlayer player)
        {
            return player.techromancerAccessoryEquipped;
        }
    }

    public class TechromancerSkull : BumblingTransientMinion
    {
        protected override int timeToLive => 60 * 10; // 10 seconds;
        protected override float inertia => 12;
        protected override float idleSpeed => maxSpeed * 0.75f;
        protected override float searchDistance => 600f;
        protected override float distanceToBumbleBack => 400f;
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.projFrames[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            frameSpeed = 15;
            projectile.width = 32;
            projectile.height = 32;
            projectile.tileCollide = true;
        }

        public override Vector2 IdleBehavior()
        {
            projectile.rotation = projectile.velocity.ToRotation();
            return base.IdleBehavior();
        }

        protected override SpriteEffects GetSpriteEffects()
        {
            if(projectile.velocity.X < 0)
            {
                return SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
            }
            return SpriteEffects.FlipHorizontally;
        }
    }
}
