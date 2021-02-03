using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	public abstract class HeadCirclingGroupAwareMinion : GroupAwareMinion
	{
		protected float idleAngle;
		protected int targetSearchDistance = 800;
		protected int idleCircle = 40;
		protected int idleCircleHeight = 10;
		protected int idleInertia = 15;
		protected int maxSpeed = 12;

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = false;
			attackState = AttackState.IDLE;
			attackFrames = 30;
			projectile.localNPCHitCooldown = 20;
			projectile.frame = (2 * projectile.minionPos) % 6;
		}

		public virtual List<Projectile> GetIdleSpaceSharingMinions()
		{
			return IdleLocationSets.GetProjectilesInSet(IdleLocationSets.circlingHead, player.whoAmI);
		}

		public virtual Vector2 CenterOfRotation()
		{
			return player.Top;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			List<Projectile> minions = GetIdleSpaceSharingMinions();
			Vector2 idlePosition = CenterOfRotation();
			int minionCount = minions.Count;
			// this was silently failing sometimes, don't know why
			if (minionCount > 0)
			{
				int radius = idleCircle;
				Vector2 maxCircle = CenterOfRotation() + new Vector2(idleCircle, -20);
				if (!Collision.CanHitLine(maxCircle, 1, 1, player.Top, 1, 1))
				{
					radius = 7;
				}
				int order = minions.IndexOf(projectile);
				idleAngle = (2 * PI * order) / minionCount;
				idleAngle += 2 * PI * groupAnimationFrame / groupAnimationFrames;
				idlePosition.X += 2 + radius * (float)Math.Cos(idleAngle);
				idlePosition.Y += -20 + idleCircleHeight * (float)Math.Sin(idleAngle);
			}
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override Vector2? FindTarget()
		{
			if (FindTargetInTurnOrder(targetSearchDistance, projectile.Center) is Vector2 target)
			{
				projectile.friendly = true;
				return target;
			}
			else
			{
				projectile.friendly = false;
				return null;
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// alway clamp to the idle position
			projectile.tileCollide = false;
			if (vectorToIdlePosition.Length() < maxSpeed)
			{
				projectile.rotation = 0;
				projectile.spriteDirection = (idleAngle % (2 * PI)) > PI ? -1 : 1;
			}
			else
			{
				vectorToIdlePosition.SafeNormalize();
				vectorToIdlePosition *= maxSpeed;
			}
			projectile.velocity = (projectile.velocity * (idleInertia - 1) + vectorToIdlePosition) / idleInertia;
		}
	}
}
