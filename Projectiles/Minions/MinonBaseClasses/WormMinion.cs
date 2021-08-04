using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	public abstract class WormMinion : EmpoweredMinion
	{
		public int framesSinceLastHit = 0;

		protected virtual int cooldownAfterHitFrames => 16;

		// stopgap to prevent the inexplicable odd behavior when too many segments are created
		// TODO investigate actual causes
		internal static int MAX_SEGMENT_COUNT = 15;

		protected virtual float baseDamageRatio => 0.67f;
		protected virtual float damageGrowthRatio => 0.33f;

		protected WormDrawer wormDrawer;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 1;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 8;
			Projectile.height = 8;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			wormDrawer.Draw(Terraria.GameContent.TextureAssets.Projectile[Projectile.type], lightColor);
			return false;
		}

		protected int GetSegmentCount()
		{
			return Math.Min(EmpowerCount, MAX_SEGMENT_COUNT);
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			wormDrawer.SegmentCount = GetSegmentCount();
			List<Projectile> minions = IdleLocationSets.GetProjectilesInSet(IdleLocationSets.circlingHead, player.whoAmI);
			int minionCount = minions.Count;
			Vector2 idlePosition = player.Top;
			// this was silently failing sometimes, don't know why
			if (minionCount > 0)
			{
				int radius = player.velocity.Length() < 4 ? 48 + 2 * EmpowerCount : 48;
				float yRadius = player.velocity.Length() < 4 ? 8 + 0.5f * EmpowerCount : 8;
				int order = minions.IndexOf(Projectile);
				float idleAngle = (2 * PI * order) / minionCount;
				idleAngle += 2 * PI * groupAnimationFrame / groupAnimationFrames;
				idlePosition.X += radius * (float)Math.Cos(idleAngle);
				idlePosition.Y += -20 + EmpowerCount + yRadius * (float)Math.Sin(idleAngle);
			}
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override void OnHitTarget(NPC target)
		{
			framesSinceLastHit = 0;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			float inertia = ComputeInertia();
			float speed = ComputeTargetedSpeed();
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= speed;
			framesSinceLastHit++;
			if (framesSinceLastHit < cooldownAfterHitFrames && framesSinceLastHit > cooldownAfterHitFrames / 2)
			{
				// start turning so we don't double directly back
				Vector2 turnVelocity = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X) / 8;
				turnVelocity *= Math.Sign(Projectile.velocity.X);
				Projectile.velocity += turnVelocity;
			}
			else if (framesSinceLastHit++ > cooldownAfterHitFrames)
			{
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
			else
			{
				Projectile.velocity.SafeNormalize();
				Projectile.velocity *= speed; // kick it away from enemies that it's just hit
			}
		}

		protected override int ComputeDamage()
		{
			return (int)(baseDamage * baseDamageRatio + baseDamage * damageGrowthRatio * GetSegmentCount());
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 0;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			wormDrawer.AddPosition(Projectile.position);
			wormDrawer.Update(Projectile.frame);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
		}
	}
}
