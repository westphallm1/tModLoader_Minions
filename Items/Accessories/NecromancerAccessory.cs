using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories
{
    public abstract class NecromancerAccessory: ModItem
    {
        internal static List<NecromancerAccessory> accessories = new List<NecromancerAccessory>();
        protected virtual float spawnVelocity => 0;

        protected virtual float onKillChance => 0;
        protected virtual int bossLifePerSpawn => 0;
        protected virtual int projType => 0;
        protected virtual int maxTransientMinions => 0;
        protected virtual float baseDamage => 0;

        internal virtual void ModifyPlayerWeaponDamage(NecromancerAccessoryPlayer necromancerAccessoryPlayer, Item item, ref float add, ref float mult, ref float flat)
        {
            // no op
        }

        public override void SetStaticDefaults()
        {
            accessories.Add(this);
        }
        internal virtual bool SpawnProjectileOnChance(Projectile projectile, NPC target, int damage)
        {
            Player player = Main.player[projectile.owner];
            bool shouldSpawnProjectile = !target.boss && target.life <= 0 && Main.rand.NextFloat() < onKillChance;
            shouldSpawnProjectile |= Main.rand.NextFloat() <  damage / (float) bossLifePerSpawn;
            if (!shouldSpawnProjectile)
            {
                return false;
            }
            Vector2 spawnVelocity = projectile.velocity;
            spawnVelocity.SafeNormalize();
            spawnVelocity *= this.spawnVelocity;
            spawnVelocity.Y = -Math.Abs(spawnVelocity.Y);
            var currentProjectiles = new List<Projectile>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == projType && p.owner == player.whoAmI)
                {
                    currentProjectiles.Add(p);
                }
            }
            if (currentProjectiles.Count >= maxTransientMinions)
            {
                Projectile oldest = currentProjectiles.OrderBy(p => p.timeLeft).FirstOrDefault();
                if (oldest != default)
                {
                    oldest.Kill();
                }
            }
            Projectile.NewProjectile(target.Center, spawnVelocity, projType, (int)(baseDamage * player.minionDamageMult), 2, player.whoAmI);
            return true;
        }

        internal abstract bool IsEquipped(NecromancerAccessoryPlayer player);
    }

    internal class NecromancerAccessoryPlayer: ModPlayer
    {
        public bool wormOnAStringEquipped = false;
        public bool spiritCallerCharmEquipped = false;
        public bool techromancerAccessoryEquipped = false;
        internal bool foragerArmorSetEquipped;

        public override void ResetEffects()
        {
            wormOnAStringEquipped = false;
            spiritCallerCharmEquipped = false;
            techromancerAccessoryEquipped = false;
            foragerArmorSetEquipped = false;
        }

        public override void ModifyWeaponDamage(Item item, ref float add, ref float mult, ref float flat)
        {
            if(!item.summon)
            {
                return;
            }
            foreach(NecromancerAccessory accessory in NecromancerAccessory.accessories)
            {
                if(accessory.IsEquipped(this))
                {
                    accessory.ModifyPlayerWeaponDamage(this, item, ref add, ref mult, ref flat);
                }
            }
        }
    }

    class NecromancerMinionGlobalProjectile : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
        {
            if (!projectile.minion && !ProjectileID.Sets.MinionShot[projectile.type])
            {
                return;
            }
            Player player = Main.player[projectile.owner];
            NecromancerAccessoryPlayer modPlayer = player.GetModPlayer<NecromancerAccessoryPlayer>();
            foreach(NecromancerAccessory accessory in NecromancerAccessory.accessories)
            {
                if(accessory.IsEquipped(modPlayer))
                {
                    accessory.SpawnProjectileOnChance(projectile, target, damage);
                }
            }
        }
    }
}
