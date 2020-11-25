using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Squires
{

	public static class SquireMinionTypes
	{
		public static HashSet<int> squireTypes;

		public static void Load()
		{
			squireTypes = new HashSet<int>();
		}

		public static void Unload()
		{
			squireTypes = null;
		}

		public static void Add(int squireType)
		{
			squireTypes.Add(squireType);
		}

		public static bool Contains(int squireType)
		{
			return squireTypes.Contains(squireType);
		}
	}

	public abstract class SquireMinion<T> : SimpleMinion<T> where T : ModBuff
	{
		protected int itemType;


		protected Vector2 relativeVelocity = Vector2.Zero;

		protected virtual float IdleDistanceMulitplier => 1.5f;

		protected bool returningToPlayer = false;

		protected int baseLocalIFrames;

		protected virtual bool travelRangeCanBeModified => true;

		protected virtual bool attackSpeedCanBeModified => true;
		public SquireMinion(int itemID)
		{
			itemType = itemID;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireMinionTypes.Add(projectile.type);
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = false;

			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[projectile.type] = false;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[projectile.type] = false;
			// Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.minion = false;
			projectile.minionSlots = 0;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			baseLocalIFrames = projectile.localNPCHitCooldown;
		}

		public override bool? CanCutTiles()
		{
			return true;
		}

		public override Vector2? FindTarget()
		{
			// move towards the mouse if player is holding and clicking
			if (returningToPlayer || Vector2.Distance(projectile.Center, player.Center) > IdleDistanceMulitplier * ModifiedMaxDistance())
			{
				returningToPlayer = true;
				return null; // force back into non-attacking mode if too far from player
			}
			if (player.HeldItem.type == itemType && player.channel && player.altFunctionUse != 2)
			{
				MousePlayer mPlayer = player.GetModPlayer<MousePlayer>();
				mPlayer.SetMousePosition();
				Vector2? _mouseWorld = mPlayer.GetMousePosition();
				if (_mouseWorld is Vector2 mouseWorld)
				{
					Vector2 targetFromPlayer = mouseWorld - player.Center;
					if (targetFromPlayer.Length() < MaxDistanceFromPlayer())
					{
						return mouseWorld - projectile.Center;
					}
					targetFromPlayer.Normalize();
					targetFromPlayer *= ModifiedMaxDistance();
					return player.Center + targetFromPlayer - projectile.Center;
				}
			}
			return null;
		}

		public override Vector2 IdleBehavior()
		{
			// hover behind the player
			Vector2 idlePosition = player.Top;
			idlePosition.X += 24 * -player.direction;
			idlePosition.Y += -8;
			// not sure what side effects changing this each frame might have
			if(attackSpeedCanBeModified)
			{
				projectile.localNPCHitCooldown = (int)(baseLocalIFrames * player.GetModPlayer<SquireModPlayer>().squireAttackSpeedMultiplier);
			}
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition.X = player.Center.X;
				idlePosition.Y = player.Center.Y - 24;
			}
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// always clamp to the idle position
			float inertia = ComputeInertia();
			float maxSpeed = ModifiedIdleSpeed();
			Vector2 speedChange = vectorToIdlePosition - projectile.velocity;
			if (speedChange.Length() > maxSpeed)
			{
				speedChange.SafeNormalize();
				speedChange *= maxSpeed;
			}
			else
			{
				returningToPlayer = false;
			}
			relativeVelocity = (relativeVelocity * (inertia - 1) + speedChange) / inertia;
			projectile.velocity = player.velocity + relativeVelocity;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (vectorToTargetPosition.Length() < 4 && relativeVelocity.Length() < 1)
			{
				relativeVelocity = Vector2.Zero;
				projectile.velocity = player.velocity;
				return;
			}
			float inertia = ComputeInertia();
			float speed = ModifiedTargetedSpeed();
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= speed;
			relativeVelocity = (relativeVelocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			projectile.velocity = player.velocity + relativeVelocity;
		}

		public float ModifiedTargetedSpeed() => ComputeTargetedSpeed() * player.GetModPlayer<SquireModPlayer>().squireTravelSpeedMultiplier;
		public float ModifiedIdleSpeed() => ComputeIdleSpeed() * player.GetModPlayer<SquireModPlayer>().squireTravelSpeedMultiplier;

		public float ModifiedMaxDistance() => MaxDistanceFromPlayer() + (travelRangeCanBeModified ? player.GetModPlayer<SquireModPlayer>().squireRangeFlatBonus : 0);

		public virtual float ComputeInertia()
		{
			return 12;
		}

		public virtual float ComputeIdleSpeed()
		{
			return 8;
		}

		public virtual float ComputeTargetedSpeed()
		{
			return 8;
		}


		public virtual float MaxDistanceFromPlayer()
		{
			return 80;
		}

	}
}
