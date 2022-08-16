using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI.ManagedAI
{
	internal class GroundedCrossModAI : BaseGroundedCrossModAI
	{
		internal int PreferredTargetDist { get; set; } = 128;
		internal int LaunchVelocity { get; set; } = 12;
		internal int AttackFrames { get; set; }
		internal int LastFiredFrame { get; set; }

		internal virtual bool ShouldDoShootingMovement => FiredProjectileId != null;
		public GroundedCrossModAI(Projectile proj, int buffId, int? projId) : base(proj, buffId, projId)
		{
		}

		internal override void UpdatePetState()
		{
			base.UpdatePetState();
			var leveledPetPlayer = Player.GetModPlayer<LeveledCombatPetModPlayer>();
			var info = CombatPetLevelTable.PetLevelTable[leveledPetPlayer.PetLevel];
			AttackFrames = Math.Max(30, 60 - 6 * info.Level);
		}

		public void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			if(FiredProjectileId is not int projId) { return; }
			launchVector *= 1.15f;
			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				Behavior.VaryLaunchVelocity(launchVector),
				projId,
				Projectile.damage,
				Projectile.knockBack,
				Player.whoAmI,
				ai0: ai0 ?? Projectile.whoAmI);
		}

		public override void DoGroundedMovement(Vector2 vector)
		{
			if(!ShouldDoShootingMovement)
			{
				base.DoGroundedMovement(vector);
				return;
			}
			if (vector.Y < -3 * Projectile.height && Math.Abs(vector.X) < StartFlyingHeight)
			{
				GHelper.DoJump(vector);
			}
			if (Behavior.VectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = Player.velocity.X;
				return;
			}
			Behavior.DistanceFromGroup(ref vector);
			// only change speed if the target is a decent distance away
			if (Math.Abs(vector.X) < 4 && Behavior.TargetNPCIndex is int idx && Math.Abs(Main.npc[idx].velocity.X) < 7)
			{
				Projectile.velocity.X = Main.npc[idx].velocity.X;
			}
			else
			{
				Projectile.velocity.X = (Projectile.velocity.X * (Inertia - 1) + Math.Sign(vector.X) * MaxSpeed) / Inertia;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(!ShouldDoShootingMovement)
			{
				base.TargetedMovement(vectorToTargetPosition);
				return;
			}
			bool inLaunchRange = 
				Math.Abs(vectorToTargetPosition.X) < 4 * PreferredTargetDist &&
				Math.Abs(vectorToTargetPosition.Y) < 4 * PreferredTargetDist;
			if (Player.whoAmI == Main.myPlayer && inLaunchRange && Behavior.AnimationFrame - LastFiredFrame >= AttackFrames)
			{
				LastFiredFrame = Behavior.AnimationFrame;
				Vector2 launchVector = vectorToTargetPosition;
				// lead shot a little bit
				if(Behavior.TargetNPCIndex is int idx && Main.npc[idx] is NPC target)
				{
					launchVector += target.velocity * 0.167f;
				}
				launchVector.SafeNormalize();
				launchVector *= LaunchVelocity;
				LaunchProjectile(launchVector);
			}

			// don't move if we're in range-ish of the target
			if (Math.Abs(vectorToTargetPosition.X) < 1.25f * PreferredTargetDist && 
				Math.Abs(vectorToTargetPosition.X) > 0.5f * PreferredTargetDist)
			{
				vectorToTargetPosition.X = 0;
			} else if (Math.Abs(vectorToTargetPosition.X) < 0.5f * PreferredTargetDist)
			{
				vectorToTargetPosition.X -= Math.Sign(vectorToTargetPosition.X) * 0.75f * PreferredTargetDist;
			}

			if(Math.Abs(vectorToTargetPosition.Y) < 1.25f * PreferredTargetDist && 
				Math.Abs(vectorToTargetPosition.Y) > 0.5 * PreferredTargetDist)
			{
				vectorToTargetPosition.Y = 0;
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			Projectile.friendly &= !ShouldDoShootingMovement;
			// having a slightly positive velocity from constant gravity messes with the vanilla frame
			// determination
			// This occurs after the velocity cache, so it should be ignored for actual calculations
			if(Projectile.velocity.Y == 0.5f)
			{
				Projectile.velocity.Y = 0;
			}
		}
	}
}
