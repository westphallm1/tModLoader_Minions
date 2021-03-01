using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Items.Materials;
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

namespace AmuletOfManyMinions.Items.WaypointRods
{
	public abstract class WaypointRod : ModItem
	{
		internal int placementRange;
		internal abstract int placementRangeInBlocks { get; }

		public override void SetStaticDefaults()
		{
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
			Tooltip.SetDefault(
				"Tool\n"+
				"Click to place a minion guidance waypoint!\n" +
				"Minions will automatically navigate to the waypoint\n" +
				"Attacking with a non-summon weapon dispels the waypoint\n"+
				"Minions deal less damage while attacking a far away waypoint\n");
		}

		public override void SetDefaults()
		{
			item.useTime = 60;
			item.useAnimation = 60;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.UseSound = SoundID.Item44;
			// These below are needed for a minion weapon
			item.noMelee = true;
			item.summon = true;
			item.autoReuse = true;
			placementRange = 16 * placementRangeInBlocks;
		}

		public override bool UseItem(Player player)
		{
			// it seems that it's possible to useItem without calling SetDefaults somehow, so check here as well
			if(placementRange == 0)
			{
				placementRange = 16 * placementRangeInBlocks;
			}
			// this needs to be synced across players for turning on/off pathfinding AI
			player.GetModPlayer<MinionPathfindingPlayer>().WaypointPlacementRange = placementRange;
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

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(mod, "MinionWaypointRange", placementRangeInBlocks + " block range.")
			{
				overrideColor = Color.LimeGreen
			});
		}
	}
	public class WoodenWaypointRod : WaypointRod
	{
		internal override int placementRangeInBlocks => 18;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Wooden Rod of Minion Guidance");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.useTime = 60;
			item.useAnimation = 60;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Wood, 15);
			recipe.AddIngredient(ItemID.Sunflower, 2);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class GraniteWaypointRod : WaypointRod
	{
		internal override int placementRangeInBlocks => 24;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Granite Rod of Minion Guidance");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.rare = ItemRarityID.Blue;
			item.useTime = 45;
			item.useAnimation = 45;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<GraniteSpark>(), 12);
			recipe.AddIngredient(ItemID.Granite, 30);
			// crafted by hand
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class BoneWaypointRod : WaypointRod
	{
		internal override int placementRangeInBlocks => 32;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Skeletal Rod of Minion Guidance");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.rare = ItemRarityID.Green;
			item.useTime = 40;
			item.useAnimation = 40;
		}
	}
	public class CrystalWaypointRod : WaypointRod
	{
		internal override int placementRangeInBlocks => 40;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crystal Rod of Minion Guidance");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.rare = ItemRarityID.LightRed;
			item.useTime = 35;
			item.useAnimation = 35;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<GraniteWaypointRod>(), 1);
			recipe.AddIngredient(ItemID.CrystalShard, 15);
			recipe.AddIngredient(ItemID.SoulofLight, 8);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class HallowedWaypointRod : WaypointRod
	{
		internal override int placementRangeInBlocks => 48;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Hallowed Rod of Minion Guidance");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.rare = ItemRarityID.Pink;
			item.useTime = 25;
			item.useAnimation = 25;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.HallowedBar, 12);
			recipe.AddIngredient(ItemID.SoulofSight, 16);
			recipe.AddIngredient(ItemID.SoulofFright, 4);
			recipe.AddIngredient(ItemID.SoulofMight, 4);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class TrueEyeWaypointRod : WaypointRod
	{
		internal override int placementRangeInBlocks => 80;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Cthulhu's Eye of Minion Vision");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.rare = ItemRarityID.Red;
			item.useTime = 18;
			item.useAnimation = 18;
		}
	}
}
