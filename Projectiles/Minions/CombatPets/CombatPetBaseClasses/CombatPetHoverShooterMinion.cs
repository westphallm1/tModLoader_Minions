using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses
{
	public abstract class CombatPetHoverShooterMinion : HoverShooterMinion
	{
		internal LeveledCombatPetModPlayer leveledPetPlayer;

		internal virtual int GetAttackFrames(CombatPetLevelInfo info) => Math.Max(30, 60 - 6 * info.Level);
		internal virtual int GetProjectileVelocity(CombatPetLevelInfo info) => (int)info.BaseSpeed + 3;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
			// these are likely to be static
			hsHelper.targetInnerRadius = 96;
			hsHelper.targetOuterRadius = 128;
			hsHelper.targetShootProximityRadius = 96;
			UpdateHsHelperWithPetLevel(0);
		}

		public override Vector2 IdleBehavior()
		{
			leveledPetPlayer = player.GetModPlayer<LeveledCombatPetModPlayer>();
			Projectile.originalDamage = leveledPetPlayer.PetDamage;
			UpdateHsHelperWithPetLevel(leveledPetPlayer.PetLevel);
			return base.IdleBehavior();
		}
		
		private void UpdateHsHelperWithPetLevel(int petLevel)
		{
			CombatPetLevelInfo info = CombatPetLevelTable.PetLevelTable[petLevel];
			targetSearchDistance = info.BaseSearchRange;
			attackFrames = GetAttackFrames(info);
			hsHelper.projectileVelocity = GetProjectileVelocity(info);
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = (int)info.BaseSpeed;
		}
	}

	public abstract class CombatPetHoverDasherMinion : CombatPetHoverShooterMinion
	{
		internal bool isDashing;
		private Vector2 dashVector;
		internal MotionBlurDrawer blurHelper;

		internal override int? FiredProjectileId => null;
		internal override int GetAttackFrames(CombatPetLevelInfo info) => 90;

		public override void SetDefaults()
		{
			base.SetDefaults();
			dealsContactDamage = true;
			hsHelper.targetInnerRadius = 96;
			hsHelper.targetOuterRadius = 160;
			hsHelper.targetShootProximityRadius = 112;
			blurHelper = new MotionBlurDrawer(5);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int dashFrames = (int)(attackFrames * 0.75f);
			int dashCyle = Math.Max(18, (int)(attackFrames * 0.25f));
			int dashLength = 15; // this should probably be constant
			int framesSinceShoot = animationFrame - hsHelper.lastShootFrame;
			if(framesSinceShoot % dashCyle < dashLength && framesSinceShoot < dashFrames)
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

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			isDashing = false;
		}

		public override void AfterMoving()
		{
			blurHelper.Update(Projectile.position, isDashing);
		}
	}
}
