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

		internal bool idleBumble = true;
		internal int idleBumbleRadius = 128;
		internal int idleBumbleFrames = 60;
		internal float travelFraction;
		internal short bumbleSpriteDirection = 1;
		internal Vector2 bumbleTarget = default;

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
			float rotationMult = 0.8f;
			if (minionCount > 0)
			{
				int order = minions.IndexOf(projectile);
				myIdleAngle = (2 * PI * order) / minionCount;
				idleSyncOffset = (int)(order * ((float)idleBumbleFrames / minionCount));
				rotationMult += (order % 2) * 0.4f;
			}
			int idleSyncFrame = player.GetModPlayer<MinionSpawningItemPlayer>().idleMinionSyncronizationFrame;
			int idleRotationIdx = ((idleSyncFrame + idleSyncOffset) / idleBumbleFrames);
			float groupIdleAngle = idleRotationIdx * MathHelper.Pi * rotationMult;
			Vector2 targetDirection = (myIdleAngle + groupIdleAngle).ToRotationVector2();
			bumbleTarget = player.Top;
			for(int i = 0; i < idleBumbleRadius + 16; i += 16)
			{
				bumbleTarget = i * targetDirection;
				// require some leeway between the nearest block and the turnaround point
				Vector2 nextTarget = bumbleTarget + 32 * targetDirection;
				if(!Collision.CanHit(player.Top + bumbleTarget, 1, 1, player.Top + nextTarget, 1, 1))
				{
					break;
				}
			}
			travelFraction = ((idleSyncFrame + idleSyncOffset) % idleBumbleFrames) / (float)idleBumbleFrames;
			Vector2 vectorToIdlePosition = player.Top + bumbleTarget * travelFraction - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			float maxIdleTravelSpeed = player.velocity.Length() + (idleBumbleRadius * 2) / (float)idleBumbleFrames;
			if(vectorToIdlePosition.LengthSquared() > maxIdleTravelSpeed && Vector2.Distance(projectile.Center, player.Top) < 1.25f * idleBumbleRadius)
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
			if(idleBumble && player.velocity.Length() < 4)
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
			if (vectorToIdlePosition.Length() < maxSpeed)
			{
				projectile.rotation = 0;
				if(idleBumble)
				{
					projectile.spriteDirection = bumbleSpriteDirection * Math.Sign(bumbleTarget.X);
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
