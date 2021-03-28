using AmuletOfManyMinions.Items.Accessories;
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

		internal bool idleBumble = false;
		internal int idleBumbleRadius = 128;
		internal int idleFrames = 90;

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

		private Vector2 BumblingHeadCircle()
		{
			List<Projectile> minions = GetIdleSpaceSharingMinions();
			int minionCount = minions.Count;
			float myIdleAngle = 0f;
			int idleSyncOffset = 0;
			if (minionCount > 0)
			{
				int order = minions.IndexOf(projectile);
				myIdleAngle = (2 * PI * order) / minionCount;
				idleSyncOffset = (int)(order * ((float)idleFrames / minionCount));
			}
			int idleSyncFrame = player.GetModPlayer<MinionSpawningItemPlayer>().idleMinionSyncronizationFrame;
			float groupIdleAngle = ((idleSyncFrame + idleSyncOffset)/ idleFrames) * MathHelper.Pi * 0.8f;
			Vector2 targetDirection = (myIdleAngle + groupIdleAngle).ToRotationVector2();
			Vector2 target = player.Top;
			for(int i = 0; i < idleBumbleRadius + 16; i += 16)
			{
				target = i * targetDirection;
				// require some leeway between the nearest block and the turnaround point
				Vector2 nextTarget = target + 32 * targetDirection;
				if(!Collision.CanHit(player.Top + target, 1, 1, player.Top + nextTarget, 1, 1))
				{
					break;
				}
			}
			float travelFraction = ((idleSyncFrame + idleSyncOffset) % idleFrames) / (float)idleFrames;
			Vector2 vectorToIdlePosition = player.Top + target * travelFraction - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			float maxIdleTravelSpeed = player.velocity.Length() + (idleBumbleRadius * 2) / (float)idleFrames;
			if(vectorToIdlePosition.LengthSquared() > maxIdleTravelSpeed && Vector2.Distance(projectile.Center, player.Top) < idleBumbleRadius)
			{
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= maxIdleTravelSpeed;
			}
			return vectorToIdlePosition;
		}

		private Vector2 DirectHeadCircle()
		{
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

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			if(idleBumble)
			{
				return BumblingHeadCircle();
			} else
			{
				return DirectHeadCircle();
			}
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
			projectile.tileCollide = false;
			if(idleBumble && vectorToIdlePosition.Length() < 4)
			{
				projectile.velocity = vectorToIdlePosition;
				projectile.spriteDirection = Math.Sign(vectorToIdlePosition.X);
				return;
			}
			if (vectorToIdlePosition.Length() < maxSpeed)
			{
				projectile.rotation = 0;
				if(idleBumble)
				{
					projectile.spriteDirection = Math.Sign(projectile.position.X - player.position.X);
				} else
				{
					projectile.spriteDirection = (idleAngle % (2 * PI)) > PI ? -1 : 1;
				}
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
