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
		internal virtual LegacySoundStyle ShootSound => null;

		internal virtual int? FiredProjectileId => null;

		internal HoverShooterHelper hsHelper;


		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.friendly = false;
			hsHelper = new HoverShooterHelper(this, FiredProjectileId)
			{
				AfterFiringProjectile = AfterFiringProjectile,
				ExtraAttackConditionsMet = IsMyTurn,
				ModifyTargetVector = ModifyTargetVector
			};
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			hsHelper.TargetedMovement(vectorToTargetPosition);
		}


		internal virtual void AfterFiringProjectile()
		{
			if(ShootSound != null)
			{
				Main.PlaySound(ShootSound, projectile.Center);
			}
		}

		internal void ModifyTargetVector(ref Vector2 target)
		{
			DistanceFromGroup(ref target);
		}
	}

	delegate void ModifyMovementVector(ref Vector2 target);

	public class HoverShooterHelper
	{
		// internal state + config
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
		internal int attackFrames = 60;

		// assume it takes ~ 6 frames for the projectile to hit the target
		internal float leadShotsFraction = 0.167f;
		internal bool inAttackRange;

		private SimpleMinion minion;
		private Projectile projectile => minion.projectile;
		internal int? firedProjectileId;

		// delegate methods
		internal Action AfterFiringProjectile;

		internal Func<bool> ExtraAttackConditionsMet;
		internal ModifyMovementVector ModifyTargetVector;
		internal Action<Vector2, int, float> CustomFireProjectile;



		public HoverShooterHelper(SimpleMinion minion, int? firedProjectileType)
		{
			this.minion = minion;
			this.firedProjectileId = firedProjectileType;
		}

		internal void FireProjectile(Vector2 lineOfFire, int projId, float ai0 = 0)
		{
			Projectile.NewProjectile(
				projectile.Center,
				lineOfFire,
				projId,
				projectile.damage,
				projectile.knockBack,
				Main.myPlayer,
				ai0: ai0);
		}

		public void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int travelSpeed = this.travelSpeed;
			Vector2 lineOfFire = vectorToTargetPosition;
			Vector2 oppositeVector = -vectorToTargetPosition;
			oppositeVector.SafeNormalize();
			float targetDistanceFromFoe = distanceCyle == 1 ? targetInnerRadius : targetOuterRadius;
			if (minion.targetNPCIndex is int targetIdx && Main.npc[targetIdx].active)
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
				inAttackRange = true;
				travelSpeed = travelSpeedAtTarget;
			} else
			{
				inAttackRange = false;
			}
			bool? doAttack = ExtraAttackConditionsMet?.Invoke();
			if ((doAttack is null || doAttack == true) && minion.animationFrame - lastShootFrame >= attackFrames 
				&& vectorToTargetPosition.LengthSquared() < targetShootProximityRadius * targetShootProximityRadius)
			{
				lineOfFire.SafeNormalize();
				lineOfFire *= projectileVelocity;
				if(minion.targetNPCIndex is int idx && Main.npc[idx].active)
				{
					lineOfFire += Main.npc[idx].velocity * leadShotsFraction;
				}
				lastShootFrame = minion.animationFrame;
				if(Main.myPlayer == minion.player.whoAmI && firedProjectileId is int projId)
				{
					(CustomFireProjectile ?? FireProjectile).Invoke(lineOfFire, projId, 0);
				}
				AfterFiringProjectile?.Invoke();
			}
			ModifyTargetVector?.Invoke(ref vectorToTargetPosition);
			if(vectorToTargetPosition.Length() > travelSpeed)
			{
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= travelSpeed;
			}
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
		}
	}
}
