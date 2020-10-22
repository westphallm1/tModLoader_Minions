using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.NonMinionSummons.WormOnAString;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.WormOnAString
{
    class WormOnAString : NecromancerAccessory
    {
        protected override float baseDamage => 8;
        protected override int bossLifePerSpawn => 200;
        protected override int maxTransientMinions => 3;
        protected override float onKillChance => 0.67f;

        protected override int projType => ProjectileType<WormProjectile>();

        protected override float spawnVelocity => 4;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Can of Worms on a String");
            Tooltip.SetDefault("Increases your minion damage by 1\n" +
                "Has a chance to create a temporary worm minion on defeating an enemy");
        }

        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;
            item.accessory = true;
            item.value = Item.sellPrice(gold: 5);
            item.rare = ItemRarityID.White;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<NecromancerAccessoryPlayer>().wormOnAStringEquipped = true;
        }

        internal override void ModifyPlayerWeaponDamage(NecromancerAccessoryPlayer necromancerAccessoryPlayer, Item item, ref float add, ref float mult, ref float flat)
        {
            flat += 1;
        }

        internal override bool IsEquipped(NecromancerAccessoryPlayer player)
        {
            return player.wormOnAStringEquipped;
        }
    }
}
