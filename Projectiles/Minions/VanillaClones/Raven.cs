using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class RavenMinionBuff : MinionBuff
	{
		public RavenMinionBuff() : base(ProjectileType<RavenMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.Ravens") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.Ravens"));
		}

	}

	public class RavenMinionItem : VanillaCloneMinionItem<RavenMinionBuff, RavenMinion>
	{
		internal override int VanillaItemID => ItemID.RavenStaff;

		internal override string VanillaItemName => "RavenStaff";
	}

	public class RavenGreekFire: ModProjectile
	{
		public override string Texture => "Terraria/Images/Item_0";
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.aiStyle = 14;
			Projectile.friendly = true;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
			Projectile.penetrate = 5;
			Projectile.timeLeft = 120;
		}

		public override void AI()
		{
			base.AI();
			int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, 0f, 0f, 100);
			Main.dust[dustId].position.X -= 2f;
			Main.dust[dustId].position.Y += 2f;
			Main.dust[dustId].scale += Main.rand.NextFloat(0.5f);
			Main.dust[dustId].noGravity = true;
			Main.dust[dustId].velocity.Y -= 2f;
			if (Main.rand.Next(2) == 0)
			{
				dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, 0f, 0f, 100);
				Main.dust[dustId].position.X -= 2f;
				Main.dust[dustId].position.Y += 2f;
				Main.dust[dustId].scale += 0.3f + Main.rand.NextFloat(0.5f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity *= 0.1f;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

	}

	public class RavenMinion : HeadCirclingGroupAwareMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Raven;
		private int framesSinceLastHit;
		private int cooldownAfterHitFrames = 16;
		bool isDashing = false;
		private MotionBlurDrawer blurHelper;
		public override string GlowTexture => base.Texture + "_Glow";
		internal override int BuffId => BuffType<RavenMinionBuff>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Raven") + " (AoMM Version)");
			Main.projFrames[Projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		public override void LoadAssets()
		{
			AddTexture(Texture + "_Glow");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			attackFrames = 60;
			targetSearchDistance = 900;
			circleHelper.idleBumbleFrames = 60;
			bumbleSpriteDirection = -1;
			blurHelper = new MotionBlurDrawer(5);
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			Projectile.frameCounter++;
			if(vectorToTarget is Vector2 target && target.Length() < 256)
			{
				minFrame = 4;
				maxFrame = 8;
			} else
			{
				minFrame = 0;
				maxFrame = 4;
			}
			if (Projectile.frameCounter >= frameSpeed)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= (int)maxFrame)
				{
					Projectile.frame = minFrame;
				}
			}
			if(vectorToTarget != null)
			{
				if(Projectile.velocity.X > 1)
				{
					Projectile.spriteDirection = -1;
				} else if (Projectile.velocity.X < -1)
				{
					Projectile.spriteDirection = 1;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// need to draw sprites manually for some reason
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : 0;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Texture2D glowTexture = ExtraTextures[0].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			if(isDashing)
			{
				for (int k = 0; k < blurHelper.BlurLength; k++)
				{
					if(!blurHelper.GetBlurPosAndColor(k, lightColor, out Vector2 blurPos, out Color blurColor)) { break; }
					blurPos = blurPos - Main.screenPosition;
					Main.EntitySpriteDraw(texture, blurPos, bounds, blurColor, r, origin, 1, effects, 0);
					Main.EntitySpriteDraw(glowTexture, blurPos, bounds, blurColor, r, origin, 1, effects, 0);
				}
			}
			// regular version
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r, origin, 1, effects, 0);
			// glow
			if(isDashing)
			{
				Main.EntitySpriteDraw(glowTexture, pos - Main.screenPosition, bounds, Color.White, r, origin, 1, effects, 0);
			}
			return false;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			float inertia = 18;
			float speed = isDashing ? 16 : 13;
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= speed;
			framesSinceLastHit++;
			if (framesSinceLastHit < cooldownAfterHitFrames && framesSinceLastHit > cooldownAfterHitFrames / 2)
			{
				// start turning so we don't double directly back
				Vector2 turnVelocity = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X) / 8;
				turnVelocity *= Math.Sign(Projectile.velocity.X);
				Projectile.velocity += turnVelocity;
			}
			else if (framesSinceLastHit++ > cooldownAfterHitFrames)
			{
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
			else
			{
				Projectile.velocity.SafeNormalize();
				Projectile.velocity *= 10; // kick it away from enemies that it's just hit
			}
		}

		public override void OnHitTarget(NPC target)
		{
			framesSinceLastHit = 0;
		}

		public override void AfterMoving()
		{
			// left shift old position
			isDashing = vectorToTarget is Vector2 target && target.Length() < 256;
			blurHelper.Update(Projectile.Center, isDashing);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if(player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[ProjectileType<RavenGreekFire>()] < 8 && Main.rand.Next(5) == 0)
			{
				Vector2 lineOfFire = (Main.rand.NextFloat(MathHelper.Pi) + MathHelper.Pi).ToRotationVector2() * Main.rand.NextFloat(6, 8);
				Projectile.NewProjectile(
					Projectile.GetProjectileSource_FromThis(),
					Projectile.Center,
					lineOfFire,
					ProjectileType<RavenGreekFire>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer);
			}
		}
	}
}
