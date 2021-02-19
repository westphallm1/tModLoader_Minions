using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.Pathfinding
{
	public class MinionPathfindingHelper
	{
		private Projectile projectile;
		internal PathfindingHelper pathfinder;
		// how close to a node we have to be before progressing to the next node
		static int NODE_PROXIMITY = 32;
		static int NODE_PROXIMITY_SQUARED = NODE_PROXIMITY * NODE_PROXIMITY;
		// number of nodes to check against before starting from the beginning
		static int HOMING_LOS_CHECKS = 4;
		static float NO_PROGRESS_THRESHOLD = 1.25f;
		internal int nodeIndex = -1;

		internal int noProgressFrames = 0;
		internal int unstuckFrames = 0;
		internal bool isStuck = false;

		internal MinionPathfindingHelper(Projectile projectile)
		{
			this.projectile = projectile;
			
		}

		internal void SetPathStartingPoint()
		{
			// find the current node closest to the projectile
			List<Vector2> orderedNodes = pathfinder.orderedPath.OrderBy(node => Vector2.DistanceSquared(projectile.Center, node)).ToList();
			nodeIndex = 0;
			for(int i = 0; i < Math.Min(orderedNodes.Count, HOMING_LOS_CHECKS); i++)
			{
				if(Collision.CanHitLine(projectile.Center, 1, 1, orderedNodes[i], 1, 1))
				{
					// probably a better way to find the permuted index
					nodeIndex = pathfinder.orderedPath.IndexOf(orderedNodes[i]);
					break;
				}
			}
		}

		// in case we need to leave the path for eg. attacking an enemy
		internal void DetachFromPath()
		{
			nodeIndex = -1;
			noProgressFrames = 0;
			unstuckFrames = 0;
		}

		internal void MoveAlongPath()
		{
			// sometimes it needs a bit of help
			if(nodeIndex <0 || nodeIndex > pathfinder.orderedPath.Count)
			{
				// bail out and reset state
				noProgressFrames = 0;
				unstuckFrames = 0;
				isStuck = false;
				return;
			}
			Vector2 target = pathfinder.orderedPath[nodeIndex] - projectile.position;
			target.SafeNormalize();
			target *= 4;
			projectile.velocity = target;
			projectile.tileCollide = false;
			unstuckFrames++;
			if(unstuckFrames > 5)
			{
				noProgressFrames = 0;
				unstuckFrames = 0;
				isStuck = false;
			}
		}

		internal Vector2? NextPathfindingTarget()
		{
			// initialize late to avoid any lifecycle issues
			if(pathfinder is null)
			{
				DetachFromPath();
				pathfinder = Main.player[projectile.owner]?.GetModPlayer<MinionPathfindingPlayer>()?.pHelper;
			}
			if(pathfinder.searchFailed || !pathfinder.searchActive)
			{
				DetachFromPath();
				return null;
			} else if (!pathfinder.pathFinalized)
			{
				DetachFromPath();
				// idle while the algorithm is still running
				return projectile.position;
			}
			if(nodeIndex == -1)
			{
				SetPathStartingPoint();
			}
			// simple approach: Go towards a node until you get close enough, then go to the next node
			List<Vector2> path = pathfinder.orderedPath;
			if(Vector2.DistanceSquared(projectile.Center, path[nodeIndex]) < NODE_PROXIMITY_SQUARED)
			{
				nodeIndex = Math.Min(path.Count-1, nodeIndex + 1);
			} 
			if(Math.Abs(projectile.velocity.Length()) < NO_PROGRESS_THRESHOLD)
			{
				noProgressFrames++;
			}
			if(noProgressFrames > 5)
			{
				isStuck = true;
				return projectile.position;
			}
			// make sure the target exceeds a certain lenght threshold,
			// so the AI will speed up the minions
			Vector2 target = path[nodeIndex] - projectile.position;
			target.SafeNormalize();
			target *= 12;
			return target;
		}
	}
}
