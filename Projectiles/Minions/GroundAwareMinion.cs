using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions
{
	public interface IGroundAwareMinion
	{
		Projectile projectile { get; }
		int animationFrame { get; }
	}

	public struct StuckInfo
	{
		internal bool throughWall;
		internal bool throughCeiling;
		internal bool throughFloor;
		internal bool overCliff;
		internal bool overLedge;

		internal bool isStuck => throughWall || throughCeiling || throughFloor || overCliff || overLedge;
	}

	public delegate bool TryWithVector(Vector2 vector);
	public class GroundAwarenessHelper
	{
		internal IGroundAwareMinion self;
		internal bool didHitWall;
		internal bool isFlying;
		internal bool isOnGround;
		internal int offTheGroundFrames;
		internal Vector2 teleportDestination;
		internal int? teleportStartFrame;
		internal TryWithVector ScaleLedge;

		private Projectile projectile => self.projectile;
		public GroundAwarenessHelper(IGroundAwareMinion self)
		{
			this.self = self;
		}
		internal Tile TileAtLocation(Vector2 position)
		{
			int x = (int)position.X / 16;
			int y = (int)position.Y / 16;
			//null-safe
			return Framing.GetTileSafely(x, y);
		}
		internal bool InTheGround(Vector2 position)
		{
			Tile tile = TileAtLocation(position);
			return tile.collisionType == 1 || Main.tileSolidTop[tile.type];
		}

		internal bool StandingOnPlatform()
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

		// Find the nearest tile that's directly beneath the minion and directly above the ground eg:
		//
		//     M
		//
		//     X
		// GGGGGGGGGGG
		internal Vector2? NearestGroundLocation(Vector2? searchStart = null, int maxSearchDistance = 320)
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

		// Find the nearest tile that's 'up a ledge' from the minion, eg:
		//
		//
		// X
		// G
		// G M
		// GGGGG
		internal Vector2? NearestLedgeLocation(Vector2 searchDirection, Vector2? searchStart = null, int maxSearchDistance = 48)
		{
			for(int i = 8; i < maxSearchDistance; i+= 16)
			{
				Vector2 searchPoint = searchStart ?? projectile.Top;
				searchPoint.Y -= i;
				Vector2 nextPointOver = searchPoint;
				nextPointOver.X += 16 * Math.Sign(searchDirection.X);
				bool searchPointInGround = InTheGround(searchPoint);
				if(searchPointInGround)
				{
					return null; // the ledge is obstructed, we can't climb it
				} else if(!InTheGround(nextPointOver))
				{
					return searchPoint;
				}
			}
			return null;
		}

		// Find the nearest tile that's 'down a ledge' from the minion, eg:
		//
		//
		// M
		// GGGGGGGG X
		// GGGGGGGG
		// GGGGGGGGGG
		internal Vector2? NearestCliffLocation(Vector2 searchDirection, int cliffHeight = 48, Vector2? searchStart = null, int maxSearchDistance = 48)
		{
			for(int i = 0; i < maxSearchDistance; i+= 16)
			{
				Vector2 searchPoint = searchStart ?? projectile.Bottom;
				searchPoint.Y -= 8; // make sure the block above is also air
				searchPoint.X += i * Math.Sign(searchDirection.X);
				bool isAllAir = true;
				for(int j = 0; j < cliffHeight; j+= 16)
				{
					Vector2 dropPoint = searchPoint + new Vector2(0, j);
					if(InTheGround(dropPoint))
					{
						if (j == 0)
						{
							// there's an obstruction between us and the ledge, so we can't fall
							return null;
						}
						isAllAir = false;
						break;
					}
				}
				if(isAllAir)
				{
					return searchPoint;
				}
			}
			return null;
		}

		internal bool DropThroughPlatform()
		{
			if (StandingOnPlatform())
			{
				projectile.position.Y += 8;
				return true;
			}
			return false;
		}

		internal StuckInfo GetStuckInfo(Vector2 vectorToIdlePosition)
		{
			StuckInfo stuckInfo = new StuckInfo();
			stuckInfo.throughWall = didHitWall && vectorToIdlePosition.Length() > 32;
			stuckInfo.throughFloor = isOnGround && vectorToIdlePosition.Y > 32 && Math.Abs(vectorToIdlePosition.X) < 32;
			stuckInfo.throughCeiling = isOnGround && vectorToIdlePosition.Y < -64 &&
				Math.Abs(vectorToIdlePosition.X) < 32 && !Collision.CanHit(projectile.Center, 1, 1, projectile.Center + vectorToIdlePosition, 1, 1);
			stuckInfo.overCliff = isOnGround && NearestCliffLocation(vectorToIdlePosition, maxSearchDistance: 32, cliffHeight: 128) != null;
			stuckInfo.overLedge = stuckInfo.throughWall && NearestLedgeLocation(vectorToIdlePosition, maxSearchDistance: 128) != null;
			return stuckInfo;
		}

		// if we've run up against a block or a ledge, find the nearest clear space in the direction
		// of the target
		private bool TeleportTowardsTarget(Vector2 vectorToTarget, int increment = 16)
		{
			Vector2 incrementVector = vectorToTarget;
			incrementVector.Normalize();
			float maxLength = vectorToTarget.Length();
			Vector2? clearSpace = null;
			bool hasFoundGround = false;
			for(int i = increment; i < maxLength; i+= increment)
			{
				Vector2 position = projectile.Center + incrementVector * i;
				bool inTheGround = InTheGround(position);
				hasFoundGround |= inTheGround;
				if(hasFoundGround && !inTheGround)
				{
					clearSpace = position;
					break;
				}
			}
			if(clearSpace == null && !InTheGround(projectile.Center + vectorToTarget)) {
				clearSpace = projectile.Center + vectorToTarget;
			}
			if(clearSpace is Vector2 clear && NearestGroundLocation(searchStart: clear) is Vector2 clearGround)
			{
				clearGround.Y -= clearGround.Y % 16; // snap to the nearest block
				clearGround.Y -= projectile.height;
				teleportDestination = clearGround;
				teleportDestination.X += Math.Sign(vectorToTarget.X) * projectile.width / 2;
				teleportStartFrame = self.animationFrame;
				return true;
			}
			return false;
		}

		// See if there's a solid block with *top* between us and the target, and teleport on
		// top of it if so
		private bool TeleportTowardsCeiling(Vector2 vectorToTarget, int increment = 16)
		{
			vectorToTarget.Y -= 16; // give a bit of leeway
			vectorToTarget.X = 0; // only go straight up
			Vector2 incrementVector = vectorToTarget;
			incrementVector.Normalize();
			float maxLength = vectorToTarget.Length();
			Vector2? clearSpace = null;
			bool hasFoundGround = false;
			for(int i = 16; i < maxLength; i+= increment)
			{
				Vector2 position = projectile.Center + incrementVector * i;
				bool inTheGround = InTheGround(position);
				hasFoundGround |= inTheGround;
				if(hasFoundGround && !inTheGround)
				{
					clearSpace = position;
					break;
				}
			}
			if(clearSpace is Vector2 clear && NearestGroundLocation(searchStart: clear) is Vector2 clearGround)
			{
				clearGround.Y -= clearGround.Y % 16; // snap to the nearest block
				clearGround.Y -= (projectile.height);
				teleportDestination = clearGround;
				teleportStartFrame = self.animationFrame;
				return true;
			}
			return false;
		}

		internal bool GetUnstuckByTeleporting(StuckInfo stuckInfo, Vector2 vectorToIdlePosition)
		{
			if(!stuckInfo.isStuck)
			{
				return true;
			}
			bool ableToNavigate = false;
			if(stuckInfo.overLedge && ScaleLedge == null ? false : ScaleLedge(vectorToIdlePosition))
			{
				ableToNavigate = true;
			} else if(stuckInfo.throughCeiling)
			{
				ableToNavigate = TeleportTowardsCeiling(vectorToIdlePosition);
			} else 
			{
				ableToNavigate = TeleportTowardsTarget(vectorToIdlePosition);
			} 
			didHitWall = false;
			isFlying = !ableToNavigate;
			//Main.NewText("F: " + needsToGoThroughFloor + 
			//	" W: " + needsToGoThroughWall + 
			//	" C: " + needsToGoThroughCeiling + 
			//	" L: " + needsToGoOverLedge + 
			//	" S: " + ableToNavigate);
			return ableToNavigate;

		}

		internal void SetDidHitWall(Vector2 oldVelocity)
		{
			if(oldVelocity.X != 0 && projectile.velocity.X == 0)
			{
				didHitWall = true;
			}
		}

		internal void SetIsOnGround()
		{
			isOnGround = InTheGround(new Vector2(projectile.Bottom.X, projectile.Bottom.Y + 8)) ||
				InTheGround(new Vector2(projectile.Bottom.X, projectile.Bottom.Y + 24));
		}

		internal void SetOffTheGroundFrames()
		{
			if(isOnGround)
			{
				offTheGroundFrames = 0;
			} else
			{
				offTheGroundFrames++;
			}
		}

		// determine whether the minion should be flying or not
		internal void SetFlyingState(Vector2 vectorToIdle, Vector2? vectorToTarget, float targetSearchDistance, float maxDistanceAboveGround)
		{
			Vector2? theGround = NearestGroundLocation();
			if(vectorToIdle.Length() > targetSearchDistance || 
				(vectorToTarget is null && (vectorToIdle.Y > maxDistanceAboveGround || theGround is null)))
			{
				isFlying = true;
			} else if (!InTheGround(projectile.Bottom) && Math.Abs(vectorToIdle.Y) < maxDistanceAboveGround/2 && theGround != null)
			{
				isFlying = false;
			}
		}
	}
}
