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
	internal class ProjectileStateCache
	{
		internal Vector2? Position { get; private set; }
		internal Vector2? Velocity { get; private set; }

		internal Vector2? PlayerPosition { get; private set; }
		internal bool? TileCollide { get; private set; }

		public void Clear()
		{
			Position = null;
			Velocity = null;
			TileCollide = null;
			PlayerPosition = null;
		}

		public void Cache(Projectile proj)
		{
			Position ??= proj.position;
			Velocity ??= proj.velocity;
			TileCollide ??= proj.tileCollide;
			PlayerPosition ??= Main.player[proj.owner].position;
		}

		public void Uncache(Projectile proj)
		{
			proj.position = Position ?? proj.position;
			proj.velocity = Velocity ?? proj.velocity;
			proj.tileCollide = TileCollide ?? proj.tileCollide;
			Main.player[proj.owner].position = PlayerPosition ?? Main.player[proj.owner].position;
			Clear();
		}
	}

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

		public int MaxSpeed { get; set; }
		internal int Inertia { get; set; }
		internal int SearchRange { get; set; }

		internal bool UseDefaultPathfindingMovement { get; set; }

		internal ProjectileStateCache ProjCache { get; set; } = new();

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


		internal void FakePlayerFlyingHeight()
		{
			ProjCache.Cache(Projectile);
			Player.position.Y = Projectile.position.Y - 320;
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
			// trick to force many vanilla-styled minions into their flying animation
			// Would be surprised if this did not have unintended side effects
			FakePlayerFlyingHeight();
		}
		
		public virtual Vector2? FindTarget()
		{
			return Behavior.SelectedEnemyInRange(SearchRange) is Vector2 target ? target - Projectile.Center : null;
		}

		public virtual void PostAI()
		{
			ProjCache.Uncache(Projectile);
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
