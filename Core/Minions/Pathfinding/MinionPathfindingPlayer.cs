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
	internal class PathfinderMetadata
	{
		internal BlockAwarePathfinder pHelper = default;
		internal Vector2 WaypointPosition = default;
		internal List<Projectile> MinionsAtWaypoint = new List<Projectile>();

		internal PathfinderMetadata(MinionPathfindingPlayer player, int tacticsGroup)
		{
			pHelper = new BlockAwarePathfinder(player, tacticsGroup);
		}
	}
	internal class MinionPathfindingPlayer : ModPlayer
	{
		public static Color[] WaypointColors = new Color[] { Color.Lime, Color.RoyalBlue, Color.Crimson };

		// dependency on Tactics player is unfortunate
		internal MinionTacticsPlayer myTacticsPlayer;
		internal int CurrentTacticsGroup => myTacticsPlayer.CurrentTacticGroup;


		internal PathfinderMetadata[] pathfinderMetas = new PathfinderMetadata[MinionTacticsPlayer.TACTICS_GROUPS_COUNT];
		internal int WaypointPlacementRange = 0;
		internal int PassivePathfindingRange = 0;
		// deal a default of 35% less damage while a minion is the maximum waypoint distance away from the player
		internal float WaypointDamageFalloff = 0.25f;
		// distance for minions to count as "at the waypoint"
		internal static int WAYPOINT_PROXIMITY_THRESHOLD = 64;

		internal BlockAwarePathfinder GetPathfinder(int idx) => pathfinderMetas[idx].pHelper;

		internal BlockAwarePathfinder GetPathfinder(Minion minion) => pathfinderMetas[myTacticsPlayer.GetGroupForMinion(minion)].pHelper;

		internal List<Projectile> GetMinionsAtWaypoint(Minion minion) => pathfinderMetas[myTacticsPlayer.GetGroupForMinion(minion)].MinionsAtWaypoint;

		internal Vector2 GetWaypointPosition(int tacticsGroup) => pathfinderMetas[tacticsGroup].WaypointPosition;

		internal Vector2 GetWaypointPosition(Minion minion) => pathfinderMetas[myTacticsPlayer.GetGroupForMinion(minion)].WaypointPosition;

		public override void ResetEffects()
		{
			PassivePathfindingRange = 0;
		}

		public override void PreUpdate()
		{
			// can't find a better hook to initialize the pathfinding helper
			if(myTacticsPlayer is null)
			{
				myTacticsPlayer = player.GetModPlayer<MinionTacticsPlayer>();
			}
			for(int i = 0; i < MinionTacticsPlayer.TACTICS_GROUPS_COUNT; i++)
			{
				if(pathfinderMetas[i] == null)
				{
					pathfinderMetas[i] = new PathfinderMetadata(this, i);
				}
			}
			FindWaypointPos();
			BuildMinionsAtWaypointList();
		}

		private void BuildMinionsAtWaypointList()
		{
			for(int j = 0; j < MinionTacticsPlayer.TACTICS_GROUPS_COUNT; j++)
			{
				PathfinderMetadata meta = pathfinderMetas[j];
				meta.MinionsAtWaypoint.Clear();
				if(meta.WaypointPosition == default)
				{
					return;
				}
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if (p.active && p.owner == player.whoAmI && (p.minion || ProjectileID.Sets.MinionShot[p.type])
						&& Vector2.DistanceSquared(meta.WaypointPosition, p.Center) < WAYPOINT_PROXIMITY_THRESHOLD * WAYPOINT_PROXIMITY_THRESHOLD)
					{
						meta.MinionsAtWaypoint.Add(p);
					}
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
				for(int i = 0; i < MinionTacticsPlayer.TACTICS_GROUPS_COUNT; i++)
				{
					pathfinderMetas[i].pHelper?.Update();
				}
			}
			if(player.ownedProjectileCounts[MinionWaypoint.Type] > 0)
			{
				player.MinionAttackTargetNPC = -1;
			}
		}

		private void FindWaypointPos()
		{
			for(int i= 0; i < MinionTacticsPlayer.TACTICS_GROUPS_COUNT; i++)
			{
				pathfinderMetas[i].WaypointPosition = default;
			}
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && p.type == MinionWaypoint.Type
					&& InWaypointRange(p.Center))
				{
					pathfinderMetas[(int)p.ai[0]].WaypointPosition = p.Center;
				}
			}
		}

		internal bool InWaypointRange(Vector2 position)
		{
			return Vector2.Distance(position, player.Center) < 1.25f * WaypointPlacementRange;
		}

		internal void UpdateWaypointFromPacket(short xOffset, short yOffset, int tacticsGroup)
		{
			Vector2 newPos = player.Center + new Vector2(xOffset, yOffset);
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && p.type == MinionWaypoint.Type && (int)p.ai[0] == tacticsGroup)
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
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && p.type == type && (int)p.ai[0] == CurrentTacticsGroup)
				{
					if(remove)
					{
						Main.NewText("Kill! " + remove);
						p.Kill();
					} else
					{
						p.position = waypointPosition;
						if(player.whoAmI == Main.myPlayer)
						{
							new WaypointMovementPacket(player, waypointPosition, (byte)CurrentTacticsGroup).Send();
						}
					}
					return;
				}
			}
			// short circuited if existing waypoint is found
			if (!remove)
			{
				Main.NewText("Create! " + remove);
				// uses AI[0] to indicate tactic group
				Projectile.NewProjectile(waypointPosition, Vector2.Zero, type, 0, 0, player.whoAmI, ai0: CurrentTacticsGroup);
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
			if(player.pathfinderMetas.All(m=>m.WaypointPosition == default))
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
