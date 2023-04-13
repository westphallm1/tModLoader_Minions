using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Items.Materials;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.UI;
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
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
			/* Tooltip.SetDefault(
				"Tool\n"+
				"Click to place a minion guidance waypoint!\n" +
				"Minions will automatically navigate to the waypoint\n" +
				"Attacking with a non-summon weapon dispels the waypoint\n"+
				"Minions deal less damage while attacking a far away waypoint"); */
		}

		public override void SetDefaults()
		{
			Item.useTime = 60;
			Item.useAnimation = 60;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item44;
			// These below are needed for a minion weapon
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.autoReuse = true;
			placementRange = 16 * placementRangeInBlocks;
		}

		public override bool? UseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				return false;
			}

            // it seems that it's possible to useItem without calling SetDefaults somehow, so check here as well
			if(placementRange == 0)
			{
				placementRange = 16 * placementRangeInBlocks;
			}
			// this needs to be synced across players for turning on/off pathfinding AI
			player.GetModPlayer<MinionPathfindingPlayer>().WaypointPlacementRange = placementRange;
			if(Main.myPlayer == player.whoAmI)
			{
				player.GetModPlayer<MinionPathfindingPlayer>().ToggleWaypoint(player.selectedItem);
			}
			return true;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override void UseAnimation(Player player)
		{
			if (player.altFunctionUse == 2 && Main.myPlayer == player.whoAmI)
			{
				UserInterfaces.buffClickCapture.PlaceTacticSelectRadial(UserInterfaces.MousePositionUI);
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "MinionWaypointRange", placementRangeInBlocks + " block range.")
			{
				OverrideColor = Color.LimeGreen
			});
		}
	}
	public class WoodenWaypointRod : WaypointRod
	{
		internal override int placementRangeInBlocks => 18;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Wooden Rod of Minion Guidance");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.useTime = 60;
			Item.useAnimation = 60;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Wood, 15).AddIngredient(ItemID.Sunflower, 2).AddTile(TileID.WorkBenches).Register();
		}
	}

	public class GraniteWaypointRod : WaypointRod
	{
		internal override int placementRangeInBlocks => 24;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Granite Rod of Minion Guidance");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.rare = ItemRarityID.Blue;
			Item.useTime = 45;
			Item.useAnimation = 45;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemType<GraniteSpark>(), 12).AddIngredient(ItemID.Granite, 30).Register();
		}
	}

	public class BoneWaypointRod : WaypointRod
	{
		internal override int placementRangeInBlocks => 32;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Skeletal Rod of Minion Guidance");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.rare = ItemRarityID.Green;
			Item.useTime = 40;
			Item.useAnimation = 40;
		}
	}
	public class CrystalWaypointRod : WaypointRod
	{
		internal override int placementRangeInBlocks => 40;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Crystal Rod of Minion Guidance");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.rare = ItemRarityID.LightRed;
			Item.useTime = 35;
			Item.useAnimation = 35;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemType<GraniteWaypointRod>(), 1).AddIngredient(ItemID.CrystalShard, 15).AddIngredient(ItemID.SoulofLight, 8).AddTile(TileID.Anvils).Register();
		}
	}

	public class HallowedWaypointRod : WaypointRod
	{
		internal override int placementRangeInBlocks => 48;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Hallowed Rod of Minion Guidance");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.rare = ItemRarityID.Pink;
			Item.useTime = 25;
			Item.useAnimation = 25;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.HallowedBar, 12).AddIngredient(ItemID.SoulofSight, 16).AddIngredient(ItemID.SoulofFright, 4).AddIngredient(ItemID.SoulofMight, 4).AddTile(TileID.MythrilAnvil).Register();
		}
	}

	public class TrueEyeWaypointRod : WaypointRod
	{
		internal override int placementRangeInBlocks => 80;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Cthulhu's Eye of Minion Vision");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.rare = ItemRarityID.Expert;
			Item.useTime = 18;
			Item.useAnimation = 18;
		}
	}

	// GlobalItem to extendWaypointRod functionality to whips,
	// if configured
	public class WhipsWaypointRods : GlobalItem
	{
		public override bool AltFunctionUse(Item item, Player player)
		{
			bool isWhip = ProjectileID.Sets.IsAWhip[item.shoot];
			if(isWhip && ClientConfig.Instance.WhipRightClickTacticsRadial)
			{
				return true;
			} else
			{
				return base.AltFunctionUse(item, player);
			}
		}
		public override bool CanUseItem(Item item, Player player)
		{
			bool isWhip = ProjectileID.Sets.IsAWhip[item.shoot];
			if(player.altFunctionUse == 2 && Main.myPlayer == player.whoAmI && isWhip && ClientConfig.Instance.WhipRightClickTacticsRadial)
			{
				UserInterfaces.buffClickCapture.PlaceTacticSelectRadial(UserInterfaces.MousePositionUI);
				return false;
			} else
			{
				return base.CanUseItem(item, player);
			}
		}
	}
}
