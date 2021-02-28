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
	public class WoodenWaypointRod : WaypointRod
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Wooden Rod of Minion Guidance");
			Tooltip.SetDefault(
				"Click to place a minion guidance waypoint!\n" +
				"Minions will automatically navigate to the waypoint\n" +
				"Attacking with a non-summon weapon dispels the waypoint\n"+
				"[c/32CD32:20 tile range.]");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.useTime = 60;
			item.useAnimation = 60;
			placementRange = 16 * 20;
		}
	}

	public class GraniteWaypointRod : WaypointRod
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Granite Rod of Minion Guidance");
			Tooltip.SetDefault(
				"Click to place a minion guidance waypoint!\n" +
				"Minions will automatically navigate to the waypoint\n" +
				"Attacking with a non-summon weapon dispels the waypoint\n"+
				"[c/32CD32:30 tile range.]");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.rare = ItemRarityID.Blue;
			item.useTime = 45;
			item.useAnimation = 45;
			placementRange = 16 * 30;
		}
	}

	public class BoneWaypointRod : WaypointRod
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Skeletal Rod of Minion Guidance");
			Tooltip.SetDefault(
				"Click to place a minion guidance waypoint!\n" +
				"Minions will automatically navigate to the waypoint\n" +
				"Attacking with a non-summon weapon dispels the waypoint\n"+
				"[c/32CD32:40 tile range.]");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.rare = ItemRarityID.Green;
			item.useTime = 40;
			item.useAnimation = 40;
			placementRange = 16 * 40;
		}
	}
	public class CrystalWaypointRod : WaypointRod
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crystal Rod of Minion Guidance");
			Tooltip.SetDefault(
				"Click to place a minion guidance waypoint!\n" +
				"Minions will automatically navigate to the waypoint\n" +
				"Attacking with a non-summon weapon dispels the waypoint\n"+
				"[c/32CD32:50 tile range.]");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.rare = ItemRarityID.LightRed;
			item.useTime = 35;
			item.useAnimation = 35;
			placementRange = 16 * 50;
		}
	}

	public class HallowedWaypointRod : WaypointRod
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hallowed Rod of Minion Guidance");
			Tooltip.SetDefault(
				"Click to place a minion guidance waypoint!\n" +
				"Minions will automatically navigate to the waypoint\n" +
				"Attacking with a non-summon weapon dispels the waypoint\n"+
				"[c/32CD32:65 tile range.]");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.rare = ItemRarityID.Pink;
			item.useTime = 25;
			item.useAnimation = 25;
			placementRange = 16 * 65;
		}
	}

}
