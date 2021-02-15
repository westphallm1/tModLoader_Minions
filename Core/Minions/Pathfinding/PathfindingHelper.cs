using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Core.Minions.Pathfinding
{
	internal class WaypointSearchNode: IComparable<WaypointSearchNode>
	{
		internal Vector2 position;
		internal float distanceHeuristic;
		internal WaypointSearchNode parent;
		internal static int BACKTRACK_DISTANCE_THRESHOLD = 48 * 48;
		internal static int positionGridSize = 4;
		internal bool hasLOS;

		public WaypointSearchNode(Vector2 position, float distanceHeuristic, WaypointSearchNode parent)
		{
			this.position = new Vector2(position.X - position.X % positionGridSize, position.Y - position.Y % positionGridSize);
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
	}

	public class PathfindingHelper
	{
		private Player player;

		internal static List<NPC> npcsInBeaconRange;

		internal SortedSet<WaypointSearchNode> searchNodes = new SortedSet<WaypointSearchNode>();
		internal SortedSet<WaypointSearchNode> visited = new SortedSet<WaypointSearchNode>();

		internal Vector2 waypointPosition;
		internal Vector2 lastWaypointPosition = default;
		internal Vector2 startingPosition = default;
		internal float maxDistance = 0;

		// Parameters for the algorithm

		internal static int DISTANCE_STEP = 32; // 2 blocks
		internal static int ANGLE_STEP = 16; // 16 rotations per
		internal static float EVALUATIONS_PER_FRAME = ANGLE_STEP * 3; // evaluate 4 cycles total per frame
		internal static int MAX_PENDING_QUEUE_SIZE = 32; // discard any nodes that fall below the current best
		internal static int WAYPOINT_PROXIMITY_THRESHOLD = 32;
		internal static int LOS_CHECK_THRESHOLD = 64;
		internal static int MAX_ITERATIONS = 60;
		internal static int MAX_NO_IMPROVEMENT_FRAMES = 10;
		internal int evaluationsThisFrame = 0;
		internal bool searchFailed = false;
		internal bool searchSucceeded = false;
		internal bool pathPruned = false;
		internal int iterations = 0;
		internal int noImprovementFrames = 0;
		internal float pathLength;
		internal List<Vector2> orderedPath;

		private static Vector2[] OffsetVectors;

		public PathfindingHelper(Player player)
		{
			this.player = player;
		}

		public static void Initialize()
		{
			OffsetVectors = new Vector2[ANGLE_STEP];
			for(int i = 0; i < ANGLE_STEP; i++)
			{
				float angle = i * MathHelper.TwoPi / ANGLE_STEP;
				OffsetVectors[i] = angle.ToRotationVector2() * DISTANCE_STEP;
			}
		}

		private WaypointSearchNode AddNode(WaypointSearchNode parent, int angleIdx)
		{
			Vector2 newPosition;
			if(parent == null)
			{
				newPosition = player.Center;
			} else
			{
				Vector2 center = parent.position;
				newPosition = center + OffsetVectors[angleIdx];
				if(!Collision.CanHitLine(center, 1, 1, newPosition, 1, 1))
				{
					return null;
				}
			}
			float distanceHeuristic = Vector2.DistanceSquared(waypointPosition, newPosition);
			bool hasLOS = false;
			if(distanceHeuristic < LOS_CHECK_THRESHOLD * LOS_CHECK_THRESHOLD
				&& Collision.CanHitLine(waypointPosition, 1, 1, newPosition, 1, 1))
			{
				hasLOS = true;
			} else 
			{
				// while we're not close to the waypoint, try to get away from the starting point
				distanceHeuristic -= Math.Min(maxDistance, Vector2.DistanceSquared(startingPosition, newPosition));

			}
			WaypointSearchNode newNode = new WaypointSearchNode(newPosition, distanceHeuristic, parent)
			{
				hasLOS = hasLOS
			};
			if (parent == null || (!parent.IsBacktracking(newNode) && !visited.Contains(newNode)))
			{
				return newNode;
			} else
			{
				return null;
			}
		}

		public Vector2? NextBeaconPosition()
		{
			int type = MinionWaypoint.Type;
			if(player.ownedProjectileCounts[type] == 0)
			{
				ResetState();
				return null;
			}
			waypointPosition = WaypointPos();
			if(waypointPosition != lastWaypointPosition || searchNodes.Count == 0)
			{
				// reset state if the waypoint moved
				ResetState();
				lastWaypointPosition = waypointPosition;
			}
			maxDistance = Vector2.DistanceSquared(startingPosition, waypointPosition);
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
				return currentBest.position;
			}
			evaluationsThisFrame = 0;
			while(evaluationsThisFrame < EVALUATIONS_PER_FRAME)
			{
				currentBest = searchNodes.Min;
				for(int angleIdx = 0; angleIdx < ANGLE_STEP; angleIdx++)
				{
					if (AddNode(currentBest, angleIdx) is WaypointSearchNode newNode)
					{
						if (newNode.hasLOS)
						{
							// terminate the algorithm here, we've found a direct path
							searchNodes.Clear();
							searchNodes.Add(newNode);
							return newNode.position;
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
			}
			if(visited.Min != null && currentBest.distanceHeuristic > visited.Min.distanceHeuristic)
			{
				noImprovementFrames++;
			} else
			{
				noImprovementFrames = 0;
			}
			if(iterations++ > MAX_ITERATIONS || noImprovementFrames > MAX_NO_IMPROVEMENT_FRAMES)
			{
				searchFailed = true;
				searchNodes.Clear();
				// take the closest guess we got
				searchNodes.Add(visited.Min);
			}
			searchNodes.Remove(currentBest);
			visited.Add(currentBest);
			return null;
		}

		public void ResetState()
		{
			lastWaypointPosition = default;
			startingPosition = player.Center;
			searchNodes = new SortedSet<WaypointSearchNode>();
			visited = new SortedSet<WaypointSearchNode>();
			searchFailed = false;
			searchSucceeded = false;
			pathPruned = false;
			iterations = 0;
			noImprovementFrames = 0;
			orderedPath = new List<Vector2>();
			searchNodes.Add(AddNode(null, 0));
		}

		// reduce the path to the minimum number of nodes with LOS to each other
		public void CleanupPath()
		{
			if(pathPruned)
			{
				return;
			}
			pathPruned = true;
			WaypointSearchNode currNode = searchNodes.Min;
			pathLength = 0;
			while(currNode != null)
			{
				while(currNode.parent?.parent is WaypointSearchNode nextParent &&  Collision.CanHitLine(currNode.position, 1, 1, nextParent.position, 1, 1))
				{
					currNode.parent = nextParent;
				} 
				if(currNode.parent != null)
				{
					pathLength += Vector2.Distance(currNode.position, currNode.parent.position);
				}
				orderedPath.Add(currNode.position);
				currNode = currNode.parent;
			}
			orderedPath.Reverse();
			if(searchSucceeded)
			{
				pathLength += Vector2.Distance(orderedPath.Last(), waypointPosition);
				orderedPath.Add(waypointPosition);
			}
		}

		public void DrawPath()
		{
			int pathAnimationLength = 60;
			float desiredDistance = (Main.GameUpdateCount % pathAnimationLength) * pathLength / pathAnimationLength;
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
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && p.type == MinionWaypoint.Type)
				{
					return p.Center;
				}
			}
			// this should never get hit
			return default;
		}

	}
}
