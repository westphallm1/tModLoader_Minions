using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.NonMinionSummons.WormOnAString;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.WormOnAString
{
    class WormOnAString : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Can of Worms on a String");
            Tooltip.SetDefault("Increases your minion damage by 1\n" +
                "Has a chance to create a temporary worm minion on defeating an enemy");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 32;
            item.accessory = true;
            item.value = Item.sellPrice(gold: 5);
            item.rare = ItemRarityID.White;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<NecromancerAccessoryPlayer>().wormOnAStringEquipped = true;
        }
    }

    class NecromancerAccessoryPlayer: ModPlayer
    {
        public bool wormOnAStringEquipped = false;

        public override void ResetEffects()
        {
            wormOnAStringEquipped = false;
        }

        public override void ModifyWeaponDamage(Item item, ref float add, ref float mult, ref float flat)
        {
            if(!item.summon)
            {
                return;
            }
            if(wormOnAStringEquipped)
            {
                flat += 1;
            }
        }
    }

    struct SpawnConfig
    {
        internal float onKillChance;
        internal int bossLifePerSpawn;
        internal int projType;
        internal float baseDamage;
        internal int maxTransientMinions;
        internal float spawnVelocity;
    }

    class NecromancerMinionGlobalProjectile: GlobalProjectile
    {
        private bool SpawnProjectileOnChance(Projectile projectile, NPC target, int damage, SpawnConfig config)
        {
            Player player = Main.player[projectile.owner];
            bool shouldSpawnProjectile = !target.boss && target.life <= 0 && Main.rand.NextFloat() < config.onKillChance;
            shouldSpawnProjectile |= target.boss && Main.rand.NextFloat() < damage / config.bossLifePerSpawn;
            if(!shouldSpawnProjectile)
            {
                return false;
            }
            Vector2 spawnVelocity = projectile.velocity;
            spawnVelocity.SafeNormalize();
            spawnVelocity *= config.spawnVelocity;
            var currentProjectiles = new List<Projectile>();
            for(int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if(p.active && p.type == config.projType && Main.player[p.owner] == player)
                {
                    currentProjectiles.Add(p);
                }
            }
            if(currentProjectiles.Count > config.maxTransientMinions)
            {
                Projectile oldest = currentProjectiles.OrderBy(p => p.timeLeft).FirstOrDefault();
                if(oldest != default)
                {
                    oldest.Kill();
                }
            }
            Projectile.NewProjectile(target.Center, spawnVelocity, config.projType, (int)(config.baseDamage * player.minionDamageMult), 2, player.whoAmI);
            return true;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
        {
            if(!projectile.minion && !ProjectileID.Sets.MinionShot[projectile.type])
            {
                return;
            }
            Player player = Main.player[projectile.owner];
            NecromancerAccessoryPlayer modPlayer = player.GetModPlayer<NecromancerAccessoryPlayer>();
            if (modPlayer.wormOnAStringEquipped &&
                SpawnProjectileOnChance(projectile, target, damage, new SpawnConfig {
                    baseDamage = 8,
                    bossLifePerSpawn = 200,
                    maxTransientMinions = 3,
                    onKillChance = 0.3f,
                    projType = ProjectileType<WormProjectile>(),
                    spawnVelocity = 5
                })) 
            {
                // todo make this more generic
                for(int i = 0; i < 10; i ++)
                {
                    Dust.NewDust(projectile.Center - Vector2.One * 16, 32, 32, DustID.Dirt);
                }
            }
        }
    }

}
