using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	public abstract class SurferMinion : HeadCirclingGroupAwareMinion
	{
		protected int framesSinceDiveBomb = 0;
		protected int diveBombHeightRequirement = 40;
		protected int diveBombHeightTarget = 120;
		protected int diveBombHorizontalRange = 80;
		protected int diveBombFrameRateLimit = 60;
		protected int diveBombSpeed = 12;
		protected int diveBombInertia = 15;
		protected int approachSpeed = 8;
		protected int approachInertia = 40;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paper Surfer");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 6;
		}

		public override void OnHitTarget(NPC target)
		{
			projectile.velocity.SafeNormalize();
			projectile.velocity *= 6; // "kick" it away from the enemy it just hit
			framesSinceDiveBomb = 0;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int inertia = approachInertia;
			int speed = approachSpeed;

			if (framesSinceDiveBomb++ < diveBombFrameRateLimit || Math.Abs(vectorToTargetPosition.X) > diveBombHorizontalRange)
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
