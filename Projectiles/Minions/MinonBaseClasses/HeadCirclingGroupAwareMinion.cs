using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	public abstract class HeadCirclingGroupAwareMinion: GroupAwareMinion
	{
		protected float idleAngle;
		protected int targetSearchDistance = 800;
		protected int idleCircle = 40;
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
			animationFrames = 240;
			projectile.localNPCHitCooldown = 20;
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
