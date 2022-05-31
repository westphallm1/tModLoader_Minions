using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Terraria.Localization;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using static AmuletOfManyMinions.Items.Accessories.MinionSpawningItemPlayer;
using System;
using Terraria.Audio;
using AmuletOfManyMinions.Core.Minions.Effects;
using Microsoft.Xna.Framework.Graphics;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd
{
	public class AbigailMinionBuff : MinionBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.AbigailMinion;

		internal override int[] ProjectileTypes => new int[] { ProjectileType<AbigailCounterMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.AbigailMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.AbigailMinion"));
		}

	}

	public class AbigailMinionItem : VanillaCloneMinionItem<AbigailMinionBuff, AbigailCounterMinion>
	{
		internal override int VanillaItemID => ItemID.AbigailsFlower;

		internal override string VanillaItemName => "AbigailsFlower";
	}

	public class AbigailCounterMinion : CounterMinion
	{
		internal override int BuffId => BuffType<AbigailMinionBuff>();
		protected override int MinionType => ProjectileType<AbigailMinion>();
	}

	public class AbigailMinion : EmpoweredMinion
	{
		internal override int BuffId => BuffType<AbigailMinionBuff>();
		public override int CounterType => ProjectileType<AbigailCounterMinion>();
		protected override int dustType => 6;

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.AbigailMinion;

		internal bool isCloseToTarget = false;
		internal HoverShooterHelper hsHelper;
		internal int stayInPlaceFrames = 0;
		internal int attackRadius = 148;
		internal int damageRadius = 80;
		internal bool IsAttacking => vectorToTarget is Vector2 target && target.LengthSquared() < attackRadius * attackRadius;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.AbigailMinion"));
			Main.projFrames[Projectile.type] = 13;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}

		public override void LoadAssets()
		{
			Main.instance.LoadProjectile(ProjectileID.MedusaHeadRay);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.tileCollide = true;
			attackThroughWalls = false;
			Projectile.width = 32;
			Projectile.height = 32;
			frameSpeed = 8;
			// can hit many npcs at once, so give it a relatively high on hit cooldown
			Projectile.localNPCHitCooldown = 20;
			hsHelper = new HoverShooterHelper(this, default)
			{
				attackFrames = 30,
				projectileVelocity = 14,
				targetShootProximityRadius = attackRadius,
				targetInnerRadius = 48,
				targetOuterRadius = 64,
			};
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, Projectile);
			idlePosition.Y += -32;
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition.X = player.Top.X;
				idlePosition.Y = player.Top.Y - 16;
			}
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}


		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			projHitbox = new Rectangle(
				(int)Projectile.Center.X - damageRadius, (int)Projectile.Center.Y - damageRadius,
				2 * damageRadius, 2 * damageRadius);
			return Vector2.DistanceSquared(Projectile.Center, targetHitbox.Center.ToVector2()) < damageRadius * damageRadius
			    || projHitbox.Intersects(targetHitbox);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			hsHelper.TargetedMovement(vectorToTargetPosition);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			isCloseToTarget = false;
			base.IdleMovement(vectorToIdlePosition);
		}

		protected override int ComputeDamage() => baseDamage + baseDamage * (int)((EmpowerCount - 1) * (Main.hardMode ? 1.3f : 0.55f));

		protected override float ComputeSearchDistance() => 800;

		protected override float ComputeInertia() => 11;

		protected override float ComputeTargetedSpeed() => Math.Min(13, 9 + EmpowerCount);

		protected override float ComputeIdleSpeed() => ComputeTargetedSpeed() + 3;

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			if(IsAttacking)
			{
				minFrame = 9;
				maxFrame = 13;
			} else
			{
				minFrame = 0;
				maxFrame = 8;
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if(IsAttacking)
			{
				Projectile.spriteDirection = Math.Sign(((Vector2)vectorToTarget).X);
			}
			else if(Math.Abs(Projectile.velocity.X) > 1)
			{
				Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
			}
		}


		private void DrawLightRays(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[ProjectileID.MedusaHeadRay].Value;
			Rectangle bounds = texture.Bounds;
			Vector2 origin = new Vector2(bounds.Width, bounds.Height) / 2;
			float baseAngle = MathHelper.TwoPi * animationFrame / 180;
			int rayCount = 7;
			for(int i = 0; i < rayCount; i++)
			{
				float localAngle = baseAngle + MathHelper.TwoPi * i / rayCount;
				float localIntensity = MathF.Sin(1.75f * localAngle);
				float scale = 0.5f + 0.25f * localIntensity;
				float brightness = 0.65f + 0.25f * localIntensity;
				Vector2 drawOffset = localAngle.ToRotationVector2() * scale * bounds.Height / 2;
				Main.EntitySpriteDraw(texture, Projectile.Center + drawOffset - Main.screenPosition,
					bounds, lightColor.MultiplyRGB(Color.LightCoral) * brightness, localAngle + MathHelper.PiOver2,
					origin, scale, 0, 0);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// attacking light rays
			if(IsAttacking)
			{
				DrawLightRays(ref lightColor);
			}

			// body
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new (0, Projectile.frame * frameHeight, texture.Width/4, frameHeight);
			Vector2 origin = new Vector2(bounds.Width, bounds.Height) / 2;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				bounds, lightColor * 0.75f, Projectile.rotation,
				origin, 1, effects, 0);

			// flower
			Color flowerColor = MinionColors[((EmpowerCount - 1)/ 3) % MinionColors.Length];
			Vector2 flowerOffset = Projectile.frame == 9 ? new(0, -6) : Projectile.frame == 10 ? new(0, 8) : default;
			bounds = new ((1 + (EmpowerCount - 1) % 3) * texture.Width/4, 0, texture.Width/4, frameHeight);
			Main.EntitySpriteDraw(texture, Projectile.Center + flowerOffset - Main.screenPosition,
				bounds, flowerColor.MultiplyRGB(lightColor), Projectile.rotation,
				origin, 1, effects, 0);
			return false;
		}


	}
}
