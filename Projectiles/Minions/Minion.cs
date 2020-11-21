using AmuletOfManyMinions.Dusts;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions
{
	public static class Vector2Extensions
	{
		// prevent 
		public static void SafeNormalize(this ref Vector2 vec)
		{
			if (vec != Vector2.Zero)
			{
				vec.Normalize();
			}
		}
	}
	public abstract class Minion<T> : ModProjectile where T : ModBuff
	{
		public readonly float PI = (float)Math.PI;

		public Player player;

		protected int? targetNPCIndex;

		protected bool useBeacon = true;

		public bool Spawned { get; private set; }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
		}
		public override void AI()
		{
			player = Main.player[projectile.owner];
			CheckActive();
			if (!Spawned)
			{
				Spawned = true;
				OnSpawn();
			}
			Behavior();
		}

		public virtual void OnSpawn()
		{

		}

		public virtual void CheckActive()
		{
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active)
			{
				player.ClearBuff(BuffType<T>());
			}
			if (player.HasBuff(BuffType<T>()))
			{
				projectile.timeLeft = 2;
			}
		}

		protected Tile TileAtLocation(Vector2 position)
		{
			int x = (int)position.X / 16;
			int y = (int)position.Y / 16;
			//null-safe
			return Framing.GetTileSafely(x, y);
		}
		protected bool InTheGround(Vector2 position)
		{
			Tile tile = TileAtLocation(position);
			return tile.collisionType == 1 || Main.tileSolidTop[tile.type];
		}

		protected bool StandingOnPlatform()
		{
			if (projectile.velocity.Y < 0)
			{
				return false; // can't be standing if we're ascending
			}
			Vector2 bottomOfProjectile = projectile.Bottom;
			bottomOfProjectile.Y += 8; // go to the next block down
			Tile tileUnderfoot = TileAtLocation(bottomOfProjectile);
			return Main.tileSolidTop[tileUnderfoot.type];
		}

		// This may or may not be less efficient than the regular Collisions methods
		// time will tell
		protected Vector2? NearestGroundLocation(Vector2? searchStart = null, int maxSearchDistance = 320)
		{
			for(int i = 8; i < maxSearchDistance; i+= 16)
			{
				Vector2 searchPoint = searchStart ?? projectile.Bottom;
				searchPoint.Y += i;
				if(InTheGround(searchPoint))
				{
					return searchPoint;
				}
			}
			return null;
		}

		protected Vector2? NearestLedgeLocation(Vector2 searchDirection, Vector2? searchStart = null, int maxSearchDistance = 48)
		{
			for(int i = 8; i < maxSearchDistance; i+= 16)
			{
				Vector2 searchPoint = searchStart ?? projectile.Top;
				searchPoint.Y -= i;
				Vector2 nextPointOver = searchPoint;
				nextPointOver.X += 16 * Math.Sign(searchDirection.X);
				if(!InTheGround(searchPoint) && !InTheGround(nextPointOver))
				{
					return searchPoint;
				}
			}
			return null;
		}

		protected bool DropThroughPlatform()
		{
			if (StandingOnPlatform())
			{
				projectile.position.Y += 8;
				return true;
			}
			return false;
		}

		public Vector2? PlayerTargetPosition(float maxRange, Vector2? centeredOn = null, float noLOSRange = 0, Vector2? losCenter = null)
		{
			Vector2 center = centeredOn ?? projectile.Center;
			Vector2 losCenterVector = losCenter ?? projectile.Center;
			if (player.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				float distance = Vector2.Distance(npc.Center, center);
				if (distance < noLOSRange || (distance < maxRange &&
					Collision.CanHitLine(losCenterVector, 1, 1, npc.position, npc.width, npc.height)))
				{
					targetNPCIndex = player.MinionAttackTargetNPC;
					return npc.Center;
				}
			}
			return null;
		}

		public Vector2? PlayerAnyTargetPosition(float maxRange, Vector2? centeredOn = null)
		{
			Vector2 center = centeredOn ?? projectile.Center;
			if (player.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				float distance = Vector2.Distance(npc.Center, center);
				bool lineOfSight = Collision.CanHitLine(center, 1, 1, npc.Center, 1, 1);
				if (distance < maxRange && lineOfSight)
				{
					targetNPCIndex = player.MinionAttackTargetNPC;
					return npc.Center;
				}
			}
			return null;
		}

		public Vector2? ClosestEnemyInRange(float maxRange, Vector2? centeredOn = null, float noLOSRange = 0, bool maxRangeFromPlayer = true, Vector2? losCenter = null)
		{
			Vector2 center = centeredOn ?? projectile.Center;
			Vector2 targetCenter = projectile.position;
			Vector2 losCenterVector = losCenter ?? projectile.Center;
			bool foundTarget = false;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.CanBeChasedBy())
				{
					continue;
				}
				float between = Vector2.Distance(npc.Center, center);
				bool closest = Vector2.Distance(center, targetCenter) > between;
				// don't let a minion infinitely chain attacks off progressively further enemies
				bool inRange = Vector2.Distance(npc.Center, maxRangeFromPlayer ? player.Center : projectile.Center) < maxRange;
				bool inNoLOSRange = Vector2.Distance(npc.Center, player.Center) < noLOSRange;
				bool lineOfSight = inNoLOSRange || Collision.CanHitLine(losCenterVector, 1, 1, npc.position, npc.width, npc.height);
				if ((inNoLOSRange || (lineOfSight && inRange)) && (closest || !foundTarget))
				{
					targetNPCIndex = i;
					targetCenter = npc.Center;
					foundTarget = true;
				}
			}
			if (foundTarget)
			{
				return targetCenter;
			}
			else if (useBeacon)
			{
				return BeaconPosition(center, maxRange, noLOSRange);
			}
			else
			{
				return null;
			}
		}

		public Vector2? BeaconPosition(Vector2 center, float maxRange, float noLOSRange = 0)
		{
			int type = MinionWaypoint.Type;
			// should automatically fall through to here if can't hit target
			if (player.ownedProjectileCounts[type] == 0)
			{
				return null;
			}
			Vector2? waypointCenter = null;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == Main.myPlayer && p.type == type)
				{
					Vector2 target = p.position;
					float distance = Vector2.Distance(target, center);
					if (distance < noLOSRange || (distance < maxRange &&
						Collision.CanHitLine(projectile.Center, 1, 1, target, 1, 1)))
					{
						waypointCenter = target;
						break;
					}
				}
			}
			// try again with the beacon position as the central search point
			if (waypointCenter is Vector2 wCenter && AnyEnemyInRange(maxRange, wCenter) is Vector2 anyTarget)
			{
				DrawDirectionDust(wCenter, anyTarget);
				return wCenter;
			}
			else
			{
				return null;
			}
		}

		private int directionFrame = 0;
		protected void DrawDirectionDust(Vector2 waypointCenter, Vector2 anyTarget)
		{
			if ((directionFrame++) % 30 != 0)
			{
				return;
			}
			int lineLength = 64;
			Vector2 fromVector = projectile.Center - waypointCenter;
			Vector2 toVector = anyTarget - waypointCenter;
			fromVector.SafeNormalize();
			toVector.SafeNormalize();
			for (int i = 12; i < lineLength; i += 2)
			{
				float scale = 1.5f - 0.015f * i;
				Dust.NewDust(waypointCenter + fromVector * i, 1, 1, DustType<MinionWaypointDust>(), newColor: new Color(0.5f, 1, 0.5f), Scale: scale);
				Dust.NewDust(waypointCenter + toVector * i, 1, 1, DustType<MinionWaypointDust>(), newColor: new Color(0.5f, 1, 0.5f), Scale: scale);

			}

		}

		public Vector2? AnyEnemyInRange(float maxRange, Vector2? centeredOn = null)
		{
			Vector2 center = centeredOn ?? projectile.Center;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.CanBeChasedBy())
				{
					continue;
				}
				// 
				bool inRange = Vector2.Distance(center, npc.Center) < maxRange;
				bool lineOfSight = Collision.CanHitLine(center, 1, 1, npc.Center, 1, 1);
				if (lineOfSight && inRange)
				{
					return npc.Center;
				}
			}
			return null;
		}

		public abstract void Behavior();
	}
}