using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses
{
	public abstract class CombatPetSlimeMinion : SimpleGroundBasedMinion
	{
		internal LeveledCombatPetModPlayer leveledPetPlayer;
		private float intendedX = 0;
		internal virtual float DamageMult => 1f;
		protected int forwardDir = 1;

		protected bool ShouldBounce => vectorToTarget != null || vectorToIdle.LengthSquared() > 32 * 32;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
			searchDistance = 600;
		}

		protected override bool DoPreStuckCheckGroundedMovement()
		{
			if (!gHelper.didJustLand)
			{
				Projectile.velocity.X = intendedX;
				// only path after landing
				return false;
			}
			return true;
		}

		public override Vector2 IdleBehavior()
		{
			leveledPetPlayer = player.GetModPlayer<LeveledCombatPetModPlayer>();
			Projectile.originalDamage = (int)(DamageMult * leveledPetPlayer.PetDamage);
			searchDistance = leveledPetPlayer.PetLevelInfo.BaseSearchRange;
			return base.IdleBehavior();
		}

		protected override bool CheckForStuckness()
		{
			return true;
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{
			if(!ShouldBounce)
			{
				// slide to a halt
				Projectile.velocity.X *= 0.75f;
				return;
			}
			// always jump "long" if we're far away from the enemy
			if (Math.Abs(vector.X) > startFlyingAtTargetDist && vector.Y < -32)
			{
				vector.Y = -32;
			}
			gHelper.DoJump(vector);
			int baseSpeed = (int)leveledPetPlayer.PetLevelInfo.BaseSpeed;
			int maxHorizontalSpeed = vector.Y < -64 ? baseSpeed/2 : baseSpeed;
			if(targetNPCIndex is int idx && vector.Length() < 64)
			{
				// go fast enough to hit the enemy while chasing them
				Vector2 targetVelocity = Main.npc[idx].velocity;
				Projectile.velocity.X = Math.Max(4, Math.Min(maxHorizontalSpeed, Math.Abs(targetVelocity.X) * 1.25f)) * Math.Sign(vector.X);
			} else
			{
				maxHorizontalSpeed = vector.Y < -64 ? 4 : 8;
				// try to match the player's speed while not chasing an enemy
				Projectile.velocity.X = Math.Max(1, Math.Min(maxHorizontalSpeed, Math.Abs(vector.X) / 16)) * Math.Sign(vector.X);
			}
			intendedX = Projectile.velocity.X;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if (Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = forwardDir;
			}
			else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = -forwardDir;
			}
		}
	}
}
