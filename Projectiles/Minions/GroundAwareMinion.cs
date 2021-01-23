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
		int groupAnimationFrame { get; set; }
	}

	public enum GroundAnimationState
	{
		WALKING,
		JUMPING,
		FLYING,
		STANDING
	}

	public struct StuckInfo
	{
		internal bool throughWall;
		internal bool throughCeiling;
		internal bool throughFloor;
		internal bool overCliff;
		internal bool overLedge;
		internal Vector2 ledgeDestination;
		internal Vector2 cliffDestination;
		internal bool isStuck => throughWall || throughCeiling || throughFloor || overCliff || overLedge;

		public override string ToString()
		{
			string val = "";
			if(throughWall)
			{
				val += "Wall, ";
			}
			if(throughCeiling)
			{
				val += "Ceil, ";
			}
			if(throughFloor)
			{
				val += "Floor, ";
			}
			if(overCliff)
			{
				val += "Cliff, ";
			}
			if(overLedge)
			{
				val += "Ledge, ";
			}
			return val;
		}
	}

	public delegate bool TryWithVector(Vector2 vector);
	public delegate void DoWithVector(Vector2 vector);
	public delegate void GetUnstuckDelegate(Vector2 destination, int startFrame, ref bool done);
	public class GroundAwarenessHelper
	{
		internal IGroundAwareMinion self;
		internal bool didHitWall;
		private bool _isFlying;
		internal bool isFlying {
			get => _isFlying;
			set {
				if(self.groupAnimationFrame - lastTransformedFrame > transformRateLimit)
				{
					lastTransformedFrame = self.groupAnimationFrame;
					_isFlying = value;
				}
			}
		}
		internal bool isOnGround;
		internal bool didJustLand;
		internal int offTheGroundFrames;
		internal Vector2 teleportDestination;
		internal int? teleportStartFrame;
		internal TryWithVector ScaleLedge;
		internal TryWithVector CrossCliff;
		internal DoWithVector IdleFlyingMovement;
		internal DoWithVector IdleGroundedMovement;
		internal GetUnstuckDelegate GetUnstuck;
		internal StuckInfo stuckInfo;
		internal int lastTransformedFrame = -1;
		internal int transformRateLimit = 15;
		private int slowFrameCount;

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
		internal bool StandingOnSlope()
		{
			if (projectile.velocity.Y < 0)
			{
				return false; // can't be standing if we're ascending
			}
			Vector2 bottomOfProjectile = projectile.Bottom;
			bottomOfProjectile.Y += 8; // go to the next block down
			Tile tileUnderfoot = TileAtLocation(bottomOfProjectile);
			return tileUnderfoot.leftSlope() || tileUnderfoot.rightSlope();
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
				didJustLand = false;
				return true;
			}
			return false;
		}

		internal StuckInfo GetStuckInfo(Vector2 vectorToIdlePosition)
		{
			stuckInfo = new StuckInfo();
			stuckInfo.throughWall = didHitWall && vectorToIdlePosition.Length() > 32;
			stuckInfo.throughFloor = isOnGround && vectorToIdlePosition.Y > 32 && Math.Abs(vectorToIdlePosition.X) < 32;
			stuckInfo.throughCeiling = isOnGround && vectorToIdlePosition.Y < -64 &&
				Math.Abs(vectorToIdlePosition.X) < 32 && !Collision.CanHit(projectile.Center, 1, 1, projectile.Center + vectorToIdlePosition, 1, 1);
			if(isOnGround && NearestCliffLocation(vectorToIdlePosition, maxSearchDistance: 32, cliffHeight: 128) is Vector2 cliff)
			{
				stuckInfo.overCliff = true;
				stuckInfo.cliffDestination = cliff;
			}
			if(stuckInfo.throughWall && NearestLedgeLocation(vectorToIdlePosition, maxSearchDistance: 128) is Vector2 ledge)
			{
				stuckInfo.overLedge = true;
				stuckInfo.ledgeDestination = ledge;
			}
			return stuckInfo;
		}

		// if we've run up against a block or a ledge, find the nearest clear space in the direction
		// of the target
		private bool FindEmptySpaceNearTarget(Vector2 vectorToTarget, int increment = 16)
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
				teleportStartFrame = self.groupAnimationFrame;
				return true;
			}
			return false;
		}

		// See if there's a solid block with *top* between us and the target, and teleport on
		// top of it if so
		private bool FindEmptySpaceThroughCeiling(Vector2 vectorToTarget, int increment = 16)
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
				teleportStartFrame = self.groupAnimationFrame;
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
			if(stuckInfo.overLedge && ScaleLedge != null)
			{
				ableToNavigate = ScaleLedge(stuckInfo.ledgeDestination - projectile.Bottom);
			} else if(stuckInfo.overCliff && CrossCliff != null)
			{
				ableToNavigate = CrossCliff(stuckInfo.cliffDestination - projectile.Bottom);
			} else if(stuckInfo.throughCeiling)
			{
				ableToNavigate = FindEmptySpaceThroughCeiling(vectorToIdlePosition);
			} else 
			{
				ableToNavigate = FindEmptySpaceNearTarget(vectorToIdlePosition);
			} 
			didHitWall = false;
			isFlying = !ableToNavigate;
			return ableToNavigate;

		}

		internal void DoTileCollide(Vector2 oldVelocity)
		{
			if(oldVelocity.X != 0 && projectile.velocity.X == 0)
			{
				didHitWall = true;
			}
			if(oldVelocity.Y >= 0 && projectile.velocity.Y == 0)
			{
				didJustLand = true;
			}
			else if (oldVelocity.Y == 0.5f && projectile.velocity.Y == 0.5f)
			{
				// extra check for landing on a slope
				didJustLand = true;
			}
		}

		internal GroundAnimationState GetAnimationState()
		{
			if(isFlying)
			{
				return GroundAnimationState.FLYING;
			} else if (!didJustLand)
			{
				return GroundAnimationState.JUMPING;
			} else
			{
				if(Math.Abs(projectile.velocity.X) < 1)
				{
					// standing still
					slowFrameCount++;
				} else
				{
					slowFrameCount = 0;
				}
				return slowFrameCount >= 15 ? GroundAnimationState.STANDING : GroundAnimationState.WALKING;
			}
		}

		internal GroundAnimationState DoGroundAnimation(Dictionary<GroundAnimationState, (int, int?)> frameInfo , Action<int, int?> animate)
		{
			GroundAnimationState state = GetAnimationState();
			projectile.rotation = state == GroundAnimationState.FLYING ? 0.05f * projectile.velocity.X : 0;
			(int, int?) pair = frameInfo[state];
			if(pair.Item2 is int maxFrame)
			{
				animate(pair.Item1, maxFrame);
			} else
			{
				projectile.frame = pair.Item1;
			}
			return state;
		}


		internal void DoJump(Vector2 vectorToTarget, int defaultJumpVelocity = 4, int maxJumpVelocity = 12)
		{
			if(!didJustLand)
			{
				return;
			}
			if (vectorToTarget.Y < -8 * defaultJumpVelocity)
			{
				projectile.velocity.Y = Math.Max(-maxJumpVelocity, vectorToTarget.Y / 8);
			}
			else
			{
				projectile.velocity.Y = -defaultJumpVelocity;
			}
			didJustLand = false;
		}

		internal void ClimbOneBlock()
		{
			projectile.position.Y -= 16;
			projectile.position.X += 4 * Math.Sign(projectile.oldVelocity.X);
			projectile.velocity.X = projectile.oldVelocity.X;
		}

		internal void SetIsOnGround()
		{
			isOnGround = InTheGround(new Vector2(projectile.Bottom.X, projectile.Bottom.Y + 8)) ||
				InTheGround(new Vector2(projectile.Bottom.X, projectile.Bottom.Y + 24));
			if(!didJustLand)
			{
				didJustLand |= StandingOnSlope();
			}
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
			Vector2? theGround = NearestGroundLocation(maxSearchDistance: (int)maxDistanceAboveGround);
			if(vectorToIdle.Length() > targetSearchDistance || 
				(vectorToTarget is null && (vectorToIdle.Y > maxDistanceAboveGround || theGround is null)))
			{
				isFlying = true;
			} else if (!InTheGround(projectile.Bottom) && Math.Abs(vectorToIdle.Y) < maxDistanceAboveGround/2 && theGround != null)
			{
				isFlying = false;
			}
		}

		internal void ApplyGravity(int maxUpwardVelocity = 12)
		{
			projectile.tileCollide = true;
			projectile.velocity.Y += 0.5f ;
			if(projectile.velocity.Y < -maxUpwardVelocity)
			{
				projectile.velocity.Y = -maxUpwardVelocity;
			}
			if (projectile.velocity.Y > 16)
			{
				projectile.velocity.Y = 16;
			}
		}

		internal void DoIdleMovement(Vector2 vectorToIdle, Vector2? vectorToTarget, float targetSearchDistance, float maxDistanceAboveGround)
		{
			SetFlyingState(vectorToIdle, vectorToTarget, vectorToTarget == null ? 400f : targetSearchDistance, maxDistanceAboveGround);
			if(teleportStartFrame != null && GetUnstuck != null)
			{
				bool done = false;
				GetUnstuck(teleportDestination, (int)teleportStartFrame, ref done);
				if(done)
				{
					teleportStartFrame = null;
				}
			} else if(isFlying && IdleFlyingMovement != null)
			{
				IdleFlyingMovement(vectorToIdle);
			} else
			{
				IdleGroundedMovement?.Invoke(vectorToIdle);
			}
		}
	}
}
