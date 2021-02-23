using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using static Terraria.ModLoader.ModContent;

/// <summary>
/// Implements an algorithm for finding a path between two points by 
/// navigating around the parimeter of any groups of blocks between them
/// </summary>
namespace AmuletOfManyMinions.Core.Minions.Pathfinding
{
	static class Vector2Extension
	{
		public static Vector2 rotate90CW(this Vector2 vec)
		{
			return new Vector2(-vec.Y, vec.X);
		}

		public static Vector2 rotate90CCW(this Vector2 vec)
		{
			return new Vector2(vec.Y, -vec.X);
		}
	}
	internal class WaypointSearchNode: IComparable<WaypointSearchNode>
	{
		internal Vector2 position;
		internal float distanceHeuristic;
		internal WaypointSearchNode parent;
		internal static int BACKTRACK_DISTANCE_THRESHOLD = 4 * 4;
		internal static int positionGridSize = 16;
		internal bool hasLOS;
		internal bool inGround;

		// Used only for the ground probes, might make more sense as subclass
		internal bool isGroundProbe = false;
		internal bool isClockwise;
		internal Vector2 parentPos;
		internal Vector2 targetDirection;
		internal Vector2 velocity;

		private int SnapToGrid(float pos)
		{
			return (int)(pos - pos % positionGridSize) + positionGridSize / 2;
		}

		public WaypointSearchNode(Vector2 position, float distanceHeuristic, WaypointSearchNode parent)
		{
			this.position = new Vector2(SnapToGrid(position.X), SnapToGrid(position.Y));
			this.distanceHeuristic = distanceHeuristic;
			this.parent = parent;
		}

		public int CompareTo(WaypointSearchNode other)
		{
			return distanceHeuristic.CompareTo(other.distanceHeuristic);
		}

		public override bool Equals(object obj)
		{
			return obj is WaypointSearchNode node &&
				   position.Equals(node.position);
		}

		public override int GetHashCode()
		{
			return 1206833562 + position.GetHashCode();
		}

		// rough attempt to prevent nodes from bouncing back and forth
		internal bool IsBacktracking(WaypointSearchNode other)
		{
			if(parent == null)
			{
				return false;
			}
			return Vector2.DistanceSquared(parent.position, other.position) < BACKTRACK_DISTANCE_THRESHOLD;
		}

		// Lifted from GroundAwareMinion
		internal static bool TileAtLocation(Vector2 position)
		{
			int x = (int)position.X / 16;
			int y = (int)position.Y / 16;
			//null-safe
			Tile tile = Framing.GetTileSafely(x, y);
			return tile.active() && !tile.actuator() && tile.collisionType == 1;
		}
		public void UpdateGroundProbe()
		{
			// check that there's still a tile blocking progress in the direction of the original goal
			Vector2 clingTarget = isClockwise ? velocity.rotate90CW() : velocity.rotate90CCW();
			bool clingToPresent = TileAtLocation(position + clingTarget);
			bool travelDirectionBlocked = TileAtLocation(position + velocity);
			bool nextTravelDirectionBlocked = TileAtLocation(position + -clingTarget);
			Vector2 cornerOffset = Vector2.Zero;
			if(!clingToPresent)
			{
				//able to round the corner! Yay!
				cornerOffset = velocity - clingTarget;
				velocity = clingTarget;
			} else if (travelDirectionBlocked && nextTravelDirectionBlocked)
			{
				// rotate in the other direction
				velocity = -velocity;
			} else if (travelDirectionBlocked)
			{
				cornerOffset = -velocity - clingTarget;
				velocity = -clingTarget;
			}
			if(travelDirectionBlocked || !clingToPresent)
			{
				Vector2 intendedNode = position + cornerOffset;
				if(TileAtLocation(intendedNode))
				{
					intendedNode = position;
				}
				WaypointSearchNode cornerNode = new WaypointSearchNode(intendedNode, 0, parent)
				{
					isGroundProbe = true
				};
				parent = cornerNode;
			}
			position += velocity;
		}

		public bool GotAroundBarrier(Vector2 waypointPosition)
		{
			bool gotAroundBarrier;
			Vector2 target = targetDirection;
			Vector2 currentWaypointOffset = waypointPosition - position;
			if(target.X == 0 && target.Y > 0)
			{
				gotAroundBarrier = position.X == parentPos.X &&
					position.Y > parentPos.Y && Math.Sign(currentWaypointOffset.Y) == Math.Sign(target.Y);
			} else if (target.X == 0 && target.Y < 0)
			{
				gotAroundBarrier = position.X == parentPos.X &&
					position.Y < parentPos.Y && Math.Sign(currentWaypointOffset.Y) == Math.Sign(target.Y);
			} else if (target.Y == 0 && target.X > 0)
			{
				gotAroundBarrier = position.Y == parentPos.Y &&
					position.X > parentPos.X && Math.Sign(currentWaypointOffset.X) == Math.Sign(target.X);
			} else
			{
				gotAroundBarrier = position.Y == parentPos.Y &&
					position.X < parentPos.X && Math.Sign(currentWaypointOffset.X) == Math.Sign(target.X);
			}
			return gotAroundBarrier;
		}

	}

	public class BlockAwarePathfinder
	{
		private Player player;

		internal static List<NPC> npcsInBeaconRange;

		internal SortedSet<WaypointSearchNode> searchNodes = new SortedSet<WaypointSearchNode>();
		internal SortedSet<WaypointSearchNode> visited = new SortedSet<WaypointSearchNode>();

		internal Vector2 waypointPosition;
		internal Vector2 lastWaypointPosition = default;
		internal Vector2 startingPosition = default;

		// Parameters for the algorithm
		internal static int DISTANCE_STEP = 16; // 1 block
		internal static float EVALUATIONS_PER_FRAME = 120; // In the air evaluations per frame
		internal static int MAX_GROUND_SEARCH = 300; // ground probe evaluations per frame
		internal static int MAX_PENDING_QUEUE_SIZE = 32; // discard any nodes that fall below the current best
		internal static int LOS_CHECK_THRESHOLD = 256;
		internal static int MAX_ITERATIONS = 60;
		internal static int MAX_NO_IMPROVEMENT_FRAMES = 10;

		// parameters for tweaking the start of the path as the player moves
		internal static int PLAYER_MOVEMENT_THRESHOLD = 32;
		internal static int PLAYER_MOVEMENT_RATE_LIMIT = 60;

		// parameters for using an NPC's location as the waypoint
		internal static int NPC_MAX_DISTANCE = 800;
		// avoid situations where an overly convoluted path leads to the nearest enemy
		internal static int MAX_AUTOMATIC_PATH_DISTANCE = 1200;
		internal static int NPC_MOVEMENT_THRESHOLD = 32;


		// internal algorithm state
		internal int evaluationsThisFrame = 0;
		internal bool searchActive = false; // if the algorithm is running
		internal bool searchFailed = false; // if the algorithm terminated unsuccessfully
		internal bool searchSucceeded = false; // if the algorithm terminated successfully
		internal bool pathFinalized = false; // if the path has been cleaned up
		internal int iterations = 0;
		internal int noImprovementFrames = 0;
		internal float pathLength;
		internal int lastPlayerMovementFrame = 0;
		internal List<Vector2> orderedPath;
		internal bool playerPlacedWaypoint = false;

		public BlockAwarePathfinder(Player player)
		{
			this.player = player;
		}

		public static void Initialize()
		{
		}

		private WaypointSearchNode AddNode(WaypointSearchNode parent)
		{
			Vector2 newPosition;
			bool inGround = false;
			if(parent == null)
			{
				newPosition = player.Center;
			} else
			{
				newPosition = parent.position;
				Vector2 distance = waypointPosition - newPosition;
				Vector2 angleOffset;
				if(Math.Abs(distance.X) > Math.Abs(distance.Y))
				{
					angleOffset = new Vector2(DISTANCE_STEP * Math.Sign(distance.X), 0);
				} else
				{
					angleOffset = new Vector2(0, DISTANCE_STEP * Math.Sign(distance.Y));
				}
				newPosition += angleOffset;
				if(WaypointSearchNode.TileAtLocation(newPosition))
				{
					inGround = true;
				}
			}
			float distanceHeuristic = Vector2.DistanceSquared(waypointPosition, newPosition);
			bool hasLOS = false;
			if(distanceHeuristic < LOS_CHECK_THRESHOLD * LOS_CHECK_THRESHOLD
				&& Collision.CanHitLine(waypointPosition, 1, 1, newPosition, 1, 1))
			{
				hasLOS = true;
			} 
			WaypointSearchNode newNode = new WaypointSearchNode(newPosition, distanceHeuristic, parent)
			{
				hasLOS = hasLOS,
				inGround = inGround
			};
			if (parent == null || (!parent.IsBacktracking(newNode) && !visited.Contains(newNode)))
			{
				return newNode;
			} else
			{
				return null;
			}
		}
		

		private void AddGroundProbeNodes(WaypointSearchNode parent)
		{
			visited.Clear();
			searchNodes.Clear();
			WaypointSearchNode grandparent = parent.parent;
			Vector2 targetDirection =  parent.position - grandparent.position;
			WaypointSearchNode probeNodeCW = new WaypointSearchNode(grandparent.position, 0, grandparent)
			{
				parentPos = parent.position,
				isGroundProbe = true,
				targetDirection = targetDirection,
				isClockwise = true,
				velocity = targetDirection.rotate90CCW()
			};

			WaypointSearchNode probeNodeCCW = new WaypointSearchNode(grandparent.position, 1, grandparent)
			{
				parentPos = parent.position,
				isGroundProbe = true,
				targetDirection = targetDirection,
				isClockwise = false,
				velocity = targetDirection.rotate90CW()
			};
			// back up the node before the ground by one block
			grandparent.position -= targetDirection;
			searchNodes.Add(probeNodeCW);
			searchNodes.Add(probeNodeCCW);
		}

		private void HandleGroundProbe()
		{
			for(int i = 0; i < MAX_GROUND_SEARCH; i++)
			{
				foreach(WaypointSearchNode groundProbe in searchNodes)
				{
					groundProbe.UpdateGroundProbe();
					// DebugDust();
					if(groundProbe.GotAroundBarrier(waypointPosition))
					{
						searchNodes.Clear();
						searchNodes.Add(AddNode(groundProbe));
						return;
					}
					float distanceToWaypoint = Vector2.DistanceSquared(groundProbe.position, waypointPosition);
					bool isClose = distanceToWaypoint < LOS_CHECK_THRESHOLD * LOS_CHECK_THRESHOLD;
					bool isReallyClose = distanceToWaypoint < LOS_CHECK_THRESHOLD * LOS_CHECK_THRESHOLD / 4;
					if((isReallyClose || (isClose && i%4 == 0) || i%8 == 0)
						&& Collision.CanHitLine(waypointPosition, 1, 1, groundProbe.position, 1, 1))
					{
						searchNodes.Clear();
						searchNodes.Add(groundProbe);
						groundProbe.hasLOS = true;
						return;
					} 
				}
			}
		}

		private void HandleAirWaypoints()
		{
			WaypointSearchNode currentBest = searchNodes.Min;
			while(evaluationsThisFrame < EVALUATIONS_PER_FRAME)
			{
				currentBest = searchNodes.Min;
				if(currentBest.inGround)
				{
					return;
				} 
				if (AddNode(currentBest) is WaypointSearchNode newNode)
				{
					if (newNode.hasLOS)
					{
						// terminate the algorithm here, we've found a direct path
						searchNodes.Clear();
						searchNodes.Add(newNode);
						return;
					}
					else
					{
						searchNodes.Add(newNode);
					}
				};
				evaluationsThisFrame++;
				if(searchNodes.Count > MAX_PENDING_QUEUE_SIZE)
				{
					searchNodes.Remove(searchNodes.Max);
				}
			}
			if(visited.Min != null && currentBest.distanceHeuristic > visited.Min.distanceHeuristic)
			{
				noImprovementFrames++;
			} else
			{
				noImprovementFrames = 0;
			}
			searchNodes.Remove(currentBest);
			visited.Add(currentBest);

		}

		// Return whether the algorithm is currently in an end state (successful or not)
		internal bool InProgress()
		{
			return searchActive;
		}

		// return whether the algorithm needs to be run
		// and do a bunch of side effects
		internal bool InEndState()
		{
			waypointPosition = WaypointPos();
			// if the waypoint isn't active, return
			if(waypointPosition == default)
			{
				lastWaypointPosition = default;
				return true;
			}
			if(waypointPosition != lastWaypointPosition)
			{
				if (searchSucceeded && lastWaypointPosition != default && 
					Collision.CanHitLine(waypointPosition,1, 1, lastWaypointPosition, 1, 1))
				{
					// if we're able to 'patch' the path by just moving the endpoint
					orderedPath.Add(waypointPosition);
					// lazy copy
					PrunePath(orderedPath);
					lastWaypointPosition = waypointPosition;
					return true;
				} else
				{
					// reset state if the waypoint moved to a point where we can't see it
					ResetState();
					lastWaypointPosition = waypointPosition;
				}
			} else if (searchSucceeded && 
				Main.GameUpdateCount - lastPlayerMovementFrame > PLAYER_MOVEMENT_RATE_LIMIT &&
				Vector2.DistanceSquared(player.Center, orderedPath[0]) > PLAYER_MOVEMENT_THRESHOLD * PLAYER_MOVEMENT_THRESHOLD)
			{
				// check if we can 'patch' the path by replacing the current few starting nodes
				// with the player's new position
				for(int i = 1; i < Math.Min(3, orderedPath.Count); i++)
				{
					if (Collision.CanHitLine(player.Center, 1, 1, orderedPath[1], 1, 1))
					{
						// can just patch the current path
						orderedPath[i-1] = player.Center;
						PrunePath(orderedPath.Skip(i-1).ToList());
						return true;
					}

				}
				// fall through, need to fully recalculate
				ResetState();
			}

			if(searchNodes.Count == 0 || searchNodes.Min is null)
			{
				// this shouldn't happen, error out
				searchFailed = true;
				return true;
			}
			WaypointSearchNode currentBest = searchNodes.Min;
			if(!currentBest.hasLOS)
			{
				// do one full LOS check per iteration, since this can eliminate some cases
				// where the path converges along a weird axis
				currentBest.hasLOS = Collision.CanHitLine(currentBest.position, 1, 1, waypointPosition, 1, 1);
			}
			searchSucceeded |= currentBest.hasLOS;
			if(searchSucceeded || searchFailed)
			{
				CleanupPath();
				DrawPath();
				return true;
			}
			return false;
		}

		private void RunIteration()
		{
			// this places a lot of trust in the algorithm always finishing
			WaypointSearchNode currentBest = searchNodes.Min;
			evaluationsThisFrame = 0;
			if (currentBest.inGround)
			{
				AddGroundProbeNodes(currentBest);
			}
			else if (currentBest.isGroundProbe)
			{
				HandleGroundProbe();
			}
			else
			{
				HandleAirWaypoints();
			}
			if (iterations++ > MAX_ITERATIONS || noImprovementFrames > MAX_NO_IMPROVEMENT_FRAMES)
			{
				searchFailed = true;
				if (!searchNodes.Min.isGroundProbe)
				{
					searchNodes.Clear();
					// take the closest guess we got
					searchNodes.Add(visited.Min);
				}
			}
		}

		public void Update()
		{
			if(InEndState())
			{
				searchActive = false;
				return;
			}
			searchActive = true;
			RunIteration();
		}

		public void ResetState()
		{
			lastWaypointPosition = default;
			startingPosition = player.Center;
			searchNodes = new SortedSet<WaypointSearchNode>();
			visited = new SortedSet<WaypointSearchNode>();
			searchFailed = false;
			searchSucceeded = false;
			pathFinalized = false;
			iterations = 0;
			noImprovementFrames = 0;
			orderedPath = new List<Vector2>();
			searchNodes.Add(AddNode(null));
		}

		// reduce the path to the minimum number of nodes with LOS to each other
		public void CleanupPath()
		{
			if(pathFinalized)
			{
				return;
			}
			pathFinalized = true;
			WaypointSearchNode currNode = searchNodes.Min;
			pathLength = 0;
			List<Vector2> allNodes = new List<Vector2>();
			while(currNode != null)
			{
				allNodes.Add(currNode.position);
				currNode = currNode.parent;
			}
			allNodes.Reverse();
			if(searchSucceeded)
			{
				allNodes.Add(waypointPosition);
				PrunePath(allNodes);
			}
		}
		private void PrunePath(List<Vector2> allNodes)
		{
			pathLength = 0;
			orderedPath = new List<Vector2>();
			// O(n ^ 2) LOS checks, maybe not great
			for(int i = 0; i < allNodes.Count -1; i++)
			{
				orderedPath.Add(allNodes[i]);
				int lastJ = i + 1;
				for(int j = i+1; j < allNodes.Count; j++)
				{
					if (Collision.CanHitLine(allNodes[i], 1, 1, allNodes[j], 1, 1))
					{
						lastJ = j;
					}
				}
				i = lastJ - 1;
			}
			orderedPath.Add(waypointPosition);
			for(int i = 0; i < orderedPath.Count - 1; i++)
			{
				pathLength += Vector2.Distance(orderedPath[i], orderedPath[i+1]);
			}

			// if this path was generated automatically, don't allow it to exceed a certain length
			if(!playerPlacedWaypoint && pathLength > MAX_AUTOMATIC_PATH_DISTANCE)
			{
				// un-succeed the search
				searchSucceeded = false;
				searchFailed = true;
			}
		}



		public void DrawPath()
		{
			int pathAnimationLength = Math.Max(30, (int)pathLength / 10);
			float desiredDistance = (Main.GameUpdateCount % pathAnimationLength) * 10;
			float traversedDistance = 0;
			for(int i = 0; i < orderedPath.Count -1; i++)
			{
				Vector2 nextPathSegment = orderedPath[i + 1] - orderedPath[i];
				float nextDistance = nextPathSegment.Length();
				if(desiredDistance <= traversedDistance + nextDistance)
				{
					float remainingDistance = desiredDistance - traversedDistance;
					nextPathSegment.Normalize();
					nextPathSegment *= remainingDistance;
					Dust.NewDust(orderedPath[i] + nextPathSegment, 1, 1, DustType<MinionWaypointDust>(), 
						newColor: searchSucceeded ? Color.LimeGreen : Color.Red, Scale: 1.2f);
					break;
				}
				traversedDistance += nextDistance;
			}
		}

		public Vector2 WaypointPos()
		{
			// first pass: look for the player-placed waypoint
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && p.type == MinionWaypoint.Type)
				{
					playerPlacedWaypoint = true;
					return p.Center;
				}
			}
			// second pass: look for any enemy
			NPC closestNPC = Main.npc.Where(npc => npc.active && npc.CanBeChasedBy() &&
				Vector2.DistanceSquared(player.Center, npc.Center) < NPC_MAX_DISTANCE * NPC_MAX_DISTANCE).OrderBy(npc =>
				Vector2.DistanceSquared(player.Center, npc.Center)).FirstOrDefault();
			if(closestNPC != default)
			{
				playerPlacedWaypoint = false;
				if(Vector2.DistanceSquared(lastWaypointPosition, closestNPC.Center) < NPC_MOVEMENT_THRESHOLD * NPC_MOVEMENT_THRESHOLD)
				{
					// if the NPC hasn't moved that much, don't recalculate the path
					return lastWaypointPosition;
				} else
				{
					return closestNPC.Center;
				}
			}
			// otherwise
			return default;
		}
		private void DebugDust()
		{
			bool isFirst = true;
			foreach (WaypointSearchNode node in searchNodes)
			{
				Dust.NewDust(node.position, 1, 1, DustType<MinionWaypointDust>(),
						newColor: isFirst ? Color.Red : Color.MediumPurple, Scale: 1.0f);
				isFirst = false;
			}
		}
	}
}
