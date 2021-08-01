using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Items.Accessories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

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
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
			// Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
			ProjectileID.Sets.Homing[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			// These below are needed for a minion weapon
			// Only controls if it deals damage to enemies on contact (more on that later)
			projectile.friendly = true;
			// Only determines the damage type
			projectile.minion = true;
			// Amount of slots this minion occupies from the total minion slots available to the player (more on that later)
			projectile.minionSlots = 1f;
			// Needed so the minion doesn't despawn on collision with enemies or tiles
			projectile.penetrate = -1;
			// Makes the minion go through tiles
			projectile.tileCollide = false;
			// use local projectile i-frames
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 10;
			// Makes sure this projectile is synced to other newly joined players 
			projectile.netImportant = true;

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
			projectile.frameCounter++;
			if (projectile.frame < minFrame)
			{
				projectile.frame = minFrame;
			}
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= (maxFrame ?? Main.projFrames[projectile.type]))
				{
					projectile.frame = minFrame;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = true;
			return true;
		}

		public override void Behavior()
		{
			targetNPCIndex = null;
			vectorToIdle = IdleBehavior();
			bool useBeaconThisFrame = useBeacon;
			if(useBeacon && usesTactics)
			{
				currentTactic = player.GetModPlayer<MinionTacticsPlayer>().GetTacticForMinion(this);
				useBeaconThisFrame &= !currentTactic.IgnoreWaypoint;
			}
			// don't allow finding the target while travelling along path
			if(useBeaconThisFrame && pathfinder.InTransit)
			{
				vectorToTarget = null;
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
					projectile.netUpdate = true;
				}
				if (useBeacon)
				{
					pathfinder.DetachFromPath();
				}
				projectile.tileCollide = !attackThroughWalls;
				framesSinceHadTarget = 0;
				TargetedMovement(targetPosition);
				oldVectorToTarget = vectorToTarget;
				oldTargetNpcIndex = targetNPCIndex;
			}
			else if (attackState != AttackState.RETURNING && oldTargetNpcIndex is int previousIndex && framesSinceHadTarget < noLOSPursuitTime)
			{
				if (useBeacon)
				{
					pathfinder.DetachFromPath();
				}
				projectile.tileCollide = !attackThroughWalls;
				if (!Main.npc[previousIndex].active)
				{
					oldTargetNpcIndex = null;
					oldVectorToTarget = null;
				}
				else if (previousIndex < Main.maxNPCs)
				{
					vectorToTarget = Main.npc[previousIndex].Center - projectile.Center;
					TargetedMovement((Vector2)vectorToTarget); // don't immediately give up if losing LOS
				}
			}
			else if (useBeaconThisFrame && pathfinder.NextPathfindingTarget() is Vector2 pathNode)
			{
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
					projectile.tileCollide = !pathfinder.atStart && !attackThroughWalls;
				}
			} 
			else
			{
				if (framesSinceHadTarget > 30)
				{
					projectile.tileCollide = false;
				}
				if (player.whoAmI == Main.myPlayer && oldVectorToTarget != null)
				{
					projectile.netUpdate = true;
				}
				oldVectorToTarget = null;
				IdleMovement(vectorToIdle);
			}
			if (targetNPCIndex is int idx &&
				targetFrameCounter++ > projectile.localNPCHitCooldown &&
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
			if (Main.myPlayer == player.whoAmI && vectorToIdlePosition.Length() > maxDistance)
			{
				projectile.position += vectorToIdlePosition;
				projectile.velocity = Vector2.Zero;
				projectile.netUpdate = true;
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
				if (other.active && other.owner == projectile.owner && other.type == projectileType)
				{
					otherMinions.Add(other);
				}
			}
			otherMinions.Sort((x, y) => x.minionPos - y.minionPos);
			if(otherMinions.Count == 0 && projectile.type == projectileType)
			{
				otherMinions.Add(projectile);
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
				float accelerationX = projectile.velocity.X - projectile.oldVelocity.X;
				// only make minion more slugish when it's moving towards the enemy, allow it 
				// to fall away at regular speeds
				if(Math.Sign(accelerationX) == Math.Sign(target.X))
				{
					accelerationX *= 0.75f;
				}
				projectile.velocity.X = projectile.oldVelocity.X + accelerationX;
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
				float distance = Math.Min(minRotDist, Vector2.Distance(Main.npc[idx].Center, projectile.Center));
				float rotation = MathHelper.Lerp(maxRotation, minRotation, distance/ minRotDist);
				return initial.RotatedBy(Main.rand.NextFloat(rotation) - rotation/2);
			} else
			{
				return initial.RotatedBy(Main.rand.NextFloat(minRotation) - minRotation/2);
			}
		}
	}
}
