using AmuletOfManyMinions.Core.Minions.CrossModAI;
using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Minions.AI
{
	public interface ISimpleMinion : IMinion
	{

		// This is a little roundabout circular reference. Should still be garbage collected (maybe)
		SimpleMinionBehavior Behavior { get; }

		WaypointMovementStyle WaypointMovementStyle { get; }

		void AfterMoving();
		public void Animate(int minFrame = 0, int? maxFrame = null)
		{
			// Empty default
		}

		public Vector2? FindTarget()
		{
			// Empty default
			return default;
		}

		Vector2 IdleBehavior();
		void IdleMovement(Vector2 vectorToIdlePosition);
		public bool MinionContactDamage() => true;

		public void OnHitTarget(NPC target)
		{
			// Empty default
		}

		void TargetedMovement(Vector2 vectorToTargetPosition);
	}

	internal class MinionGroupState
	{
		internal List<Projectile> Others { get; set; }
		internal Projectile Leader { get; set; }
		internal Projectile Head { get; set; }

		internal void Clear()
		{
			Others = null;
			Leader = null;
			Head = null;
		}
	}
	public class SimpleMinionBehavior : MinionBehavior
	{

		// Instance variables, not sure if it's the best to put them here
		internal int AnimationFrame { get; set; }
		internal bool AttackThroughWalls { get; set; }
		internal bool DealsContactDamage { get; set; } = true;
		internal int FrameSpeed { get; set; } = 5;
		internal int FramesSinceHadTarget { get; set; }

		internal int NoLOSPursuitTime { get; set; } = 15; // time to chase the NPC after losing sight
		internal int? OldTargetNpcIndex { get; set; }
		internal Vector2 OldVectorToIdle { get; set; }
		internal Vector2? OldVectorToTarget { get; set; }
		internal MinionPathfindingHelper Pathfinder { get; set; }
		internal int TargetFrameCounter { get; set; }
		internal bool UsesTactics { get; set; }
		internal Vector2 VectorToIdle { get; set; }
		internal Vector2? VectorToTarget { get; set; }
		internal AttackState AttackState { get; set; } = AttackState.IDLE;

		internal MinionGroupState GroupState { get; set; } = new();

		internal bool IsFollowingBeacon { get; set; }

		internal Vector2? NextPathfindingTarget { get; set; }

		internal Vector2? PathfindingDestination { get; set; }

		private readonly int ProximityForOnHitTarget = 24;

		public int GroupAnimationFrames = 180;
		public int GroupAnimationFrame => 
			Player.GetModPlayer<MinionSpawningItemPlayer>().idleMinionSyncronizationFrame % GroupAnimationFrames;

		internal bool IsPrimaryFrame => Projectile.extraUpdates == 0 || AnimationFrame % (Projectile.extraUpdates + 1) == 0;

		private new ISimpleMinion Minion { get; set; }

		public SimpleMinionBehavior(ISimpleMinion minion) : base(minion)
		{
			Minion = minion;
			Pathfinder = new(minion);
		}

		private void ResetState()
		{
			TargetNPCIndex = null;
			GroupState.Clear();
			IsFollowingBeacon = false;
			PathfindingDestination = null;
			NextPathfindingTarget = null;
		}

		public void MainBehavior()
		{
			ResetState();
			VectorToIdle = Minion.IdleBehavior();

			// Determine whether to update tactic selection, and whether th change pathfinding state
			bool useBeaconThisFrame = UseBeacon;
			var tacticsPlayer = Player.GetModPlayer<MinionTacticsPlayer>();
			var waypointsPlayer = Player.GetModPlayer<MinionPathfindingPlayer>();
			bool didChangePathfindingState = false;
			bool tacticMissing = false;
			if (UseBeacon && UsesTactics)
			{
				CurrentTactic = tacticsPlayer.GetTacticForMinion(Minion);
				tacticMissing = CurrentTactic == null;
				useBeaconThisFrame &= !tacticMissing && !CurrentTactic.IgnoreWaypoint;
				didChangePathfindingState =
					(tacticsPlayer.DidUpdateAttackTarget || waypointsPlayer.DidUpdateWaypoint) &&
					!Pathfinder.PrecheckPathCompletion();
			}
			// don't allow finding the target while traveling along path
			if (tacticMissing || (useBeaconThisFrame && (didChangePathfindingState || Pathfinder.InTransit)))
			{
				VectorToTarget = null;
				TargetNPCCacheFrames = CurrentTactic?.TargetCacheFrames ?? 999;
				FramesSinceHadTarget = NoLOSPursuitTime;
			}
			else
			{
				VectorToTarget = Minion.FindTarget();
				FramesSinceHadTarget++;
			}

			// Update frame counter metadata
			AnimationFrame++;

			// Do targeted movement using the most recently found NPC
			if (VectorToTarget is Vector2 targetPosition)
			{
				if (Player.whoAmI == Main.myPlayer && OldVectorToTarget == null)
				{
					Projectile.netUpdate = true;
				}
				Projectile.tileCollide = !AttackThroughWalls;
				FramesSinceHadTarget = 0;
				Projectile.friendly = DealsContactDamage;
				Minion.TargetedMovement(targetPosition);
				OldVectorToTarget = VectorToTarget;
				OldTargetNpcIndex = TargetNPCIndex;
			}
			// For several frames after losing the target, continue doing targeted movement against the previous cached target
			else if (AttackState != AttackState.RETURNING && 
				OldTargetNpcIndex is int previousIndex && FramesSinceHadTarget < NoLOSPursuitTime)
			{
				Projectile.tileCollide = !AttackThroughWalls;
				if (!Main.npc[previousIndex].active)
				{
					OldTargetNpcIndex = null;
					OldVectorToTarget = null;
				}
				else if (previousIndex < Main.maxNPCs)
				{
					VectorToTarget = Main.npc[previousIndex].Center - Projectile.Center;
					TargetNPCIndex = OldTargetNpcIndex;
					Projectile.friendly = DealsContactDamage;
					Minion.TargetedMovement((Vector2)VectorToTarget); // don't immediately give up if losing LOS
				}
			}
			// If no target and beacon is active, follow the beacon
			else if (useBeaconThisFrame && Pathfinder.NextPathfindingTarget() is Vector2 pathNode)
			{
				IsFollowingBeacon = true;
				NextPathfindingTarget = pathNode;
				PathfindingDestination = Pathfinder.pathfinder.waypointPosition;
				Projectile.friendly = false;
				if (Pathfinder.isStuck)
				{
					Pathfinder.GetUnstuck();
				}
				else
				{
					if (Minion.WaypointMovementStyle == WaypointMovementStyle.IDLE)
					{
						Minion.IdleMovement(pathNode);
					}
					else
					{
						Minion.TargetedMovement(pathNode);
					}
					Projectile.tileCollide = !Pathfinder.atStart && !AttackThroughWalls;
				}
			}
			// Do idle movement
			else
			{
				if (FramesSinceHadTarget > 30)
				{
					Projectile.tileCollide = false;
				}
				if (Player.whoAmI == Main.myPlayer && OldVectorToTarget != null)
				{
					Projectile.netUpdate = true;
				}
				OldVectorToTarget = null;
				Projectile.friendly = false;
				Minion.IdleMovement(VectorToIdle);
			}

			// If we've reached the end of the pathfinding path, return to regular AI
			if (UseBeacon && !IsFollowingBeacon)
			{
				Pathfinder.DetachFromPath();
			}

			// Perform a Multiplayer-safe approximation of OnHitNPC() against just the target NPC
			if (TargetFrameCounter++ > Projectile.localNPCHitCooldown && TargetNPCIndex is int idx &&
				VectorToTarget is Vector2 target && target.LengthSquared() < ProximityForOnHitTarget * ProximityForOnHitTarget)
			{
				TargetFrameCounter = 0;
				Minion.OnHitTarget(Main.npc[idx]);
			}
			Minion.AfterMoving();
			Minion.Animate();
			OldVectorToIdle = VectorToIdle;
			AdjustInertia();
		}

		public void TeleportToPlayer(ref Vector2 vectorToIdlePosition, float maxDistance)
		{
			if (Main.myPlayer == Player.whoAmI && vectorToIdlePosition.LengthSquared() > maxDistance * maxDistance)
			{
				Projectile.position += vectorToIdlePosition;
				Projectile.velocity = Vector2.Zero;
				Projectile.netUpdate = true;
				vectorToIdlePosition = Vector2.Zero;
			}
		}


		public List<Projectile> GetMinionsOfType(int projectileType)
		{
			var otherMinions = new List<Projectile>();
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (other.active && other.owner == Projectile.owner && other.type == projectileType)
				{
					otherMinions.Add(other);
				}
			}
			otherMinions.Sort((x, y) => x.minionPos - y.minionPos);
			if (otherMinions.Count == 0 && Projectile.type == projectileType)
			{
				otherMinions.Add(Projectile);
			}
			return otherMinions;
		}

		/**
		 * Optionally tune down the turning radius of minions for a gameplay
		 * experience closer to standard vanilla AI
		 */
		internal void AdjustInertia()
		{
			if (ServerConfig.Instance.MinionsInnacurate && UseBeacon && VectorToTarget is Vector2 target)
			{
				// only alter horizontal velocity, messes with gravity otherwise
				float accelerationX = Projectile.velocity.X - Projectile.oldVelocity.X;
				// only make minion more slugish when it's moving towards the enemy, allow it 
				// to fall away at regular speeds
				if (Math.Sign(accelerationX) == Math.Sign(target.X))
				{
					accelerationX *= 0.75f;
				}
				Projectile.velocity.X = Projectile.oldVelocity.X + accelerationX;
			}
		}

		/**
		 * Optionally introduce a shot spread to minions for a gameplay experience closer to standard
		 * vanilla ai
		 */
		internal Vector2 VaryLaunchVelocity(Vector2 initial)
		{
			if (!ServerConfig.Instance.MinionsInnacurate)
			{
				return initial;
			}
			float maxRotation = MathHelper.Pi / 8;
			float minRotation = MathHelper.Pi / 24;
			float minRotDist = 800f;
			if (TargetNPCIndex is int idx)
			{
				float distance = Math.Min(minRotDist, Vector2.Distance(Main.npc[idx].Center, Projectile.Center));
				float rotation = MathHelper.Lerp(maxRotation, minRotation, distance / minRotDist);
				return initial.RotatedBy(Main.rand.NextFloat(rotation) - rotation / 2);
			}
			else
			{
				return initial.RotatedBy(Main.rand.NextFloat(minRotation) - minRotation / 2);
			}
		}

		// Methods lifted from GroupAwareMinion, used by many subclasses

		// TODO this is implemented in a way that is not safe on cross mod content (affects Projectile.ai)
		// just use a proxy for starters
		public bool IsMyTurn() => true;
		public List<Projectile> GetActiveMinions()
		{
			GroupState.Others ??= GetMinionsOfType(Projectile.type);
			return GroupState.Others;
		}

		public List<Projectile> GetAllMinionsOwnedByPlayer()
		{
			return Main.projectile
				.Where(p => p.active && p.owner == Projectile.owner && (p.minion || ProjectileID.Sets.MinionShot[p.type]))
				.ToList();
		}

		public Projectile GetHead(int headType)
		{
			GroupState.Head ??= GetMinionsOfType(headType).FirstOrDefault();
			return GroupState.Head;
		}

		public Projectile GetFirstMinion(List<Projectile> others = null)
		{
			GroupState.Leader ??= (others ?? GetActiveMinions()).FirstOrDefault();
			return GroupState.Leader;
		}

		public void DistanceFromGroup(ref Vector2 distanceToTarget, int separation = 16, int closeDistance = 32)
		{
			// if multiple minions are gathered on a target, space them out a little bit
			if (distanceToTarget.Length() < closeDistance)
			{
				foreach (Projectile otherMinion in GetAllMinionsOwnedByPlayer())
				{
					if (otherMinion.whoAmI == Projectile.whoAmI)
					{
						continue;
					}
					if (Projectile.Hitbox.Intersects(otherMinion.Hitbox))
					{
						Vector2 difference = otherMinion.Center - Projectile.Center;
						difference.SafeNormalize();
						distanceToTarget += -separation * difference;
					}
				}
			}
		}
	}
}
