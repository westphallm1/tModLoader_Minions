using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Audio;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets
{
	public class CompanionCubeMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<CompanionCubeMinion>() };
		public override int VanillaBuffId => BuffID.CompanionCube;
		public override string VanillaBuffName => "CompanionCube";
	}

	public class CompanionCubeMinionItem : CombatPetMinionItem<CompanionCubeMinionBuff, CompanionCubeMinion>
	{
		internal override int VanillaItemID => ItemID.CompanionCube;
		internal override string VanillaItemName => "CompanionCube";
		internal override int AttackPatternUpdateTier => (int)CombatPetTier.Spectre;
	}

	public class CompanionCubeMinion : CombatPetSlimeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CompanionCube;
		internal override int BuffId => BuffType<CompanionCubeMinionBuff>();

		internal NPC teleportTarget;
		internal int teleportStartFrame;
		internal float teleportStartAngle;
		internal int teleportDuration = 120;
		internal int teleportCycleFrames = 20;
		private int teleportRadius;
		private float currentAngle;

		internal int teleportFrame => animationFrame - teleportStartFrame;
		internal bool IsTeleporting => teleportTarget != null && teleportTarget.active && teleportFrame < teleportDuration;

		public override void SetDefaults()
		{
			base.SetDefaults();
			CombatPetConvenienceMethods.ConfigureDrawBox(this, 30, 30, 0, -2);
		}

		public override void LoadAssets()
		{
			AddTexture(base.Texture + "Platform");
			Main.instance.LoadProjectile(ProjectileID.PortalGunGate);
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

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if(IsTeleporting)
			{
				damage = (int)(1.25f * damage); // slight damage boost while teleporting
			}
		}

		private void DoTeleportMovement()
		{
			int cycleFrame = teleportFrame % teleportCycleFrames;
			teleportRadius = 64 + (teleportTarget.width + teleportTarget.height) / 4;
			float teleportDistance;
			if(cycleFrame < teleportCycleFrames / 2)
			{
				teleportDistance = teleportRadius * 2f * cycleFrame / teleportCycleFrames;
			} else
			{
				teleportDistance = -teleportRadius + teleportRadius * (2f * cycleFrame / teleportCycleFrames - 1);
			}
			currentAngle = teleportStartAngle + MathHelper.TwoPi * teleportFrame / teleportDuration;
			Vector2 offset = currentAngle.ToRotationVector2() * teleportDistance;
			Vector2 target = teleportTarget.Center + offset;
			// actually teleport rather than going backwards very fast, that has some
			// unintended consequences
			if(cycleFrame == teleportCycleFrames / 2)
			{
				Projectile.Center = target;
				SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.5f }, Projectile.Center);
			} else
			{
				Projectile.velocity = target - Projectile.Center;
			}
			Projectile.tileCollide = false;
		}

		public override void OnHitTarget(NPC target)
		{
			base.OnHitTarget(target);
			// this is less accurate than OnHitNPC + MP sync, but it's easier to write
			if(!IsTeleporting && leveledPetPlayer.PetLevel >= (int)CombatPetTier.Spectre)
			{
				teleportTarget = target;
				teleportStartFrame = animationFrame;
				teleportStartAngle = Projectile.velocity.ToRotation();
			}
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			if(!IsTeleporting)
			{
				return;
			}
			Vector2 portalOffset = currentAngle.ToRotationVector2() * (teleportRadius - 14);
			// always add an orange trail
			Color trailColor = PortalHelper.GetPortalColor(1);
			trailColor.A = byte.MaxValue;
			int cycleFrame = teleportFrame % teleportCycleFrames;
			for(int i = 0; i < 3; i++)
			{
				int dustIdx = Dust.NewDust(Projectile.position, 16, 16, DustID.PortalBoltTrail);
				Main.dust[dustIdx].color = trailColor;
				Main.dust[dustIdx].noLight = true;
				Main.dust[dustIdx].noGravity = true;
				Main.dust[dustIdx].velocity = Projectile.velocity/2f + Utils.RandomVector2(Main.rand, -0.25f, 0.25f);
			}
			for(int sign = -1; sign <= 1; sign++)
			{
				Vector2 portalPosition = teleportTarget.Center + portalOffset;
				Color portalColor = PortalHelper.GetPortalColor(sign == 1 ? 0 : 1);
				Lighting.AddLight(portalPosition, portalColor.ToVector3());
				bool shouldAddDust =
					(sign == 1 && cycleFrame == teleportCycleFrames / 2 - 1) ||
					(sign == -1 && cycleFrame == teleportCycleFrames / 2 + 1);
				if(shouldAddDust)
				{
					portalColor.A = byte.MaxValue;
					for(int i = 0; i < 10; i++)
					{
						int dustIdx = Dust.NewDust(Projectile.position, 24, 24, DustID.PortalBolt);
						Main.dust[dustIdx].color = portalColor;
						Main.dust[dustIdx].noLight = true;
						Main.dust[dustIdx].noGravity = true;
					}
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			if(!IsTeleporting)
			{
				return;
			}
			float r = currentAngle;
			Texture2D texture = ExtraTextures[0].Value;
			Texture2D portalTexture = TextureAssets.Projectile[ProjectileID.PortalGunGate].Value;
			Vector2 offset = currentAngle.ToRotationVector2() * (teleportRadius + 14);
			Vector2 portalOffset = currentAngle.ToRotationVector2() * teleportRadius;
			int portalFrame = (animationFrame / 5) % 4;
			int portalHeight = portalTexture.Height / 4;
			Rectangle portalBounds = new Rectangle(0, portalHeight * portalFrame, portalTexture.Width, portalHeight);
			Vector2 portalOrigin = new Vector2(portalBounds.Width / 2, portalBounds.Height / 2);
			for(int sign = -1; sign <= 1; sign+= 2)
			{
				Vector2 pos = teleportTarget.Center + sign * offset;
				Vector2 portalPos = teleportTarget.Center + sign * portalOffset;
				Color portalColor = PortalHelper.GetPortalColor(sign == 1 ? 0 : 1);
				portalColor.A = byte.MaxValue;
				float portalR = sign == 1 ? r : r + MathHelper.Pi;
				int fadeOutFrame = teleportDuration - teleportCycleFrames / 2;
				float fadeFraction = 2f * (teleportFrame > fadeOutFrame ? teleportDuration - teleportFrame : teleportFrame) / teleportCycleFrames;
				lightColor *= Math.Min(1f, fadeFraction);
				portalColor *= Math.Min(1f, fadeFraction);
				Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
					texture.Bounds, lightColor, r, texture.Bounds.Center.ToVector2(), 1, 0, 0);
				Main.EntitySpriteDraw(portalTexture, portalPos - Main.screenPosition,
					portalBounds, portalColor, portalR, portalOrigin, 1, 0, 0);
			}
		}
	}
}
