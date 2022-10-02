using AmuletOfManyMinions.Core.Minions.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	public interface IGroundedMinion : ISimpleMinion
	{
		GroundAwarenessHelper GHelper { get; }

		void IdleFlyingMovement(Vector2 target);

		void DoGroundedMovement(Vector2 target);

		bool DoPreStuckCheckGroundedMovement();

		// by default, only check for stuckness every 5 frames
		bool CheckForStuckness(); 

		int DefaultJumpVelocity { get; }
		int MaxJumpVelocity { get; }

		int StartFlyingHeight { get; }

		float StartFlyingDist { get; }

		int MaxSpeed { get; }
		int LastHitFrame { get; }
	}

	public class DefaultGroundedBehavior
	{
		private IGroundedMinion Minion { get; set; }

		private SimpleMinionBehavior Behavior => Minion.Behavior;
		private Projectile Projectile => Minion.Projectile;
		private GroundAwarenessHelper GHelper => Minion.GHelper;

		internal int GroundedNoLOSPursuitTime = 300;

		public DefaultGroundedBehavior(IGroundedMinion minion)
		{
			Minion = minion;
		}


		public Vector2 FindIdlePosition()
		{
			GHelper.SetIsOnGround();
			// the ground-based minions can sometimes jump/bounce to get themselves unstuck
			// , but the flying versions can't
			Behavior.NoLOSPursuitTime = GHelper.isFlying ? 15 : GroundedNoLOSPursuitTime;
			Vector2 idlePosition = Minion.Player.Center;
			idlePosition.X += -Minion.Player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingOnGround, Projectile);
			if (!Collision.CanHitLine(idlePosition, 1, 1, Minion.Player.Center, 1, 1))
			{
				idlePosition = Minion.Player.Center;
			}
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			Minion.Behavior.TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public void GetUnstuck(Vector2 destination, int startFrame, ref bool done)
		{
			if (Behavior.VectorToTarget is null || GHelper.stuckInfo.overCliff)
			{
				Vector2 vectorToUnstuck = destination - Projectile.Center;
				if (vectorToUnstuck.Length() < 16)
				{
					done = true;
				}
				else
				{
					Minion.IdleFlyingMovement(vectorToUnstuck);
				}
			}
			else
			{
				Minion.IdleFlyingMovement(Behavior.VectorToIdle);
				if (Behavior.VectorToIdle.LengthSquared() < 16 * 16)
				{
					done = true;
				}
			}
		}

		public bool ScaleLedge(Vector2 vector)
		{
			vector.Y = Math.Min(vector.Y - 32f, (Behavior.VectorToTarget ?? Behavior.VectorToIdle).Y);
			GHelper.DoJump(vector, Minion.DefaultJumpVelocity, Minion.MaxJumpVelocity);
			return true;
		}

		public virtual bool CrossCliff(Vector2 vector)
		{
			// always jump the same height, since we don't get info about how wide the gap is
			if (Math.Abs(vector.X) < 32)
			{
				GHelper.DoJump(new Vector2(0, -Minion.MaxJumpVelocity * 4), Minion.DefaultJumpVelocity, Minion.MaxJumpVelocity);
			}
			return true;
		}

		public void DefaultGroundedMovement(Vector2 vector)
		{
			if (vector.Y < -3 * Projectile.height && Math.Abs(vector.X) < Minion.StartFlyingHeight)
			{
				GHelper.DoJump(vector);
			}
			float xInertia = GHelper.stuckInfo.overLedge && !GHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 1.25f : 8;
			if (Behavior.VectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = Minion.Player.velocity.X;
				return;
			}
			Behavior.DistanceFromGroup(ref vector);
			if (Behavior.AnimationFrame - Minion.LastHitFrame > Math.Max(6, 90 / Minion.MaxSpeed))
			{
				Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * Minion.MaxSpeed) / xInertia;
			}
			else
			{
				Projectile.velocity.X = Math.Sign(Projectile.velocity.X) * Minion.MaxSpeed * 0.75f;
			}
		}

		public void DetermineIdleGroundedState(Vector2 vector)
		{
			if (vector.Y < -Minion.StartFlyingHeight && Math.Abs(vector.X) < Minion.StartFlyingDist && Behavior.VectorToTarget != null)
			{
				GHelper.isFlying = true;
				if (GHelper.isFlying)
				{
					Minion.IdleFlyingMovement(vector);
				}
				return;
			}
			GHelper.ApplyGravity();
			if (vector.Y > 32 && GHelper.DropThroughPlatform())
			{
				return;
			}
			if (!Minion.DoPreStuckCheckGroundedMovement())
			{
				return;
			}
			if (Minion.CheckForStuckness())
			{
				StuckInfo info = GHelper.GetStuckInfo(vector);
				if (info.isStuck)
				{
					GHelper.GetUnstuckByTeleporting(info, vector);
				}
			}
			Minion.DoGroundedMovement(vector);
		}
	}

	public abstract class SimpleGroundBasedMinion : HeadCirclingGroupAwareMinion, IGroundedMinion
	{
		public GroundAwarenessHelper GHelper { get; set; }
		protected int searchDistance = 600;
		public int LastHitFrame { get; set; }
		public int StartFlyingHeight { get; set; }
		public float StartFlyingDist { get; set; }
		public int DefaultJumpVelocity { get; set; }
		public int MaxJumpVelocity { get; set; }
		
		private DefaultGroundedBehavior GroundedBehavior { get; set; }

		protected int xMaxSpeed;

		internal Dictionary<GroundAnimationState, (int, int?)> frameInfo;


		public int MaxSpeed => xMaxSpeed;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			IdleLocationSets.trailingOnGround.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			attackFrames = 60;
			NoLOSPursuitTime = 300;
			LastHitFrame = -1;
			StartFlyingHeight = 96;
			StartFlyingDist = 64;
			DefaultJumpVelocity = 4;
			MaxJumpVelocity = 12;
			GroundedBehavior = new(this);
			GHelper = new GroundAwarenessHelper(this)
			{
				ScaleLedge = ScaleLedge,
				CrossCliff = CrossCliff,
				IdleFlyingMovement = IdleFlyingMovement,
				IdleGroundedMovement = IdleGroundedMovement,
				GetUnstuck = GetUnstuck,
				transformRateLimit = 60
			};
			Pathfinder.modifyPath = GHelper.ModifyPathfinding;
		}

		public override Vector2 IdleBehavior()
		{
			GHelper.SetIsOnGround();
			return GroundedBehavior.FindIdlePosition();
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition) =>
			GHelper.DoIdleMovement(vectorToIdlePosition, VectorToTarget, searchDistance, 180f);

		protected virtual void GetUnstuck(Vector2 destination, int startFrame, ref bool done) => 
			GroundedBehavior.GetUnstuck(destination, startFrame, ref done);

		protected virtual void IdleGroundedMovement(Vector2 vector) =>
			GroundedBehavior.DetermineIdleGroundedState(vector);

		protected virtual bool DoPreStuckCheckGroundedMovement() => true;

		protected virtual bool CheckForStuckness() => AnimationFrame % 5 == 0; // by default, only check for stuckness every 5 frames

		protected abstract void DoGroundedMovement(Vector2 vector);

		internal void DoDefaultGroundedMovement(Vector2 vector) =>
			GroundedBehavior.DefaultGroundedMovement(vector);

		protected virtual void IdleFlyingMovement(Vector2 vector)
		{
			if (!GHelper.DropThroughPlatform() && AnimationFrame - LastHitFrame > 15)
			{
				base.IdleMovement(vector);
			}
		}

		public override void OnHitTarget(NPC target)
		{
			LastHitFrame = AnimationFrame;
		}

		protected virtual bool ScaleLedge(Vector2 vector) => GroundedBehavior.ScaleLedge(vector);

		protected virtual bool CrossCliff(Vector2 vector) => GroundedBehavior.CrossCliff(vector);

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			Projectile.frameCounter++;
			if (Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = 1;
			}
			else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = -1;
			}
			maxFrame = maxFrame ?? Main.projFrames[Projectile.type];
			if (Projectile.frame < minFrame)
			{
				Projectile.frame = minFrame;
			}
			if (Projectile.frameCounter >= FrameSpeed)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= maxFrame)
				{
					Projectile.frame = minFrame;
				}
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			IdleMovement(vectorToTargetPosition);
			Projectile.tileCollide = true;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = false;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			GHelper.DoTileCollide(oldVelocity);
			return false;
		}

		public override Vector2? FindTarget()
		{
			if (Vector2.Distance(Player.Center, Projectile.Center) > 1.5f * searchDistance)
			{
				return null;
			}
			else if (PlayerTargetPosition(searchDistance, Player.Center) is Vector2 target)
			{
				return target - Projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance) is Vector2 target2)
			{
				return target2 - Projectile.Center;
			}
			else
			{
				return null;
			}
		}

		protected void DoSimpleFlyingDust(int dustId = 16)
		{
			if(GHelper.isFlying)
			{
				Projectile.rotation = Projectile.velocity.X * 0.05f;
				int idx = Dust.NewDust(Projectile.Bottom, 8, 8, dustId, -Projectile.velocity.X / 2, -Projectile.velocity.Y / 2);
				Main.dust[idx].alpha = 112;
				Main.dust[idx].scale = .9f;
			} else
			{
				Projectile.rotation = 0;
			}
		}

		protected void ConfigureFrames(int total, (int, int) idle, (int, int) walking, (int, int) jumping, (int, int) flying)
		{
			// this should really go into SetStaticDefaults, but we're trying to condense
			// things as much as possible
			Main.projFrames[Projectile.type] = total;
			frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
			{
				[GroundAnimationState.STANDING] = idle,
				[GroundAnimationState.WALKING] = walking,
				[GroundAnimationState.JUMPING] = jumping,
				[GroundAnimationState.FLYING] = flying,
			};
		}

		// This is a little hacky
		void IGroundedMinion.IdleFlyingMovement(Vector2 target) => IdleFlyingMovement(target);

		void IGroundedMinion.DoGroundedMovement(Vector2 target) => DoGroundedMovement(target);

		bool IGroundedMinion.DoPreStuckCheckGroundedMovement() => DoPreStuckCheckGroundedMovement();

		bool IGroundedMinion.CheckForStuckness() => CheckForStuckness();
	}
}
