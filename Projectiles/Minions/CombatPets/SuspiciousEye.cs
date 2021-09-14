using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets
{
	public class SuspiciousEyeMinionBuff : MinionBuff
	{
		public SuspiciousEyeMinionBuff() : base(ProjectileType<SuspiciousEyeMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.TwinEyesMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.TwinEyesMinion"));
			Main.vanityPet[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			base.Update(player, ref buffIndex);
			CombatPetUtils.SpawnIfAbsent(player, buffIndex, projectileTypes[0], 14);
		}
	}

	public class SuspiciousEyeMinionItem : CombatPetMinionItem<SuspiciousEyeMinionBuff, MiniRetinazerMinion>
	{
		internal override int VanillaItemID => ItemID.EyeOfCthulhuPetItem;

		internal override string VanillaItemName => "EyeOfCthulhuPetItem";

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 10;
			Item.knockBack = 0.5f;
		}
	}

	public class SuspiciousEyeMinion : HoverShooterMinion
	{
		private bool isDashing;
		private static readonly int DashFrames = 60;
		private Vector2 dashVector;
		private MotionBlurDrawer blurHelper;

		internal override int BuffId => BuffType<SuspiciousEyeMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.EyeOfCthulhuPet;

		internal override int? FiredProjectileId => null;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Spazmamini") + " (AoMM Version)");
			Main.projFrames[Projectile.type] = 20;
			Main.projPet[Projectile.type] = true;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
			Projectile.width = 24;
			Projectile.height = 24;
			attackFrames = 90;
			circleHelper.idleBumbleFrames = 90;
			frameSpeed = 5;
			targetSearchDistance = 600;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 12;
			hsHelper.targetInnerRadius = 96;
			hsHelper.targetOuterRadius = 160;
			hsHelper.targetShootProximityRadius = 112;
			blurHelper = new MotionBlurDrawer(5);
		}

		public override void OnSpawn()
		{
			// cut down damage since it's got such a high rate of fire
			Projectile.damage = (int)(Projectile.damage * 0.67f);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int framesSinceShoot = animationFrame - hsHelper.lastShootFrame;
			if(framesSinceShoot % 30 < 15 && framesSinceShoot < DashFrames)
			{
				// dash at the target
				isDashing = true;
				if(dashVector == default)
				{
					dashVector = vectorToTargetPosition;
					dashVector.SafeNormalize();
					dashVector *= (hsHelper.travelSpeed);
					Projectile.velocity = dashVector;
				}
			} else
			{
				dashVector = default;
				isDashing = false;
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			isDashing = false;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			// need to draw sprites manually for some reason
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally: 0;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			// motion blur
			if(isDashing)
			{
				// lifted from ExampleMod's ExampleBullet
				for (int k = 0; k < blurHelper.BlurLength; k++)
				{
					if(!blurHelper.GetBlurPosAndColor(k, lightColor, out Vector2 blurPos, out Color blurColor)) { break; }
					blurPos = blurPos - Main.screenPosition + origin;
					Main.EntitySpriteDraw(texture, blurPos, bounds, blurColor, r, origin, 1, effects, 0);
				}
			}
			// regular version
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r, origin, 1, effects, 0);
			return false;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(isDashing)
			{
				minFrame = 10;
				maxFrame = 16;
			} else
			{
				minFrame = 0;
				maxFrame = 6;
			}
			base.Animate(minFrame, maxFrame);
			Projectile.rotation = 0.05f * Projectile.velocity.X;
			if(Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = 1;
			} else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = -1;
			}
		}
		public override void AfterMoving()
		{
			blurHelper.Update(Projectile.position, isDashing);
		}
	}
}
