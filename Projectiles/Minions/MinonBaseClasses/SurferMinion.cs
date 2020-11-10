using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	public abstract class SurferMinion<T> : GroupAwareMinion<T> where T : ModBuff
	{
		protected float idleAngle;
		protected int framesSinceDiveBomb = 0;
		protected int diveBombHeightRequirement = 40;
		protected int diveBombHeightTarget = 120;
		protected int diveBombHorizontalRange = 80;
		protected int diveBombFrameRateLimit = 60;
		protected int diveBombSpeed = 12;
		protected int diveBombInertia = 15;
		protected int approachSpeed = 8;
		protected int approachInertia = 40;
		protected int targetSearchDistance = 800;
		protected int idleCircle = 40;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paper Surfer");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = false;
			attackState = AttackState.IDLE;
			attackFrames = 30;
			animationFrames = 240;
			projectile.frame = (2 * projectile.minionPos) % 6;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			List<Projectile> minions = GetActiveMinions();
			Vector2 idlePosition = player.Top;
			int minionCount = minions.Count;
			// this was silently failing sometimes, don't know why
			if (minionCount > 0)
			{
				int radius = idleCircle;
				Vector2 maxCircle = player.Top + new Vector2(idleCircle, -20);
				if (!Collision.CanHitLine(maxCircle, 1, 1, player.Top, 1, 1))
				{
					radius = 7;
				}
				int order = minions.IndexOf(projectile);
				idleAngle = (2 * PI * order) / minionCount;
				idleAngle += 2 * PI * minions[0].ai[1] / animationFrames;
				idlePosition.X += 2 + radius * (float)Math.Cos(idleAngle);
				idlePosition.Y += -20 + 10 * (float)Math.Sin(idleAngle);
			}
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override void OnHitTarget(NPC target)
		{
			projectile.velocity.SafeNormalize();
			projectile.velocity *= 6; // "kick" it away from the enemy it just hit
			framesSinceDiveBomb = 0;
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

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int inertia = approachInertia;
			int speed = approachSpeed;

			projectile.friendly = framesSinceDiveBomb++ > 20; // limit rate of attack
			if (framesSinceDiveBomb < diveBombFrameRateLimit || Math.Abs(vectorToTargetPosition.X) > diveBombHorizontalRange)
			{
				// always aim for "above" while approaching, if it's in the line of sight
				if (Collision.CanHitLine(projectile.Center, 1, 1,
					new Vector2(vectorToTargetPosition.X, vectorToTargetPosition.Y - diveBombHeightTarget), 1, 1))
				{
					vectorToTargetPosition.Y -= diveBombHeightTarget;
				}
				projectile.rotation = 0;
			}
			else if (vectorToTargetPosition.Y > diveBombHeightRequirement)
			{
				inertia = diveBombInertia;
				speed = diveBombSpeed;
				projectile.rotation = (projectile.velocity.X > 0 ? 7 : 5) * PI / 4;
			}
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= speed;
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			projectile.spriteDirection = projectile.velocity.X > 0 ? -1 : 1;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// alway clamp to the idle position
			projectile.tileCollide = false;
			int inertia = 15;
			int maxSpeed = 12;
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
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			// This is a simple "loop through all frames from top to bottom" animation
			minFrame = (2 * projectile.minionPos) % 6;
			int frameSpeed = 15;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame = projectile.frame == minFrame ? minFrame + 1 : minFrame;
			}
		}
	}
}
