using AmuletOfManyMinions.Core.Minions.Pathfinding;
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

namespace AmuletOfManyMinions.Items.WaypointRods
{
	public abstract class WaypointRod : ModItem
	{
		internal short placementRange;

		public override void SetStaticDefaults()
		{
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
		}

		public override void SetDefaults()
		{
			item.useTime = 60;
			item.useAnimation = 36;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.UseSound = SoundID.Item44;
			// These below are needed for a minion weapon
			item.noMelee = true;
			item.summon = true;
			item.autoReuse = true;
		}

		public override bool UseItem(Player player)
		{
			// this needs to be synced across players for turning on/off pathfinding AI
			player.GetModPlayer<MinionPathfindingPlayer>().waypointPlacementRange = placementRange;
			if(Main.myPlayer == player.whoAmI)
			{
				player.GetModPlayer<MinionPathfindingPlayer>().ToggleWaypoint();
			}
			return true;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2 && Main.myPlayer == player.whoAmI)
			{
				player.GetModPlayer<MinionPathfindingPlayer>().ToggleWaypoint(remove: true);
			}
			return base.CanUseItem(player);
		}
	}
}
