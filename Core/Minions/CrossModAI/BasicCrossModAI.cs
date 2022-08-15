using AmuletOfManyMinions.Core.Minions.AI;
using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI
{
	/// <summary>
	/// Basic cross-mod AI that allows for exposing AoMM-tracked state to other mods. Also include
	/// the basic "follow waypoint" code here, since it's pretty simple.
	/// </summary>
	internal class BasicCrossModAI : ICrossModSimpleMinion
	{

		public Projectile Projectile { get; set; }

		public Player Player => Main.player[Projectile.owner];
		public int BuffId { get; set; }
		public SimpleMinionBehavior Behavior { get; set; }

		internal int MaxSpeed { get; set; }
		internal int Inertia { get; set; }
		internal int SearchRange { get; set; }

		internal bool UseDefaultPathfindingMovement { get; set; }

		/// <summary>
		/// Cache the projectile's velocity
		/// </summary>
		private Vector2 CachedVelocity { get; set; }
		private Vector2 CachedPosition { get; set; }

		private Vector2 CachedPlayerPosition { get; set; }

		public BasicCrossModAI(
			Projectile projectile, int buffId, int maxSpeed = 8, int inertia = 8, int searchRange = 600, bool defaultPathfinding = false)
		{
			Projectile = projectile;
			BuffId = buffId;
			MaxSpeed = maxSpeed;
			Inertia = inertia;
			SearchRange = searchRange;
			UseDefaultPathfindingMovement = defaultPathfinding;
			
			Behavior = new(this);
		}

		public WaypointMovementStyle WaypointMovementStyle => WaypointMovementStyle.IDLE;


		internal void CacheProjectileState()
		{
			CachedVelocity = Projectile.velocity;
			CachedPosition = Projectile.position;
			CachedPlayerPosition = Player.position;
		}

		internal void UncacheProjectileState()
		{
			Projectile.velocity = CachedVelocity;
			Projectile.position = CachedPosition;
			Player.position = CachedPlayerPosition;
		}

		public virtual void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// Only take over idle movement if pathfinding
			if(!Behavior.IsFollowingBeacon || !UseDefaultPathfindingMovement) { return; }

			if(vectorToIdlePosition.LengthSquared() > MaxSpeed * MaxSpeed)
			{
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= MaxSpeed;
			} 
			if (!Behavior.Pathfinder.InTransit && vectorToIdlePosition.LengthSquared() < MaxSpeed * MaxSpeed)
			{
				Projectile.velocity = vectorToIdlePosition;
			} else
			{
				Projectile.velocity = (Projectile.velocity * (Inertia - 1) + vectorToIdlePosition) / Inertia;
			}
			CacheProjectileState();
			// trick to force many vanilla-styled minions into their flying animation
			// Would be surprised if this did not have unintended side effects
			Player.position.Y = CachedPosition.Y + 300;
		}
		
		public virtual Vector2? FindTarget()
		{
			return Behavior.SelectedEnemyInRange(SearchRange) is Vector2 target ? target - Projectile.Center : null;
		}

		public virtual void PostAI()
		{
			if(!Behavior.IsFollowingBeacon || !UseDefaultPathfindingMovement) { return; }
			UncacheProjectileState();
		}

		public virtual void AfterMoving() 
		{
			// no op
		}

		public virtual bool DoVanillaAI() => true;

		public virtual Vector2 IdleBehavior()
		{
			// no op
			return default;
		}


		public virtual bool MinionContactDamage() => false;

		public virtual void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// No op
		}
	}
}
