using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	public abstract class HoverShooterMinion : HeadCirclingGroupAwareMinion
	{
		internal int lastShootFrame = 0;
		// used to gently bob back and forth between 2 set points from the enemy
		internal int distanceCyle = 1;
		internal int travelSpeed = 10;
		internal int travelSpeedAtTarget = 3;
		internal int projectileVelocity = 14;
		internal int inertia = 10;
		internal int targetMovementProximityRadius = 64;
		internal int targetShootProximityRadius = 64;
		internal int targetInnerRadius = 170;
		internal int targetOuterRadius = 230;

		internal virtual LegacySoundStyle ShootSound => null;

		internal virtual int? FiredProjectileId => null;

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.friendly = false;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int travelSpeed = this.travelSpeed;
			Vector2 lineOfFire = vectorToTargetPosition;
			Vector2 oppositeVector = -vectorToTargetPosition;
			oppositeVector.SafeNormalize();
			float targetDistanceFromFoe = distanceCyle == 1 ? targetInnerRadius : targetOuterRadius;
			if (targetNPCIndex is int targetIdx && Main.npc[targetIdx].active)
			{
				// use the average of the width and height to get an approximate "radius" for the enemy
				NPC npc = Main.npc[targetIdx];
				Rectangle hitbox = npc.Hitbox;
				targetDistanceFromFoe += (hitbox.Width + hitbox.Height) / 4;
			}
			vectorToTargetPosition += targetDistanceFromFoe * oppositeVector;
			// slowly bob back and forth between two radii from the target
			if(vectorToTargetPosition.LengthSquared() < 16 * 16)
			{
				distanceCyle *= -1;
			}
			if(vectorToTargetPosition.LengthSquared() < targetMovementProximityRadius * targetMovementProximityRadius)
			{
				travelSpeed = travelSpeedAtTarget;
			} 
			if (IsMyTurn() &&
				animationFrame - lastShootFrame >= attackFrames &&
				vectorToTargetPosition.LengthSquared() < targetShootProximityRadius * targetShootProximityRadius)
			{
				lineOfFire.SafeNormalize();
				lineOfFire *= projectileVelocity;
				lastShootFrame = animationFrame;
				if(Main.myPlayer == player.whoAmI && FiredProjectileId is int projId)
				{
					FireProjectile(lineOfFire, projId);
				}
				AfterFiringProjectile();
			}
			DistanceFromGroup(ref vectorToTargetPosition);
			if (vectorToTargetPosition.Length() > travelSpeed)
			{
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= travelSpeed;
			}
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
		}

		internal virtual void FireProjectile(Vector2 lineOfFire, int projId)
		{
			Projectile.NewProjectile(
				projectile.Center,
				lineOfFire,
				projId,
				projectile.damage,
				projectile.knockBack,
				Main.myPlayer);
		}

		internal virtual void AfterFiringProjectile()
		{
			if(ShootSound != null)
			{
				Main.PlaySound(ShootSound, projectile.Center);
			}
		}
	}
}
