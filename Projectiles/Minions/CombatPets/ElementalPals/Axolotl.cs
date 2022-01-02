using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets;
using AmuletOfManyMinions.Projectiles.Squires.SoulboundArsenal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class AxolotlMinionBuff : CombatPetBuff
	{
		public AxolotlMinionBuff() : base(ProjectileType<AxolotlMinion>()) { }
	}

	public class AxolotlMinionItem : CombatPetMinionItem<AxolotlMinionBuff, AxolotlMinion>
	{
		internal override string VanillaItemName => "FullMoonSqueakyToy";
		internal override int VanillaItemID => ItemID.FullMoonSqueakyToy;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Axolotl");
		}
	}

	public class WaterBeam : MovableLaser
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Typhoon;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			ChargeTime = 1;
			maxLength = 16;
			TimeToLive = 600;
			Projectile.timeLeft = TimeToLive;
			baseTangentSize = 0;
			Projectile.localNPCHitCooldown = 15;
			StopAfterFirstCollision = true;
		}

		protected override void SpawnDust(Vector2 position, Vector2 velocity)
		{
			if (Main.rand.Next(20) == 0)
			{
				int dustCreated = Dust.NewDust(position, 1, 1, 137, Projectile.velocity.X, Projectile.velocity.Y, 0, Scale: 1.4f);
				Main.dust[dustCreated].noGravity = true;
			}
		}
		public override void AI()
		{
			if(maxLength < 24 * 16)
			{
				maxLength += 8;
			}
			base.AI();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			int frame = TimeToLive - Projectile.timeLeft;
			if(TimeToLive - Projectile.timeLeft < 2)
			{
				return false;
			}
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameIdx = (frame / 5) % 3;

			Vector2 center = Main.projectile[(int)Projectile.ai[1]].Center;
			Vector2 end = endPoint;
			Vector2 step = end - center;
			float beamLength = step.Length();
			step.SafeNormalize();
			int stepSize = 8;
			int wavelength = 8 * stepSize;
			int amplitude = 16;
			float phaseOffset = MathHelper.TwoPi * frame / 30f;
			int frameHeight = texture.Height / 3;
			Rectangle bounds = new Rectangle(0, frameIdx * frameHeight, texture.Width, frameHeight);
			for(int i = 0; i < beamLength - 2 * stepSize; i+= stepSize)
			{
				Vector2 origin = new Vector2(bounds.Width, bounds.Height) / 2;
				float scale = Math.Min(0.5f, i / (16f * 16f));
				float angle = phaseOffset + MathHelper.TwoPi * i / wavelength;
				float rotation = angle;
				Vector2 offset = tangent * scale * amplitude * MathF.Sin(angle);
				Vector2 pos = center + i * step + offset;
				Main.EntitySpriteDraw(
					texture, pos - Main.screenPosition, 
					bounds, lightColor, rotation, 
					origin, scale, 0, 0);
			}
			return false;
		}

		public override void Kill(int timeLeft)
		{
			Vector2 center = Main.projectile[(int)Projectile.ai[1]].Center;
			Vector2 end = endPoint;
			Vector2 step = end - center;
			float beamLength = step.Length();
			step.SafeNormalize();
			int stepSize = 8;
			for(int i = 0; i < beamLength + stepSize; i+= stepSize)
			{
				Vector2 position = center + i * step;
				int dustCreated = Dust.NewDust(position, 1, 1, 137, 0, 0, 0, Scale: 1.4f);
				Main.dust[dustCreated].noGravity = true;
			}
		}

	}

	public class AxolotlMinion : CombatPetGroundedRangedMinion
	{
		internal override int BuffId => BuffType<AxolotlMinionBuff>();

		internal override Vector2 LaunchPos => Projectile.Top;

		private Projectile laser;

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(30, 24, -16, -2, -1);
			ConfigureFrames(12, (0, 1), (2, 6), (2, 2), (7, 11));
		}

		internal override bool ShouldDoShootingMovement => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Skeletal;

		internal override int? ProjId => (leveledPetPlayer?.PetLevel ?? 0) >= (int)CombatPetTier.Spectre ?
			ProjectileType<WaterBeam>() :
			ProjectileType<SharkPupBubble>();

		public override Vector2 IdleBehavior()
		{
			laser = default;
			int laserType = ProjectileType<WaterBeam>();
			if(ProjId == laserType)
			{
				for(int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if(p.active && p.owner == player.whoAmI && p.type == laserType && p.ai[1] == Projectile.whoAmI)
					{
						laser = p;
						break;
					}
				}
			}
			return base.IdleBehavior();
		}

		public override void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			if(ProjId == ProjectileType<SharkPupBubble>())
			{
				base.LaunchProjectile(launchVector, ai0);
			} else if (laser == default)
			{
				Projectile.NewProjectile(
					Projectile.GetProjectileSource_FromThis(),
					LaunchPos,
					Vector2.Zero,
					(int)ProjId,
					(int)(ModifyProjectileDamage(leveledPetPlayer.PetLevelInfo) * Projectile.damage),
					Projectile.knockBack,
					player.whoAmI,
					ai0: launchVector.ToRotation(),
					ai1: Projectile.whoAmI);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if(laser != default)
			{
				laser.Center = LaunchPos;
				laser.ai[0] = Utils.AngleLerp(laser.ai[0], vectorToTargetPosition.ToRotation(), 0.1f);
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			if(laser != default && vectorToTarget == default)
			{
				laser.Kill();
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.GetAnimationState();
			frameSpeed = (state == GroundAnimationState.WALKING) ? 5 : 10;
			base.Animate(minFrame, maxFrame);
			if(state == GroundAnimationState.JUMPING)
			{
				Projectile.frame = Projectile.velocity.Y > 0 ? 2 : 5;
			} 
		}
	}
}
