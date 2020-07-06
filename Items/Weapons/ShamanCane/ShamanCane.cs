using System.Threading.Tasks;
using Terraria;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace DemoMod.Items.Weapons.ShamanCane
{
    public class ShamanCaneBuff: ModBuff
    {
		public override void SetDefaults() {
			Main.buffNoSave[Type] = true;
		}

        public override void Update(Player player, ref int buffIndex)
        {
            if(!player.HasBuff(Type)) //when about to despawn, un-apply the damage bonus
            {
                for (int i = 0; i < Main.maxProjectiles; i++) {
                    // Fix overlap with other minions
                    Projectile other = Main.projectile[i];
                    if (other.active && other.owner == Main.myPlayer && other.minion )
                    {
                        other.damage -= ShamanCane.MinionDamageBonus;
                    }
                }
            }
        }
    }

    public class ShamanCaneProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.penetrate = 1;
            projectile.maxPenetrate = 1;
            projectile.tileCollide = true;
            projectile.timeLeft = 40;
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ignoreWater = true;

            // always travel at speed 6
            projectile.velocity.Normalize();
            projectile.velocity *= 7;
            projectile.timeLeft = 90;
        }

        public override void AI()
        {
            base.AI();
            Lighting.AddLight(projectile.position, Color.Yellow.ToVector3() * 0.25f);
        }
        private void ApplyMinionDamageBonus()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                // Fix overlap with other minions
                Projectile other = Main.projectile[i];
                if (other.active && other.owner == Main.myPlayer && other.minion)
                {
                    other.damage += ShamanCane.MinionDamageBonus;
                }
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            base.OnHitNPC(target, damage, knockback, crit);
            Player player = Main.player[projectile.owner];
            // bad hack to workaround not knowing how to use ModPlayer
            // only apply the buff's effect before they have it,
            // then let the buff's effect remove it when it despawns
            if (!player.HasBuff(BuffType<ShamanCaneBuff>()))
            {
                ApplyMinionDamageBonus();
            }
            player.AddBuff(BuffType<ShamanCaneBuff>(), 3 * 60);
        }
    }

    public class ShamanCane : ModItem
    {
        public static int MinionDamageBonus = 10;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
            ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
            DisplayName.SetDefault("Shaman's Cane");
        }

        public override void SetDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
            ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
            item.damage = 15;
            item.mana = 5;
            item.knockBack = 4f;
            item.width = 40;
            item.height = 40;
            item.useTime = 45;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.value = Item.buyPrice(0, 30, 0, 0);
            item.rare = ItemRarityID.Green;
            item.autoReuse = true; // for convenience's sake
            // These below are needed for a minion weapon
            item.summon = true;
            item.noMelee = true;
            item.shoot = ProjectileType<ShamanCaneProjectile>();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 vectorToTarget = Main.MouseWorld - player.Center;
            vectorToTarget.Normalize();
            vectorToTarget *= 5;
            speedX = vectorToTarget.X;
            speedY = vectorToTarget.Y;
            return true;
        }
    }
}
