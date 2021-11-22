using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using AmuletOfManyMinions.Projectiles.Squires.SoulboundArsenal;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class MoonlingMinionBuff : CombatPetVanillaCloneBuff
	{
		public MoonlingMinionBuff() : base(ProjectileType<MoonlingMinion>()) { }

		public override int VanillaBuffId => BuffID.MoonLordPet;

		public override string VanillaBuffName => "MoonLordPet";
	}

	public class MoonlingMinionItem : CombatPetMinionItem<MoonlingMinionBuff, MoonlingMinion>
	{
		internal override int VanillaItemID => ItemID.MoonLordPetItem;

		internal override string VanillaItemName => "MoonLordPetItem";
	}

	class MoonlingLaser : MovableLaser
	{
		protected override Rectangle GetFrame(int idx, bool isLast)
		{
			int Y = isLast ? 0 : idx == 0 ? 44 : 22;
			return new Rectangle(0, Y, 22, 20);
		}

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			ChargeTime = 1;
			maxLength = 16 * 16;
			TimeToLive = 120;
			Projectile.timeLeft = TimeToLive;
			Projectile.localNPCHitCooldown = 6; // only on enemy for a couple frames, let it hit twice
		}

		protected override void SpawnDust(Vector2 position, Vector2 velocity)
		{
			if(Main.rand.Next(5) == 0)
			{
				int dustCreated = Dust.NewDust(position, 1, 1, DustID.UltraBrightTorch, velocity.X, velocity.Y, 50, default, Scale: 1.4f);
				Main.dust[dustCreated].color = Color.Azure;
				Main.dust[dustCreated].noGravity = true;
				Main.dust[dustCreated].velocity *= 0.8f;
			}
		}
	}

	public class MoonlingMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<MoonlingMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MoonLordPet;
		internal override int? FiredProjectileId => ProjectileType<MoonlingLaser>();
		internal override LegacySoundStyle ShootSound => SoundID.Item17;

		float initialRotation = 0;
		Vector2 lastValidTarget;

		internal override int GetAttackFrames(CombatPetLevelInfo info) => Math.Max(120, 180 - 6 * info.Level);

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Moonling"));
			Main.projFrames[Projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			DrawOffsetX = -16;
			forwardDir = -1;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			hsHelper.projectileVelocity = 0;
			base.TargetedMovement(vectorToTargetPosition);
			// check if a proj is active
			int projType = (int)FiredProjectileId;
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == player.whoAmI && p.type == projType)
				{
					MoveLaser(p, vectorToTargetPosition);
					p.damage = 2 * Projectile.damage;
					break;
				}
			}
		}

		private void MoveLaser(Projectile p, Vector2 vectorToTargetPosition)
		{
			int framesSinceFired = animationFrame - hsHelper.lastShootFrame;
			int rotationFrames = (int)(0.75f * attackFrames);
			if(framesSinceFired > rotationFrames)
			{
				p.Kill();
				return;
			}
			if(framesSinceFired % 30 == 0)
			{
				SoundEngine.PlaySound(new LegacySoundStyle(2, 15).WithVolume(0.5f), Projectile.Center);
			}
			lastValidTarget = vectorToTargetPosition;
			if(framesSinceFired == 0)
			{
				// start a bit behind the enemy for better visual effect
				initialRotation = vectorToTargetPosition.ToRotation() - MathHelper.Pi/4;
			}
			float rotation;
			if(framesSinceFired < rotationFrames/2)
			{
				rotation = MathHelper.TwoPi * framesSinceFired / rotationFrames;
			} else
			{
				rotation = MathHelper.TwoPi - MathHelper.TwoPi * framesSinceFired / rotationFrames;
			}
			float currentRotation = rotation + initialRotation;
			Vector2 offset = currentRotation.ToRotationVector2() * 32;
			p.ai[0] = currentRotation;
			p.Center = Projectile.Center + offset;
			p.velocity = Vector2.Zero;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			int projType = (int)FiredProjectileId; // kill proj
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == Main.myPlayer && p.type == projType)
				{
					MoveLaser(p, lastValidTarget);
					break;
				}
			}
			base.IdleMovement(vectorToIdlePosition);
		}
	}
}
