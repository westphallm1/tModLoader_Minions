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

		private int MaxSpeed { get; set; }
		private int Inertia { get; set; }
		private int SearchRange { get; set; }

		private bool UseDefaultPathfindingMovement { get; set; }

		/// <summary>
		/// Cache the projectile's velocity
		/// </summary>
		private Vector2 CachedVelocity { get; set; }
		private Vector2 CachedPosition { get; set; }

		private Vector2 CachedPlayerPosition { get; set; }

		public BasicCrossModAI(
			Projectile projectile, int buffId, int maxSpeed, int inertia = 8, int searchRange = 600, bool defaultPathfinding = false)
		{
			Projectile = projectile;
			BuffId = buffId;
			MaxSpeed = maxSpeed;
			Inertia = inertia;
			SearchRange = searchRange;
			UseDefaultPathfindingMovement = defaultPathfinding;
			
			Behavior = new(this);
			Behavior.Player = Player;
		}

		public WaypointMovementStyle WaypointMovementStyle => WaypointMovementStyle.IDLE;

		public void IdleMovement(Vector2 vectorToIdlePosition)
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
				CachedVelocity = vectorToIdlePosition;
			} else
			{
				CachedVelocity = (Projectile.velocity * (Inertia - 1) + vectorToIdlePosition) / Inertia;
			}

			CachedPosition = Projectile.position;
			// trick to force many vanilla-styled minions into their flying animation
			// Would be surprised if this did not have unintended side effects
			CachedPlayerPosition = Player.position;
			Player.position.Y = CachedPosition.Y + 300;
		}
		
		public Vector2? FindTarget()
		{
			return Behavior.AnyEnemyInRange(SearchRange);
		}

		public void PostAI()
		{
			if(!Behavior.IsFollowingBeacon || !UseDefaultPathfindingMovement) { return; }

			Projectile.velocity = CachedVelocity;
			Projectile.position = CachedPosition;
			Player.position = CachedPlayerPosition;
		}

		public void AfterMoving() 
		{
			// no op
		}

		public bool DoVanillaAI() => true;

		public Vector2 IdleBehavior()
		{
			// no op
			return default;
		}


		public bool MinionContactDamage() => false;

		public void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// No op
		}
	}
}
