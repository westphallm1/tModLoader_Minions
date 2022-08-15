using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI.ManagedAI
{
	// TODO this is a lot more verbose than the flying AI copy
	internal abstract class BaseGroundedCrossModAI : GroupAwareCrossModAI, ICrossModSimpleMinion
	{

		protected GroundAwarenessHelper GHelper { get; set; }
		protected int LastHitFrame { get; set; } = -1;
		protected int StartFlyingHeight { get; set; } = 96;
		protected float StartFlyingDist { get; set; } = 64;
		protected int DefaultJumpVelocity { get; set; } = 4;
		protected int MaxJumpVelocity { get; set; } = 12;

		public BaseGroundedCrossModAI(Projectile proj, int buffId, int? projId) : base(proj, buffId, projId)
		{
			IdleLocationSets.trailingOnGround.Add(Projectile.type);
			Behavior.NoLOSPursuitTime = 300;
			GHelper = new GroundAwarenessHelper(this)
			{
				ScaleLedge = ScaleLedge,
				CrossCliff = CrossCliff,
				IdleFlyingMovement = IdleFlyingMovement,
				IdleGroundedMovement = IdleGroundedMovement,
				GetUnstuck = GetUnstuck,
				transformRateLimit = 60
			};
			Behavior.Pathfinder.modifyPath = GHelper.ModifyPathfinding;
			if(IsPet) { ApplyPetDefaults(); }
		}

		public override Vector2 IdleBehavior()
		{
			if(IsPet) { UpdatePetState(); }
			GHelper.SetIsOnGround();
			// the ground-based minions can sometimes jump/bounce to get themselves unstuck
			// , but the flying versions can't
			Behavior.NoLOSPursuitTime = GHelper.isFlying ? 15 : 300;
			Vector2 idlePosition = Player.Center;
			idlePosition.X += -Player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingOnGround, Projectile);
			if (!Collision.CanHitLine(idlePosition, 1, 1, Player.Center, 1, 1))
			{
				idlePosition = Player.Center;
			}
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			Behavior.TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			GHelper.DoIdleMovement(vectorToIdlePosition, Behavior.VectorToTarget, SearchRange, 180f);
		}

		protected virtual void GetUnstuck(Vector2 destination, int startFrame, ref bool done)
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
					base.IdleMovement(vectorToUnstuck);
				}
			}
			else
			{
				base.IdleMovement(Behavior.VectorToIdle);
				if (Behavior.VectorToIdle.LengthSquared() < 16 * 16)
				{
					done = true;
				}
			}
		}

		protected virtual void IdleGroundedMovement(Vector2 vector)
		{
			if (vector.Y < -StartFlyingHeight && Math.Abs(vector.X) < StartFlyingDist && Behavior.VectorToTarget != null)
			{
				GHelper.isFlying = true;
				if (GHelper.isFlying)
				{
					IdleFlyingMovement(vector);
				}
				return;
			}
			GHelper.ApplyGravity();
			if (vector.Y > 32 && GHelper.DropThroughPlatform())
			{
				return;
			}
			if (!DoPreStuckCheckGroundedMovement())
			{
				return;
			}
			if (CheckForStuckness())
			{
				StuckInfo info = GHelper.GetStuckInfo(vector);
				if (info.isStuck)
				{
					GHelper.GetUnstuckByTeleporting(info, vector);
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
			return Behavior.AnimationFrame % 5 == 0; // by default, only check for stuckness every 5 frames
		}

		internal virtual void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -3 * Projectile.height && Math.Abs(vector.X) < StartFlyingHeight)
			{
				GHelper.DoJump(vector);
			}
			float xInertia = GHelper.stuckInfo.overLedge && !GHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 1.25f : 8;
			if (Behavior.VectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = Player.velocity.X;
				return;
			}
			Behavior.DistanceFromGroup(ref vector);
			if (Behavior.AnimationFrame - LastHitFrame > Math.Max(6, 90 / MaxSpeed))
			{
				Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * MaxSpeed) / xInertia;
			}
			else
			{
				Projectile.velocity.X = Math.Sign(Projectile.velocity.X) * MaxSpeed * 0.75f;
			}
		}

		protected virtual void IdleFlyingMovement(Vector2 vector)
		{
			if (!GHelper.DropThroughPlatform() && Behavior.AnimationFrame - LastHitFrame > 15)
			{
				base.IdleMovement(vector);
			}
			// ensure that the minion is in its flying animation while flying
			if(Behavior.IsFollowingBeacon)
			{
				FakePlayerFlyingHeight();
			}
		}

		public void OnHitTarget(NPC target)
		{
			LastHitFrame = Behavior.AnimationFrame;
		}

		protected virtual bool ScaleLedge(Vector2 vector)
		{
			vector.Y = Math.Min(vector.Y - 32f, (Behavior.VectorToTarget ?? Behavior.VectorToIdle).Y);
			GHelper.DoJump(vector, DefaultJumpVelocity, MaxJumpVelocity);
			return true;
		}

		protected virtual bool CrossCliff(Vector2 vector)
		{
			// always jump the same height, since we don't get info about how wide the gap is
			if (Math.Abs(vector.X) < 32)
			{
				GHelper.DoJump(new Vector2(0, -MaxJumpVelocity * 4), DefaultJumpVelocity, MaxJumpVelocity);
			}
			return true;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			IdleMovement(vectorToTargetPosition);
			Projectile.tileCollide = true;
		}

		public void OnTileCollide(Vector2 oldVelocity) => GHelper.DoTileCollide(oldVelocity);
	}
}
