﻿using AmuletOfManyMinions.CrossModClient.SummonersShine;
using AmuletOfManyMinions.Projectiles.Minions.BoneSerpent;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses
{
	// class hierarchy was a mistake
	public abstract class CombatPetGroundedWormMinion : GroundTravellingWormMinion
	{
		public override int CounterType => -1;
		internal LeveledCombatPetModPlayer leveledPetPlayer;
		// very strong in pre-HM due to wall phasing, nerf a bit there
		internal bool IsPreHM => (leveledPetPlayer?.PetLevel ?? 0) <= 3;
		internal virtual float DamageMult =>  IsPreHM ? 0.95f : 1f;

		internal int MaxHits => 4;
		internal int hitCount;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
			Projectile.tileCollide = false;
			AttackThroughWalls = true;
		}

		public override Vector2 IdleBehavior()
		{
			leveledPetPlayer = Player.GetModPlayer<LeveledCombatPetModPlayer>();
			maxFramesInAir = 50 + 8 * leveledPetPlayer.PetLevel;
			Vector2 target = base.IdleBehavior();
			if(IsPreHM)
			{
				Projectile.localNPCHitCooldown = 25;
			}
			CrossModSetup.CombatPetComputeMinionStats(Projectile, leveledPetPlayer);
			return target;
		}

		// don't grow
		protected override int EmpowerCount => 1;
		internal override int GetSegmentCount() => 6;

		protected override int ComputeDamage() => (int)(DamageMult * leveledPetPlayer.PetDamage);

		protected override float ComputeSearchDistance() => leveledPetPlayer.PetLevelInfo.BaseSearchRange;

		protected override float ComputeInertia() => Math.Max(10, 28 - 2 * leveledPetPlayer.PetLevel);

		protected override float ComputeTargetedSpeed() => leveledPetPlayer.PetLevelInfo.BaseSpeed + (IsPreHM ? 0 : 2);

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}

		public override void OnHitTarget(NPC target)
		{
			base.OnHitTarget(target);
			hitCount++;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			if(vectorToIdlePosition.LengthSquared() < 128 * 128)
			{
				hitCount = 0;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// require that the worm returns to owner between attacks
			if(IsPreHM && hitCount >= MaxHits)
			{
				IdleMovement(VectorToIdle);
			} else
			{
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(IsPreHM && hitCount >= MaxHits)
			{
				return false;
			}
			return base.Colliding(projHitbox, targetHitbox);
		}
	}

	public abstract class CombatPetWormMinion : WormMinion
	{
		public override int CounterType => -1;
		internal LeveledCombatPetModPlayer leveledPetPlayer;
		internal virtual float DamageMult => 1f;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
			Projectile.tileCollide = false;
			AttackThroughWalls = true;
		}

		public override Vector2 IdleBehavior()
		{
			leveledPetPlayer = Player.GetModPlayer<LeveledCombatPetModPlayer>();
			CrossModSetup.CombatPetComputeMinionStats(Projectile, leveledPetPlayer);
			return base.IdleBehavior();
		}

		// Make the worm pet wiggle a bit while the player is walking in the select screen
		internal static void PreviewWormPet(Projectile proj, bool walking)
		{
			var worm = (WormMinion)proj.ModProjectile;
			worm.wormDrawer.SegmentCount = worm.GetSegmentCount();
			Vector2 offset = new(4f, -16f);
			if (walking) {
				offset.Y += 4 * MathF.Sin(MathHelper.TwoPi * proj.position.X / 180);
			}
			worm.wormDrawer.AddPosition(proj.position + offset);
		}

		// don't grow
		protected override int EmpowerCount => 1;
		internal override int GetSegmentCount() => 6;

		protected override int ComputeDamage() => (int)(DamageMult * leveledPetPlayer.PetDamage);

		protected override float ComputeSearchDistance() => leveledPetPlayer.PetLevelInfo.BaseSearchRange;

		protected override float ComputeInertia() => Math.Max(10, 18 - leveledPetPlayer.PetLevel);

		protected override float ComputeTargetedSpeed() => leveledPetPlayer.PetLevelInfo.BaseSpeed + 2;

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}
	}

}
