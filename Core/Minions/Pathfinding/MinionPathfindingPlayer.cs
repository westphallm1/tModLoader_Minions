using AmuletOfManyMinions.Core.Netcode.Packets;
using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.Pathfinding
{
	internal class MinionPathfindingPlayer : ModPlayer
	{
		internal BlockAwarePathfinder pHelper = default;
		internal int WaypointPlacementRange = 0;
		internal int PassivePathfindingRange = 0;
		// deal a default of 35% less damage while a minion is the maximum waypoint distance away from the player
		internal float WaypointDamageFalloff = 0.25f;
		internal Vector2 WaypointPosition = default;
		internal List<Projectile> MinionsAtWaypoint = new List<Projectile>();
		// distance for minions to count as "at the waypoint"
		internal static int WAYPOINT_PROXIMITY_THRESHOLD = 64;


		public override void ResetEffects()
		{
			PassivePathfindingRange = 0;
		}

		public override void PreUpdate()
		{
			// can't find a better hook to initialize the pathfinding helper
			if(pHelper == default)
			{
				pHelper = new BlockAwarePathfinder(this);
			}
			FindWaypointPos();
			BuildMinionsAtWaypointList();
		}

		private void BuildMinionsAtWaypointList()
		{
			MinionsAtWaypoint.Clear();
			if(WaypointPosition == default)
			{
				return;
			}
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && (p.minion || ProjectileID.Sets.MinionShot[p.type])
					&& Vector2.DistanceSquared(WaypointPosition, p.Center) < WAYPOINT_PROXIMITY_THRESHOLD * WAYPOINT_PROXIMITY_THRESHOLD)
				{
					MinionsAtWaypoint.Add(p);
				}
			}

		}

		public override void PostUpdate()
		{

			//Only send to other player if he's in visible range
			Rectangle bounds = Utils.CenteredRectangle(Main.player[Main.myPlayer].Center, new Vector2(1920, 1080) * 1.5f);
			Point myCenter = player.Center.ToPoint();
			bool doUpdate = bounds.Contains(myCenter);
			if(doUpdate)
			{
				pHelper?.Update();
			}
			if(player.ownedProjectileCounts[MinionWaypoint.Type] > 0)
			{
				player.MinionAttackTargetNPC = -1;
			}
		}

		private void FindWaypointPos()
		{
			// get waypoint position
			WaypointPosition = default;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && p.type == MinionWaypoint.Type
					&& InWaypointRange(p.Center))
				{
					WaypointPosition = p.Center;
				}
			}
		}
		internal bool InWaypointRange(Vector2 position)
		{
			return Vector2.Distance(position, player.Center) < 1.25f * WaypointPlacementRange;
		}

		internal void UpdateWaypointFromPacket(short xOffset, short yOffset)
		{
			Vector2 newPos = player.Center + new Vector2(xOffset, yOffset);
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && p.type == MinionWaypoint.Type)
				{
					p.position = newPos;
				}
			}
		}

		private Vector2 GetNewWaypointPosition()
		{
			Vector2 offset = Main.MouseWorld - player.Center;
			if(offset.Length() < WaypointPlacementRange)
			{
				return Main.MouseWorld;
			}
			offset.Normalize();
			offset *= WaypointPlacementRange;
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
							p.position = waypointPosition;
							if(player.whoAmI == Main.myPlayer)
							{
								new WaypointMovementPacket(player, waypointPosition).Send();
							}
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// GlobalProjectile that reduces a projectile's damage based on how far away the pathfinding reticle is from the player
	/// </summary>
	internal class WaypointDamageFalloffProjectile: GlobalProjectile
	{
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			// nerf all minion damage, even if they're not following the waypoint. Maybe a bit iffy
			if(!(projectile.minion ||
				ProjectileID.Sets.MinionShot[projectile.type]))
			{
				return;
			}
			MinionPathfindingPlayer player = Main.player[projectile.owner].GetModPlayer<MinionPathfindingPlayer>();
			// no falloff if waypoint not placed
			if(player.WaypointPosition == default)
			{
				return;
			}
			float maxDist = player.WaypointPlacementRange;
			float damageReduction = player.WaypointDamageFalloff * Math.Min(maxDist, Vector2.Distance(projectile.Center, player.player.Center)) / maxDist;
			damage = (int)((1 - damageReduction) * damage);
		}
	}

	/// <summary>
	/// GlobalItem that dispells a player's waypoint when they attack with a non-summon weapon
	/// </summary>
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
