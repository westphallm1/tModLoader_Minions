using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{

	public abstract class TeleportingWeaponMinion<T> : GroupAwareMinion<T> where T: ModBuff
	{

		internal int framesInAir;
		internal float idleAngle;
		protected virtual int maxFramesInAir => 60;
		protected virtual Vector3 lightColor => default;
		internal float travelVelocity;
		internal NPC targetNPC = null;
		internal float distanceFromFoe = default;
		internal float teleportAngle = default;
		internal Vector2 teleportDirection;
		internal int phaseFrames;
		internal int maxPhaseFrames = 60;
		internal int lastHitFrame = 0;
		internal int framesWithoutTarget;
		internal bool targetIsDead;
		internal bool lastActive;


		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			List<Projectile> minions = GetActiveMinions();
			Vector2 idlePosition = player.Center;
			int minionCount = minions.Count;
			int order = minions.IndexOf(projectile);
			idleAngle = (float)(MathHelper.TwoPi * order) / minionCount;
			idleAngle += (MathHelper.TwoPi * minions[0].ai[1]) / animationFrames;
			idlePosition.X += 2 + 30 * (float)Math.Cos(idleAngle);
			idlePosition.Y += -12 + 5 * (float)Math.Sin(idleAngle);
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override Vector2? FindTarget()
		{
			if (FindTargetInTurnOrder(800f, projectile.Center, 600f) is Vector2 target)
			{
				framesWithoutTarget = 0;
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

			if (targetNPC is null && targetNPCIndex is int index)
			{
				OnAcquireTarget(vectorToTargetPosition);
				targetNPC = Main.npc[index];
				
				distanceFromFoe = default;
				phaseFrames = 0;
				framesInAir = 0;
				lastHitFrame = -10;
				targetIsDead = false;
				lastActive = true;
			} else if(!targetIsDead && targetNPC != null && lastActive && !targetNPC.active)
			{
				phaseFrames = maxPhaseFrames;
				targetIsDead = true;
				lastActive = false;
				OnLoseTarget(ref vectorToTargetPosition);
			}

			if (targetNPC != null && phaseFrames++ < maxPhaseFrames / 2)
			{
				// do nothing, preDraw will do phase out animation
				IdleMovement(vectorToIdle);
			}
			else if (phaseFrames > maxPhaseFrames / 2 && phaseFrames < maxPhaseFrames)
			{
				WindUpBehavior(ref vectorToTargetPosition);
				projectile.friendly = false;
			} else
			{
				SwingBehavior(ref vectorToTargetPosition);
				projectile.friendly = true;
			}
			Lighting.AddLight(projectile.Center, lightColor);
		}

		internal virtual void OnAcquireTarget(Vector2 vectorToTargetPosition)
		{
		}

		public abstract void WindUpBehavior(ref Vector2 vectorToTargetPosition);
		public abstract void SwingBehavior(ref Vector2 vectorToTargetPosition);

		public virtual void OnLoseTarget(ref Vector2 vectorToTargetPosition)
		{

		}

		public override void OnHitTarget(NPC target)
		{
			lastHitFrame = framesInAir;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (attackState == AttackState.ATTACKING)
			{
				framesWithoutTarget++;
				if (phaseFrames < maxPhaseFrames && targetNPC != null)
				{
					TargetedMovement(targetNPC.Center - projectile.Center);
				}
				else
				{
					TargetedMovement(projectile.velocity);
				}
				return;
			}
			projectile.rotation = (float)Math.PI;
			// alway clamp to the idle position
			projectile.tileCollide = false;

			if (vectorToIdlePosition.Length() > 32 && vectorToIdlePosition.Length() < 1000)
			{
				projectile.position += vectorToIdlePosition;
			}
			else
			{
				attackState = AttackState.IDLE;
				projectile.rotation = (player.Center - projectile.Center).X * -0.01f;
				projectile.position += vectorToIdlePosition;
				projectile.velocity = Vector2.Zero;
			}
		}
	}
}
