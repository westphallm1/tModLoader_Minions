using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.AI
{
	public interface ISimpleMinion : IMinion
	{
		// Instance variables, not sure if it's the best to force them to be exposed here
		int AnimationFrame { get; set; }
		bool AttackThroughWalls { get; set; }
		bool DealsContactDamage { get; set; }
		int FrameSpeed { get; set; }
		int FramesSinceHadTarget { get; set; }
		int GroupAnimationFrame { get; }
		int NoLOSPursuitTime { get; set; }
		int? OldTargetNpcIndex { get; set; }
		Vector2 OldVectorToIdle { get; set; }
		Vector2? OldVectorToTarget { get; set; }
		MinionPathfindingHelper Pathfinder { get; set; }
		int ProximityForOnHitTarget { get; set; }
		int TargetFrameCounter { get; set; }
		bool UsesTactics { get; }
		Vector2 VectorToIdle { get; set; }
		Vector2? VectorToTarget { get; set; }
		WaypointMovementStyle WaypointMovementStyle { get; }
		public AttackState AttackState { get; set; }

		// Actual methods
		void AfterMoving();
		void Animate(int minFrame = 0, int? maxFrame = null);
		Vector2? FindTarget();
		Vector2 IdleBehavior();
		void IdleMovement(Vector2 vectorToIdlePosition);
		bool MinionContactDamage();
		void OnHitTarget(NPC target);
		bool OnTileCollide(Vector2 oldVelocity);
		void TargetedMovement(Vector2 vectorToTargetPosition);
		void TeleportToPlayer(ref Vector2 vectorToIdlePosition, float maxDistance);
	}

	internal class SimpleMinionBehavior : MinionBehavior
	{

		private int ProximityForOnHitTarget = 24;
		private new ISimpleMinion Minion { get; set; }

		public SimpleMinionBehavior(ISimpleMinion minion) : base(minion)
		{
			Minion = minion;
		}

		public void MainBehavior()
		{
			Minion.TargetNPCIndex = null;
			Minion.VectorToIdle = Minion.IdleBehavior();

			// Determine whether to update tactic selection, and whether th change pathfinding state
			bool useBeaconThisFrame = Minion.UseBeacon;
			var tacticsPlayer = Player.GetModPlayer<MinionTacticsPlayer>();
			var waypointsPlayer = Player.GetModPlayer<MinionPathfindingPlayer>();
			bool didChangePathfindingState = false;
			bool isFollowingPath = false;
			bool tacticMissing = false;
			if (Minion.UseBeacon && Minion.UsesTactics)
			{
				Minion.CurrentTactic = tacticsPlayer.GetTacticForMinion(Minion);
				tacticMissing = Minion.CurrentTactic == null;
				useBeaconThisFrame &= !tacticMissing && !Minion.CurrentTactic.IgnoreWaypoint;
				didChangePathfindingState =
					(tacticsPlayer.DidUpdateAttackTarget || waypointsPlayer.DidUpdateWaypoint) &&
					!Minion.Pathfinder.PrecheckPathCompletion();
			}
			// don't allow finding the target while travelling along path
			if (tacticMissing || (useBeaconThisFrame && (didChangePathfindingState || Minion.Pathfinder.InTransit)))
			{
				Minion.VectorToTarget = null;
				Minion.TargetNPCCacheFrames = Minion.CurrentTactic?.TargetCacheFrames ?? 999;
				Minion.FramesSinceHadTarget = Minion.NoLOSPursuitTime;
			}
			else
			{
				Minion.VectorToTarget = Minion.FindTarget();
				Minion.FramesSinceHadTarget++;
			}

			// Update frame counter metadata
			Minion.AnimationFrame++;

			// Do targeted movement using the most recently found NPC
			if (Minion.VectorToTarget is Vector2 targetPosition)
			{
				if (Player.whoAmI == Main.myPlayer && Minion.OldVectorToTarget == null)
				{
					Projectile.netUpdate = true;
				}
				Projectile.tileCollide = !Minion.AttackThroughWalls;
				Minion.FramesSinceHadTarget = 0;
				Projectile.friendly = Minion.DealsContactDamage;
				Minion.TargetedMovement(targetPosition);
				Minion.OldVectorToTarget = Minion.VectorToTarget;
				Minion.OldTargetNpcIndex = Minion.TargetNPCIndex;
			}
			// For several frames after losing the target, contine doing targeted movement against the previous cached target
			else if (Minion.AttackState != AttackState.RETURNING && 
				Minion.OldTargetNpcIndex is int previousIndex && Minion.FramesSinceHadTarget < Minion.NoLOSPursuitTime)
			{
				Projectile.tileCollide = !Minion.AttackThroughWalls;
				if (!Main.npc[previousIndex].active)
				{
					Minion.OldTargetNpcIndex = null;
					Minion.OldVectorToTarget = null;
				}
				else if (previousIndex < Main.maxNPCs)
				{
					Minion.VectorToTarget = Main.npc[previousIndex].Center - Projectile.Center;
					Projectile.friendly = Minion.DealsContactDamage;
					Minion.TargetedMovement((Vector2)Minion.VectorToTarget); // don't immediately give up if losing LOS
				}
			}
			// If no target and beacon is active, follow the beacon
			else if (useBeaconThisFrame && Minion.Pathfinder.NextPathfindingTarget() is Vector2 pathNode)
			{
				isFollowingPath = true;
				Projectile.friendly = false;
				if (Minion.Pathfinder.isStuck)
				{
					Minion.Pathfinder.GetUnstuck();
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
					Projectile.tileCollide = !Minion.Pathfinder.atStart && !Minion.AttackThroughWalls;
				}
			}
			// Do idle movement
			else
			{
				if (Minion.FramesSinceHadTarget > 30)
				{
					Projectile.tileCollide = false;
				}
				if (Player.whoAmI == Main.myPlayer && Minion.OldVectorToTarget != null)
				{
					Projectile.netUpdate = true;
				}
				Minion.OldVectorToTarget = null;
				Projectile.friendly = false;
				Minion.IdleMovement(Minion.VectorToIdle);
			}

			// If we've reached the end of the pathfinding path, return to regular AI
			if (Minion.UseBeacon && !isFollowingPath)
			{
				Minion.Pathfinder.DetachFromPath();
			}

			// Perform a Multiplayer-safe approximation of OnHitNPC() against just the target NPC
			if (Minion.TargetNPCIndex is int idx &&
				Minion.TargetFrameCounter++ > Projectile.localNPCHitCooldown &&
				Minion.VectorToTarget is Vector2 target && target.LengthSquared() < ProximityForOnHitTarget * ProximityForOnHitTarget)
			{
				Minion.TargetFrameCounter = 0;
				Minion.OnHitTarget(Main.npc[idx]);
			}
			Minion.AfterMoving();
			Minion.Animate();
			Minion.OldVectorToIdle = Minion.VectorToIdle;
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
			if (ServerConfig.Instance.MinionsInnacurate && Minion.UseBeacon && Minion.VectorToTarget is Vector2 target)
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
			if (Minion.TargetNPCIndex is int idx)
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
	}
}
