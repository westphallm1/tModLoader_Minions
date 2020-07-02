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
			return true;
		}
    }
}
