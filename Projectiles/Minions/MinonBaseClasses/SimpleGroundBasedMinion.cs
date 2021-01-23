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
	public abstract class SimpleGroundBasedMinion : HeadCirclingGroupAwareMinion
	{
		protected GroundAwarenessHelper gHelper;
		protected int searchDistance = 600;
		protected int lastHitFrame;
		protected int startFlyingAtTargetHeight;
		protected float startFlyingAtTargetDist;
		protected int defaultJumpVelocity;
		protected int maxJumpVelocity;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			IdleLocationSets.trailingOnGround.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			attackFrames = 60;
			noLOSPursuitTime = 300;
			lastHitFrame = -1;
			gHelper = new GroundAwarenessHelper(this)
			{
				ScaleLedge = ScaleLedge,
				CrossCliff = CrossCliff,
				IdleFlyingMovement = IdleFlyingMovement,
				IdleGroundedMovement = IdleGroundedMovement,
				GetUnstuck = GetUnstuck,
				transformRateLimit = 60
			};
		}

		public override Vector2 IdleBehavior()
		{
			gHelper.SetIsOnGround();
			// the ground-based minions can sometimes jump/bounce to get themselves unstuck
			// , but the flying versions can't
			noLOSPursuitTime = gHelper.isFlying ? 15 : 300;
			Vector2 idlePosition = player.Center;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingOnGround, projectile);
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition = player.Center;
			}
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			gHelper.DoIdleMovement(vectorToIdlePosition, vectorToTarget, searchDistance, 180f);
		}

		protected virtual void GetUnstuck(Vector2 destination, int startFrame, ref bool done)
		{
			if(vectorToTarget is null || gHelper.stuckInfo.overCliff)
			{
				Vector2 vectorToUnstuck = destination - projectile.Center;
				if(vectorToUnstuck.Length() < 16)
				{
					done = true;
				} else
				{
					base.IdleMovement(vectorToUnstuck);
				}
			} else
			{
				base.IdleMovement(vectorToIdle);
				if(vectorToIdle.Length() < 16)
				{
					done = true;
				}
			}
		}

		protected virtual void IdleGroundedMovement(Vector2 vector)
		{
			if (vector.Y < -startFlyingAtTargetHeight && Math.Abs(vector.X) < startFlyingAtTargetDist && vectorToTarget != null)
			{
				gHelper.isFlying = true;
				if(gHelper.isFlying)
				{
					IdleFlyingMovement(vector);
				}
				return;
			}
			gHelper.ApplyGravity();
			if(vector.Y > 32 && gHelper.DropThroughPlatform())
			{
				return;
			}
			if (!DoPreStuckCheckGroundedMovement())
			{
				return;
			}
			if(CheckForStuckness())
			{
				StuckInfo info = gHelper.GetStuckInfo(vector);
				if(info.isStuck)
				{
					gHelper.GetUnstuckByTeleporting(info, vector);
				}
			}
			DoGroundedMovement(vector);
		}

		protected virtual bool DoPreStuckCheckGroundedMovement()
		{
			return true;
		}

		protected virtual bool CheckForStuckness()
		{
			return animationFrame % 5 == 0; // by default, only check for stuckness every 5 frames
		}

		protected abstract void DoGroundedMovement(Vector2 vector);

		protected virtual void IdleFlyingMovement(Vector2 vector)
		{
			if (!gHelper.DropThroughPlatform() && animationFrame - lastHitFrame > 15)
			{
				base.IdleMovement(vector);
			}
		}

		public override void OnHitTarget(NPC target)
		{
			lastHitFrame = animationFrame;
		}

		protected virtual bool ScaleLedge(Vector2 vector)
		{
			vector.Y = Math.Min(vector.Y - 32f, (vectorToTarget ?? vectorToIdle).Y);
			gHelper.DoJump(vector, defaultJumpVelocity, maxJumpVelocity);
			return true;
		}

		protected virtual bool CrossCliff(Vector2 vector)
		{
			// always jump the same height, since we don't get info about how wide the gap is
			if(Math.Abs(vector.X) < 32)
			{
				gHelper.DoJump(new Vector2(0, -maxJumpVelocity * 4), defaultJumpVelocity, maxJumpVelocity);
			}
			return true;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			projectile.frameCounter++;
			if(projectile.velocity.X > 1)
			{
				projectile.spriteDirection = 1;
			} else if (projectile.velocity.X < -1)
			{
				projectile.spriteDirection = -1;
			}
			maxFrame = maxFrame ?? Main.projFrames[projectile.type];
			if(projectile.frame < minFrame)
			{
				projectile.frame = minFrame;
			}
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= maxFrame)
				{
					projectile.frame = minFrame;
				}
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			IdleMovement(vectorToTargetPosition);
			projectile.tileCollide = true;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = false;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			gHelper.DoTileCollide(oldVelocity);
			return false;
		}

		public override Vector2? FindTarget()
		{
			if(Vector2.Distance(player.Center, projectile.Center) > 1.5f * searchDistance)
			{
				return null;
			} 
			else if (PlayerTargetPosition(searchDistance, player.Center) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (ClosestEnemyInRange(searchDistance, player.Center) is Vector2 target2)
			{
				return target2 - projectile.Center;
			}
			else
			{
				return null;
			}
		}
	}
}
