using Terraria;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;

namespace DemoMod.Items.Weapons
{
    class ShepherdStaffBuff: ModBuff
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
                        other.damage -= ShepherdStaff.MinionDamageBonus;
                    }
                }
            }
        }
    }

    class ShepherdStaff: ModItem
    {
        public static int MinionDamageBonus = 5;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
            ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
            DisplayName.SetDefault("Shepherd's Cane");
        }

        public override void SetDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
            ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
            item.damage = 10;
            item.knockBack = 10f;
            item.width = 32;
            item.height = 32;
            item.useTime = 30;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.value = Item.buyPrice(0, 30, 0, 0);
            item.rare = ItemRarityID.White;
            item.autoReuse = true; // for convenience's sake
            // These below are needed for a minion weapon
            item.summon = true;
        }

        private void ApplyMinionDamageBonus()
        {
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (other.active && other.owner == Main.myPlayer && other.minion )
				{
                    other.damage += MinionDamageBonus;
				}
			}
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            // bad hack to workaround not knowing how to use ModPlayer
            // only apply the buff's effect before they have it,
            // then let the buff's effect remove it when it despawns
            if(!player.HasBuff(BuffType<ShepherdStaffBuff>()))
            {
                ApplyMinionDamageBonus();
            }
            player.AddBuff(BuffType<ShepherdStaffBuff>(), 5 * 60);
        }
    }
}
