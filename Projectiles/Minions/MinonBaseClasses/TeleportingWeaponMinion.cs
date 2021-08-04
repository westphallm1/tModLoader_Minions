using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{

	public abstract class TeleportingWeaponMinion : GroupAwareMinion
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

		protected virtual int searchDistance => 800;
		protected virtual int noLOSSearchDistance => 600;


		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 1;
			IdleLocationSets.circlingBody.Add(Projectile.type);
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			List<Projectile> minions = IdleLocationSets.GetProjectilesInSet(IdleLocationSets.circlingBody, player.whoAmI);
			Vector2 idlePosition = player.Center;
			if (minions.Count > 0)
			{
				int minionCount = minions.Count;
				int order = minions.IndexOf(Projectile);
				idleAngle = (float)(MathHelper.TwoPi * order) / minionCount;
				idleAngle += (MathHelper.TwoPi * groupAnimationFrame) / groupAnimationFrames;
				idlePosition.X += 2 + 30 * (float)Math.Cos(idleAngle);
				idlePosition.Y += -12 + 5 * (float)Math.Sin(idleAngle);
			}
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override Vector2? FindTarget()
		{
			if (FindTargetInTurnOrder(searchDistance, Projectile.Center, noLOSSearchDistance) is Vector2 target)
			{
				framesWithoutTarget = 0;
				return target;
			}
			else
			{
				Projectile.friendly = false;
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
			}
			else if (!targetIsDead && targetNPC != null && lastActive && !targetNPC.active)
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
				Projectile.friendly = false;
			}
			else
			{
				SwingBehavior(ref vectorToTargetPosition);
				Projectile.friendly = true;
			}
			Lighting.AddLight(Projectile.Center, lightColor);
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
					TargetedMovement(targetNPC.Center - Projectile.Center);
				}
				else
				{
					TargetedMovement(Projectile.velocity);
				}
				return;
			}
			Projectile.rotation = (float)Math.PI;
			// alway clamp to the idle position
			Projectile.tileCollide = false;

			if (vectorToIdlePosition.Length() > 32 && vectorToIdlePosition.Length() < 1000)
			{
				Projectile.position += vectorToIdlePosition;
			}
			else
			{
				attackState = AttackState.IDLE;
				Projectile.rotation = (player.Center - Projectile.Center).X * -0.01f;
				Projectile.position += vectorToIdlePosition;
				Projectile.velocity = Vector2.Zero;
			}
		}
	}
}
