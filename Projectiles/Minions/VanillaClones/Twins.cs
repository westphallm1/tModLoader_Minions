using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
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

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class TwinsMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<MiniRetinazerMinion>(), ProjectileType<MiniSpazmatismMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault(Language.GetTextValue("BuffName.TwinEyesMinion") + " (AoMM Version)");
			// Description.SetDefault(Language.GetTextValue("BuffDescription.TwinEyesMinion"));
		}
	}

	public class TwinsMinionItem : VanillaCloneMinionItem<TwinsMinionBuff, MiniRetinazerMinion>
	{
		internal override int VanillaItemID => ItemID.OpticStaff;

		internal override string VanillaItemName => "OpticStaff";

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ApplyBuff(player);

			var p = Projectile.NewProjectileDirect(source, position, Vector2.Zero, ProjectileType<MiniSpazmatismMinion>(), damage, knockback, player.whoAmI);
			p.originalDamage = Item.damage;
			var p2 = Projectile.NewProjectileDirect(source, position, Vector2.Zero, ProjectileType<MiniRetinazerMinion>(), damage, knockback, player.whoAmI);
			p2.originalDamage = Item.damage;
			return false;
		}

		public override void AddRecipes()
		{
			// Rather annoying little Calamity shim. I will remember this.
			if(!ModLoader.HasMod("CalamityMod"))
			{
				base.AddRecipes();
			}
		}
	}

	public class MiniEyeFire : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.EyeFire;
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Flames);
			Projectile.aiStyle = 0; // unset default flames AI
			base.SetDefaults();
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.timeLeft = 42;
		}
		public override void AI()
		{
			base.AI();
			Projectile.friendly = Projectile.ai[0] == 0;
			Projectile.localAI[0]++;
			if(Projectile.localAI[0] < 8 || !Main.rand.NextBool(2))
			{
				return;
			}
			float dustScale = Math.Min(1, 0.25f * (Projectile.localAI[0] - 7));
			int dustType = 75;
			int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100);
			Main.dust[dustId].scale *= 1.5f * dustScale;
			Main.dust[dustId].velocity.X *= 1.2f;
			Main.dust[dustId].velocity.Y *= 1.2f;
			Main.dust[dustId].noGravity = true;
			if (Main.rand.NextBool(3))
			{
				Main.dust[dustId].scale *= 1.25f;
				Main.dust[dustId].velocity.X *= 2f;
				Main.dust[dustId].velocity.Y *= 2f;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.CursedInferno, 300);
		}
	}

	public class MiniTwinsLaser : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MiniRetinaLaser;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 2;
		}

		public override void SetDefaults()
			{
			Projectile.CloneDefaults(ProjectileID.MiniRetinaLaser);
			base.SetDefaults();
			Projectile.penetrate = 1;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Rectangle bounds = texture.Bounds;
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, Color.White, r,
				bounds.GetOrigin(), 1, 0, 0);
			return false;
		}
	}

	public class MiniRetinazerMinion : HoverShooterMinion
	{
		public override int BuffId => BuffType<TwinsMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Retanimini;

		internal override int? FiredProjectileId => ProjectileType<MiniTwinsLaser>();

		internal override SoundStyle? ShootSound => SoundID.Item10 with { Volume = 0.5f };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Retanimini") + " (AoMM Version)");
			Main.projFrames[Projectile.type] = 3;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0.5f;
			Projectile.width = 24;
			Projectile.height = 24;
			attackFrames = 90;
			circleHelper.idleBumbleFrames = 90;
			FrameSpeed = 5;
			targetSearchDistance = 850;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 12;
			hsHelper.projectileVelocity = 24;
			hsHelper.targetInnerRadius = 208;
			hsHelper.targetOuterRadius = 264;
			hsHelper.targetShootProximityRadius = 128;
		}
		public override void OnSpawn()
		{
			// cut down damage since it's got such a high rate of fire
			Projectile.originalDamage = (int)(Projectile.originalDamage * 0.75);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			Projectile.rotation = vectorToTargetPosition.ToRotation() + MathHelper.Pi;
			int attackCycleFrame = AnimationFrame - hsHelper.lastShootFrame;
			if(attackCycleFrame == 32)
			{
				Vector2 lineOfFire = vectorToTargetPosition;
				lineOfFire.SafeNormalize();
				lineOfFire *= hsHelper.projectileVelocity;
				if(Player.whoAmI == Main.myPlayer)
				{
					hsHelper.FireProjectile(lineOfFire, (int)FiredProjectileId, 0);
				}
				AfterFiringProjectile();
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			Projectile.spriteDirection = 0;
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
		}
	}
	public class MiniSpazmatismMinion : HoverShooterMinion
	{
		private bool isDashing;
		private Vector2 dashVector;
		// something in the ai overrides seems to prevent projectile.oldPos from populating properly,
		// so just replicate it manually
		private Vector2[] myOldPos = new Vector2[5];
		private MotionBlurDrawer blurHelper;

		public override int BuffId => BuffType<TwinsMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Spazmamini;

		internal override int? FiredProjectileId => ProjectileType<MiniEyeFire>();
		internal override SoundStyle? ShootSound => SoundID.Item34 with { Volume = 0.5f };

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Spazmamini") + " (AoMM Version)");
			Main.projFrames[Projectile.type] = 3;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0.5f;
			Projectile.width = 24;
			Projectile.height = 24;
			attackFrames = 90;
			circleHelper.idleBumbleFrames = 90;
			FrameSpeed = 5;
			targetSearchDistance = 850;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 14;
			hsHelper.projectileVelocity = 6;
			hsHelper.targetInnerRadius = 96;
			hsHelper.targetOuterRadius = 160;
			hsHelper.targetShootProximityRadius = 112;
			blurHelper = new MotionBlurDrawer(5);
			DealsContactDamage = true;
		}

		public override void OnSpawn()
		{
			// cut down damage since it's got such a high rate of fire
			Projectile.originalDamage = (int)(Projectile.originalDamage * 0.67f);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int framesSinceShoot = AnimationFrame - hsHelper.lastShootFrame;
			if((framesSinceShoot > 45 && framesSinceShoot < 55) || (framesSinceShoot > 75 && framesSinceShoot < 85))
			{
				// dash at the target
				isDashing = true;
				if(dashVector == default)
				{
					dashVector = vectorToTargetPosition;
					dashVector.SafeNormalize();
					dashVector *= (hsHelper.travelSpeed + 2);
					Projectile.velocity = dashVector;
				}
			} else
			{
				dashVector = default;
				isDashing = false;
				base.TargetedMovement(vectorToTargetPosition);
			}
			Projectile.rotation = vectorToTargetPosition.ToRotation() + MathHelper.Pi;
			if(framesSinceShoot < 45 && framesSinceShoot % 6 == 0)
			{
				Vector2 lineOfFire = vectorToTargetPosition;
				lineOfFire.SafeNormalize();
				lineOfFire *= hsHelper.projectileVelocity;
				lineOfFire += Projectile.velocity / 3;
				if(Player.whoAmI == Main.myPlayer)
				{
					hsHelper.FireProjectile(lineOfFire, (int)FiredProjectileId, framesSinceShoot % 18);
				}
				AfterFiringProjectile();
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
			SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : 0;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			// motion blur
			if(isDashing)
			{
				blurHelper.DrawBlur(texture, lightColor, bounds, r);
			}
			// regular version
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r, bounds.GetOrigin(), 1, effects, 0);
			return false;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			Projectile.spriteDirection = 0;
		}
		public override void AfterMoving()
		{
			// left shift old position
			if(isDashing)
			{
				for(int i = myOldPos.Length -1; i > 0; i--)
				{
					myOldPos[i] = myOldPos[i - 1];
				}
				myOldPos[0] = Projectile.position;
			} else
			{
				myOldPos = new Vector2[5];
			}
		}
	}
}
