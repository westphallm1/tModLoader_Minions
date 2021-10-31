using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
using System;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets
{
	public class CompanionCubeMinionBuff : CombatPetVanillaCloneBuff
	{
		public CompanionCubeMinionBuff() : base(ProjectileType<CompanionCubeMinion>()) { }
		public override int VanillaBuffId => BuffID.CompanionCube;
		public override string VanillaBuffName => "CompanionCube";
	}

	public class CompanionCubeMinionItem : CombatPetMinionItem<CompanionCubeMinionBuff, CompanionCubeMinion>
	{
		internal override int VanillaItemID => ItemID.CompanionCube;
		internal override string VanillaItemName => "CompanionCube";
	}

	public class CompanionCubeMinion : CombatPetSlimeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CompanionCube;
		internal override int BuffId => BuffType<CompanionCubeMinionBuff>();

		internal NPC teleportTarget;
		internal int teleportStartFrame;
		internal float teleportStartAngle;
		internal int teleportDuration = 90;
		internal int teleportCycleFrames = 30;
		internal int teleportFrame => animationFrame - teleportStartFrame;
		internal bool IsTeleporting => teleportTarget != null && teleportTarget.active && teleportFrame < teleportDuration;

		public override void SetDefaults()
		{
			base.SetDefaults();
			CombatPetConvenienceMethods.ConfigureDrawBox(this, 30, 30, 0, 0);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(ShouldBounce)
			{
				Projectile.rotation += MathHelper.Pi / 15 * Math.Sign(Projectile.velocity.X);
			} else
			{
				Projectile.rotation = Utils.AngleLerp(Projectile.rotation, 0, 0.25f);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(IsTeleporting)
			{
				// TODO
				DoTeleportMovement();
			} else
			{
				teleportTarget = null;
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		private void DoTeleportMovement()
		{
			int cycleFrame = teleportFrame % teleportCycleFrames;
			float teleportRadius = 64 + (teleportTarget.width + teleportTarget.height) / 4;
			float teleportDistance;
			if(cycleFrame < teleportCycleFrames / 2)
			{
				teleportDistance = teleportRadius * 2f * cycleFrame / teleportCycleFrames;
			} else
			{
				teleportDistance = -teleportRadius + teleportRadius * (2f * cycleFrame / teleportCycleFrames - 1);
			}
			float currentAngle = teleportStartAngle + MathHelper.TwoPi * teleportFrame / teleportDuration;
			Vector2 offset = currentAngle.ToRotationVector2() * teleportDistance;
			Vector2 target = teleportTarget.Center + offset;
			Projectile.velocity = target - Projectile.position;
			Projectile.tileCollide = false;
		}

		public override void OnHitTarget(NPC target)
		{
			base.OnHitTarget(target);
			// this is less accurate than OnHitNPC + MP sync, but it's easier to write
			if(!IsTeleporting)
			{
				teleportTarget = target;
				teleportStartFrame = animationFrame;
				teleportStartAngle = Projectile.velocity.ToRotation();
			}
		}

	}
}
