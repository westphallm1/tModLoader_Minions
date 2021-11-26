using AmuletOfManyMinions.Core.BackportUtils;
using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.UI;
using AmuletOfManyMinions.UI.TacticsUI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions
{
	public abstract class MinionItem<TBuff, TProj> : BackportModItem where TBuff : ModBuff where TProj : Minion
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
		}

		public override void SetDefaults()
		{
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
			player.AddBuff(item.buffType, 3);

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;
			return true;
		}

		public override bool AltFunctionUse(Player player)
		{
			return false;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2 && Main.myPlayer == player.whoAmI)
			{
				return false;
			}
			return base.CanUseItem(player);
		}
	}
}
