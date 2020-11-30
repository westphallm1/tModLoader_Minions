using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Squires
{
	public abstract class SquireBoomerangMinion : SquireAccessoryMinion
	{
		protected bool returning = false;
		protected int? returnedToHeadFrame = -10;

		protected abstract int idleVelocity { get; }
		protected abstract int targetedVelocity { get; }
		protected abstract int inertia { get; }
		protected abstract int attackRange { get; }
		protected abstract int attackCooldown { get; }

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.penetrate = -1;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 30;
			attackThroughWalls = true;
		}
		public override Vector2 IdleBehavior()
		{
			if(squire != null)
			{
				projectile.damage = Math.Max(1, 5 * squire.damage / 6);
			}
			Vector2 crownOffset = new Vector2(0, -14);
			crownOffset.Y += 2 * (float)Math.Sin(2 * Math.PI * animationFrame / 60f);
			return base.IdleBehavior() + crownOffset;
		}

		protected override bool IsEquipped(SquireModPlayer player)
		{
			return player.royalArmorSetEquipped;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (vectorToIdlePosition.Length() > 32)
			{
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= idleVelocity;
				projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
			} else
			{
				returnedToHeadFrame = returnedToHeadFrame ?? animationFrame;
				projectile.position += vectorToIdlePosition;
				projectile.velocity = Vector2.Zero;
				returning = false;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			vectorToTargetPosition.Normalize();
			vectorToTargetPosition *= targetedVelocity;
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
		}

		public override Vector2? FindTarget()
		{
			if(SquireAttacking() &&
				returnedToHeadFrame is int frame &&
				animationFrame - frame > attackCooldown && 
				!returning &&
				ClosestEnemyInRange(attackRange, maxRangeFromPlayer: false) is Vector2 target)
			{
				projectile.friendly = true;
				projectile.tileCollide = true;
				return target - projectile.Center;
			}
			projectile.friendly = false;
			projectile.tileCollide = false;
			return null;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			returnedToHeadFrame = null;
			returning = true;
		}

		public override void OnHitTarget(NPC target)
		{
			if(player.whoAmI != Main.myPlayer)
			{
				returnedToHeadFrame = null;
				returning = true;
			}
		}
	}
}
