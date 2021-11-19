using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Items.Accessories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions
{

	internal enum WaypointMovementStyle
	{
		IDLE,
		TARGET,
		CUSTOM
	}

	public abstract class SimpleMinion : Minion
	{
		protected Vector2 vectorToIdle;
		protected Vector2? vectorToTarget;
		protected Vector2 oldVectorToIdle;
		protected Vector2? oldVectorToTarget = null;
		protected int? oldTargetNpcIndex = null;
		protected int framesSinceHadTarget = 0;
		protected bool attackThroughWalls = false;
		protected bool dealsContactDamage = true;
		protected int frameSpeed = 5;
		protected int proximityForOnHitTarget = 24;
		protected int targetFrameCounter = 0;
		protected int noLOSPursuitTime = 15; // time to chase the NPC after losing sight
		protected MinionPathfindingHelper pathfinder;

		internal virtual WaypointMovementStyle waypointMovementStyle => WaypointMovementStyle.IDLE;

		public int animationFrame { get; set; }

		public int groupAnimationFrames = 180;
		public int groupAnimationFrame
		{
			get => player.GetModPlayer<MinionSpawningItemPlayer>().idleMinionSyncronizationFrame % groupAnimationFrames;
		}
		public AttackState attackState = AttackState.IDLE;
		public bool usesTactics = true;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			// Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
			// ProjectileID.Sets.CountsAsHoming[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			// These below are needed for a minion weapon
			// Only controls if it deals damage to enemies on contact (more on that later)
			Projectile.friendly = true;
			// Determines multiple things
			Projectile.minion = true;
			// Determines damage type
			Projectile.DamageType = DamageClass.Summon; //TODO 1.4 check shot projectiles, the spawn for originalDamage
			// Amount of slots this minion occupies from the total minion slots available to the player (more on that later)
			Projectile.minionSlots = 1f;
			// Needed so the minion doesn't despawn on collision with enemies or tiles
			Projectile.penetrate = -1;
			// Makes the minion go through tiles
			Projectile.tileCollide = false;
			// use local projectile i-frames
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			// Makes sure this projectile is synced to other newly joined players 
			Projectile.netImportant = true;

			pathfinder = new MinionPathfindingHelper(this);
		}


		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles()
		{
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage()
		{
			return true;
		}

		public abstract Vector2 IdleBehavior();
		public abstract Vector2? FindTarget();
		public abstract void IdleMovement(Vector2 vectorToIdlePosition);
		public abstract void TargetedMovement(Vector2 vectorToTargetPosition);

		public virtual void AfterMoving() { }

		public virtual void Animate(int minFrame = 0, int? maxFrame = null)
		{

			// This is a simple "loop through all frames from top to bottom" animation
			Projectile.frameCounter++;
			if (Projectile.frame < minFrame)
			{
				Projectile.frame = minFrame;
			}
			if (Projectile.frameCounter >= frameSpeed)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= (maxFrame ?? Main.projFrames[Projectile.type]))
				{
					Projectile.frame = minFrame;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = true;
			return true;
		}

		public override void Behavior()
		{
			targetNPCIndex = null;
			vectorToIdle = IdleBehavior();
			bool useBeaconThisFrame = useBeacon;
			var tacticsPlayer = player.GetModPlayer<MinionTacticsPlayer>();
			var waypointsPlayer = player.GetModPlayer<MinionPathfindingPlayer>();
			bool didChangePathfindingState = false;
			bool isFollowingPath = false;
			bool tacticMissing = false;
			if(useBeacon && usesTactics)
			{
				currentTactic = tacticsPlayer.GetTacticForMinion(this);
				tacticMissing = currentTactic == null;
				useBeaconThisFrame &= ! tacticMissing && !currentTactic.IgnoreWaypoint;
				didChangePathfindingState = 
					(tacticsPlayer.DidUpdateAttackTarget || waypointsPlayer.DidUpdateWaypoint) &&
					!pathfinder.PrecheckPathCompletion();
			}
			// don't allow finding the target while travelling along path
			if(tacticMissing || (useBeaconThisFrame && (didChangePathfindingState || pathfinder.InTransit)))
			{
				vectorToTarget = null;
				targetNPCCacheFrames = currentTactic?.TargetCacheFrames ?? 999;
				framesSinceHadTarget = noLOSPursuitTime;
			} else
			{
				vectorToTarget = FindTarget();
				framesSinceHadTarget++;
			}
			animationFrame++;
			if (vectorToTarget is Vector2 targetPosition)
			{
				if (player.whoAmI == Main.myPlayer && oldVectorToTarget == null)
				{
					Projectile.netUpdate = true;
				}
				Projectile.tileCollide = !attackThroughWalls;
				framesSinceHadTarget = 0;
				Projectile.friendly = dealsContactDamage;
				TargetedMovement(targetPosition);
				oldVectorToTarget = vectorToTarget;
				oldTargetNpcIndex = targetNPCIndex;
			}
			else if (attackState != AttackState.RETURNING && oldTargetNpcIndex is int previousIndex && framesSinceHadTarget < noLOSPursuitTime)
			{
				Projectile.tileCollide = !attackThroughWalls;
				if (!Main.npc[previousIndex].active)
				{
					oldTargetNpcIndex = null;
					oldVectorToTarget = null;
				}
				else if (previousIndex < Main.maxNPCs)
				{
					vectorToTarget = Main.npc[previousIndex].Center - Projectile.Center;
					Projectile.friendly = dealsContactDamage;
					TargetedMovement((Vector2)vectorToTarget); // don't immediately give up if losing LOS
				}
			}
			else if (useBeaconThisFrame && pathfinder.NextPathfindingTarget() is Vector2 pathNode)
			{
				isFollowingPath = true;
				Projectile.friendly = false;
				if(pathfinder.isStuck)
				{
					pathfinder.GetUnstuck();
				} else
				{
					if(waypointMovementStyle == WaypointMovementStyle.IDLE)
					{
						IdleMovement(pathNode);
					} else
					{
						TargetedMovement(pathNode);
					}
					Projectile.tileCollide = !pathfinder.atStart && !attackThroughWalls;
				}
			} 
			else
			{
				if (framesSinceHadTarget > 30)
				{
					Projectile.tileCollide = false;
				}
				if (player.whoAmI == Main.myPlayer && oldVectorToTarget != null)
				{
					Projectile.netUpdate = true;
				}
				oldVectorToTarget = null;
				Projectile.friendly = false;
				IdleMovement(vectorToIdle);
			}
			if(useBeacon && !isFollowingPath)
			{
				pathfinder.DetachFromPath();
			}
			if (targetNPCIndex is int idx &&
				targetFrameCounter++ > Projectile.localNPCHitCooldown &&
				vectorToTarget is Vector2 target && target.LengthSquared() < proximityForOnHitTarget * proximityForOnHitTarget)
			{
				targetFrameCounter = 0;
				OnHitTarget(Main.npc[idx]);
			}
			AfterMoving();
			Animate();
			oldVectorToIdle = vectorToIdle;
			AdjustInertia();
		}

		/**
		 * Multiplayer safe approximation of OnHitNPC
		 */
		public virtual void OnHitTarget(NPC target)
		{
			// no-op
		}


		// utility methods
		public void TeleportToPlayer(ref Vector2 vectorToIdlePosition, float maxDistance)
		{
			if (Main.myPlayer == player.whoAmI && vectorToIdlePosition.LengthSquared() > maxDistance * maxDistance)
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
			if(otherMinions.Count == 0 && Projectile.type == projectileType)
			{
				otherMinions.Add(Projectile);
			}
			return otherMinions;
		}

		/**
		 * Optionally tune down the turning radius of minions for a gameplay
		 * experience closer to standard vanilla AI
		 */
		private void AdjustInertia()
		{
			if(ServerConfig.Instance.MinionsInnacurate && useBeacon && vectorToTarget is Vector2 target)
			{
				// only alter horizontal velocity, messes with gravity otherwise
				float accelerationX = Projectile.velocity.X - Projectile.oldVelocity.X;
				// only make minion more slugish when it's moving towards the enemy, allow it 
				// to fall away at regular speeds
				if(Math.Sign(accelerationX) == Math.Sign(target.X))
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
			if(!ServerConfig.Instance.MinionsInnacurate)
			{
				return initial;
			}
			float maxRotation = MathHelper.Pi / 8;
			float minRotation = MathHelper.Pi / 24;
			float minRotDist = 800f;
			if(targetNPCIndex is int idx)
			{
				float distance = Math.Min(minRotDist, Vector2.Distance(Main.npc[idx].Center, Projectile.Center));
				float rotation = MathHelper.Lerp(maxRotation, minRotation, distance/ minRotDist);
				return initial.RotatedBy(Main.rand.NextFloat(rotation) - rotation/2);
			} else
			{
				return initial.RotatedBy(Main.rand.NextFloat(minRotation) - minRotation/2);
			}
		}
	}
}
