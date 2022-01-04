using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets;
using Microsoft.Xna.Framework;
using System;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class WyvernFlyMinionBuff : CombatPetBuff
	{
		public WyvernFlyMinionBuff() : base(ProjectileType<WyvernFlyMinion>()) { }
	}

	public class WyvernFlyMinionItem : CombatPetCustomMinionItem<WyvernFlyMinionBuff, WyvernFlyMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Hasty Bow of Friendship");
			Tooltip.SetDefault("Summons a pet WyvernFly!");
		}
	}

	public class WyvernFlyMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<WyvernFlyMinionBuff>();
		internal override bool DoBumblingMovement => leveledPetPlayer.PetLevel < (int)CombatPetTier.Skeletal;
		internal override int GetAttackFrames(CombatPetLevelInfo info) => 
			(int)(base.GetAttackFrames(info) * (info.Level >= (int)CombatPetTier.Spectre ? 1.5f : 1f));

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 6;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			forwardDir = -1;
			resetIdleRotation = false;
			DrawOriginOffsetX = -16;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			hsHelper.firedProjectileId = (leveledPetPlayer?.PetLevel ?? 0) >= (int)CombatPetTier.Spectre ?
				ProjectileType<TwisterProjectile>() : ProjectileType<CloudPuffProjectile>();
			float idleAngle = (MathHelper.TwoPi * animationFrame % 240) / 240;
			Vector2 idlePosition = player.Center;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, Projectile);
			idlePosition.Y += -35 + 5 * MathF.Sin(idleAngle);
			if (!Collision.CanHit(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition = player.Center;
				idlePosition.X += 30 * -player.direction;
				idlePosition.Y += -35;
			}
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if(vectorToTarget == null && vectorToIdle.LengthSquared() < 32 * 32)
			{
				Projectile.spriteDirection = forwardDir * Math.Sign(player.Center.X - Projectile.Center.X);
			}
		}
	}
}
