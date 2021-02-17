#define DEBUG
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

		// Try to detect 'bad convergences' that don't decrease but also don't lead to the target
		internal Vector2 stuckPosition = default; // the location of the bad convergence
		internal int unstuckStartFrame = default; // the frame we started trying to get unstuck on
		internal int FRAMES_TO_GET_UNSTUCK = 10; // the max number of frames to get unstuck

		internal static int DISTANCE_STEP = 32; // 2 blocks
		internal static int ANGLE_STEP = 16; // 16 rotations per
		internal static float EVALUATIONS_PER_FRAME = ANGLE_STEP * 3; // evaluate 3 cycles total per frame
		internal static int MAX_PENDING_QUEUE_SIZE = 32; // discard any nodes that fall below the current best
		internal static int WAYPOINT_PROXIMITY_THRESHOLD = 32;
		internal static int LOS_CHECK_THRESHOLD = 64;
		internal static int MAX_ITERATIONS = 60;
		internal static int MAX_NO_IMPROVEMENT_FRAMES = 10;
		internal static int PLAYER_DISTANCE_THRESHOLD = 64; // re-calculate if the player moves this much
		internal static int RECALCULATE_RATE_LIMIT = 90; // only re-calculate once per X frames
		internal int evaluationsThisFrame = 0;
		internal bool searchFailed = false;
		internal bool searchSucceeded = false;
		internal bool pathFinalized = false;
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

		private float GetHeuristic(Vector2 newPosition, ref bool hasLOS)
		{
			float distanceHeuristic = Vector2.DistanceSquared(waypointPosition, newPosition);
			if(stuckPosition != default && iterations - unstuckStartFrame < FRAMES_TO_GET_UNSTUCK)
			{
				distanceHeuristic = Vector2.DistanceSquared(stuckPosition, newPosition);
				return distanceHeuristic;
			} 
			if(stuckPosition == default && distanceHeuristic < LOS_CHECK_THRESHOLD * LOS_CHECK_THRESHOLD
				&& Collision.CanHitLine(waypointPosition, 1, 1, newPosition, 1, 1))
			{
				hasLOS = true;
			} else 
			{
				// while we're not close to the waypoint, try to get away from the starting point
				distanceHeuristic -= Math.Min(maxDistance, Vector2.DistanceSquared(startingPosition, newPosition));
			}
			return distanceHeuristic;
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
			bool hasLOS = false;
			float distanceHeuristic = GetHeuristic(newPosition, ref hasLOS);
			WaypointSearchNode newNode = new WaypointSearchNode(newPosition, distanceHeuristic, parent)
			{
				hasLOS = hasLOS
			};
			if (parent == null || (!parent.IsBacktracking(newNode) && !visited.Contains(newNode)))
			{
				return newNode;
			} else
			{
#if DEBUG
				if(stuckPosition != default)
				{
					Main.NewText("Not adding node because " + parent.IsBacktracking(newNode) + " " + visited.Contains(newNode));
				}
#endif
				return null;
			}
		}

		public Vector2? Update()
		{
#if DEBUG
			if(Main.GameUpdateCount % 15 != 0)
			{
				return null;
			}
#endif
			//
			// Reset the state if necessary
			//
			int type = MinionWaypoint.Type;
			if(player.ownedProjectileCounts[type] == 0)
			{
				ResetState();
				return null;
			}
			iterations++;
			if(orderedPath.Count > 0 && 
				Vector2.DistanceSquared(orderedPath[0], player.Center) > PLAYER_DISTANCE_THRESHOLD * PLAYER_DISTANCE_THRESHOLD &&
				iterations > RECALCULATE_RATE_LIMIT)
			{
				ResetState();
			}
			waypointPosition = WaypointPos();
			if(waypointPosition != lastWaypointPosition || searchNodes.Count == 0)
			{
				// reset state if the waypoint moved
				ResetState();
				lastWaypointPosition = waypointPosition;
			}

			//
			// If the algorithm has terminated, return the result
			//
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
				FinalizePath();
				DrawPath();
				return currentBest.position;
			}

#if DEBUG
			DebugDust();
#endif
			//
			// Evaluate nodes
			//
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
#if DEBUG
			Main.NewText("Current pending nodes " + searchNodes.Count);
#endif
			//
			// Check if the algoritm has terminated
			//
			if(visited.Min != null && currentBest.distanceHeuristic > visited.Min.distanceHeuristic &&
				(stuckPosition == default || iterations - unstuckStartFrame > FRAMES_TO_GET_UNSTUCK))
			{
				noImprovementFrames++;
			} else
			{
				noImprovementFrames = 0;
			}
			// if we're about to fail, give a second chance by moving away from the stuck point
			if(noImprovementFrames == MAX_NO_IMPROVEMENT_FRAMES - 1 && stuckPosition == default)
			{
				searchNodes.Clear();
				searchNodes.Add(visited.Min);
				searchNodes.Min.parent = null;
				stuckPosition = visited.Min.position;
				unstuckStartFrame = iterations;
				noImprovementFrames = 0;
				visited.Clear();
#if DEBUG
				Main.NewText("Attempting to get unstuck from "+stuckPosition);
#endif
				return null;
			} else if (stuckPosition != default && iterations - unstuckStartFrame == FRAMES_TO_GET_UNSTUCK)
			{
#if DEBUG
				Main.NewText("Done attempting to get unstuck "+stuckPosition);
#endif
				bool hasLOS = false;
				WaypointSearchNode newBest = searchNodes.Min;
				newBest.distanceHeuristic = GetHeuristic(newBest.position, ref hasLOS);
				newBest.parent = null;
				searchNodes.Clear();
				visited.Clear();
				searchNodes.Add(newBest);
				return null;
			}
			if(iterations > MAX_ITERATIONS || noImprovementFrames > MAX_NO_IMPROVEMENT_FRAMES)
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
#if DEBUG
			Main.NewText("Resetting state!" + searchNodes.Count);
#endif
			lastWaypointPosition = default;
			startingPosition = player.Center;
			searchNodes = new SortedSet<WaypointSearchNode>();
			visited = new SortedSet<WaypointSearchNode>();
			searchFailed = false;
			searchSucceeded = false;
			pathFinalized = false;
			stuckPosition = default;
			iterations = 0;
			noImprovementFrames = 0;
			orderedPath = new List<Vector2>();
			searchNodes.Add(AddNode(null, 0));
		}

		// reduce the path to the minimum number of nodes with LOS to each other
		public void FinalizePath()
		{
			if(pathFinalized)
			{
				return;
			}
			pathFinalized = true;
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

		private void DebugDust()
		{
			bool isFirst = true;
			foreach(WaypointSearchNode node in searchNodes)
			{
				Dust.NewDust(node.position, 1, 1, DustType<MinionWaypointDust>(), 
					newColor: isFirst ? Color.Red : Color.MediumPurple, Scale: 1.0f);
				isFirst = false;
			}
			if(stuckPosition != default)
			{
				Dust.NewDust(stuckPosition, 1, 1, DustType<MinionWaypointDust>(), 
					newColor: Color.LimeGreen, Scale: 1.5f);
			}
		}
	}
}
