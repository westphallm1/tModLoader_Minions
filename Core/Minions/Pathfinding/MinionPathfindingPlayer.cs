using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.Pathfinding
{
	internal class MinionPathfindingPlayer : ModPlayer
	{
		internal BlockAwarePathfinder pHelper;
		internal int waypointPlacementRange = 0;
		internal int passivePathfindingRange = 0;
		internal Vector2 waypointPosition => pHelper.WaypointPos();
		public override void OnEnterWorld(Player player)
		{
			base.OnEnterWorld(player);
			pHelper = new BlockAwarePathfinder(this);
		}

		public override void ResetEffects()
		{
			passivePathfindingRange = 0;
		}

		public override void PostUpdate()
		{
			pHelper?.Update();
			if(player.ownedProjectileCounts[MinionWaypoint.Type] > 0)
			{
				player.MinionAttackTargetNPC = -1;
			}
		}

		internal bool InWaypointRange(Vector2 position)
		{
			return Vector2.Distance(position, player.Center) < 1.25f * waypointPlacementRange;
		}

		private Vector2 GetNewWaypointPosition()
		{
			Vector2 offset = Main.MouseWorld - player.Center;
			if(offset.Length() < waypointPlacementRange)
			{
				return Main.MouseWorld;
			}
			offset.Normalize();
			offset *= waypointPlacementRange;
			return player.Center + offset;
		}

		internal void ToggleWaypoint(bool remove = false)
		{
			int type = MinionWaypoint.Type;
			Vector2 waypointPosition = GetNewWaypointPosition();
			if (player.ownedProjectileCounts[type] == 0 && !remove)
			{
				Projectile.NewProjectile(waypointPosition, Vector2.Zero, type, 0, 0, player.whoAmI);
			}
			else
			{
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if (p.active && p.owner == player.whoAmI && p.type == type)
					{
						if(remove)
						{
							p.Kill();
						} else
						{
							// todo multiplayer sync
							p.position = waypointPosition;
						}
					}
				}
			}
		}
	}

	internal class WaypointDispellingModItem: GlobalItem
	{
		public override bool UseItem(Item item, Player player)
		{
			if(item.damage > 0 && !item.summon && item.pick == 0 && item.axe == 0 && item.hammer == 0)
			{
				player.GetModPlayer<MinionPathfindingPlayer>().ToggleWaypoint(remove: true);
			}
			return base.UseItem(item, player);
		}
	}
}
