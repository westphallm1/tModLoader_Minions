using AmuletOfManyMinions.Core.Minions.AI;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI.ManagedAI
{
	internal abstract class BaseGroundedCrossModAI : GroupAwareCrossModAI, ICrossModSimpleMinion, IGroundedMinion
	{

		public GroundAwarenessHelper GHelper { get; set; }
		protected DefaultGroundedBehavior GroundedBehavior { get; set; }
		public int LastHitFrame { get; set; } = -1;
		public int StartFlyingHeight { get; set; } = 96;
		public float StartFlyingDist { get; set; } = 64;
		public int DefaultJumpVelocity { get; set; } = 4;
		public int MaxJumpVelocity { get; set; } = 12;

		public BaseGroundedCrossModAI(Projectile proj, int buffId, int? projId, bool isPet) : base(proj, buffId, projId, isPet)
		{
			IdleLocationSets.trailingOnGround.Add(Projectile.type);
			Behavior.NoLOSPursuitTime = 300;
			GroundedBehavior = new(this);
			GHelper = new GroundAwarenessHelper(this)
			{
				IdleFlyingMovement = IdleFlyingMovement,
				ScaleLedge = GroundedBehavior.ScaleLedge,
				CrossCliff = GroundedBehavior.CrossCliff,
				IdleGroundedMovement = GroundedBehavior.DetermineIdleGroundedState,
				GetUnstuck = GroundedBehavior.GetUnstuck,
				transformRateLimit = 60
			};
			Behavior.Pathfinder.modifyPath = GHelper.ModifyPathfinding;
			if(IsPet) { ApplyPetDefaults(); }
		}


		public override Vector2 IdleBehavior()
		{
			if(IsPet) { UpdatePetState(); }
			return GroundedBehavior.FindIdlePosition();
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition) =>
			GHelper.DoIdleMovement(vectorToIdlePosition, Behavior.VectorToTarget, SearchRange, 180f);

		public virtual bool DoPreStuckCheckGroundedMovement() => true;

		// by default, only check for stuckness every 5 frames
		public virtual bool CheckForStuckness() => Behavior.AnimationFrame % 5 == 0;

		public virtual void DoGroundedMovement(Vector2 vector) => GroundedBehavior.DefaultGroundedMovement(vector);

		public virtual void IdleFlyingMovement(Vector2 vector)
		{
			if (!(vector.Y > 8 && GHelper.DropThroughPlatform()) && Behavior.AnimationFrame - LastHitFrame > 15)
			{
				base.IdleMovement(vector);
			}
			// ensure that the minion is in its flying animation while flying
			if(Behavior.IsFollowingBeacon || GHelper.isFlying)
			{
				FakePlayerFlyingHeight();
			}
		}

		public void OnHitTarget(NPC target)
		{
			LastHitFrame = Behavior.AnimationFrame;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			IdleMovement(vectorToTargetPosition);
			Projectile.tileCollide = true;
		}

		public void OnTileCollide(Vector2 oldVelocity) => GHelper.DoTileCollide(oldVelocity);
	}
}
