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
		public override void SetDefaults()
		{
			base.SetDefaults();
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
		public override string Texture => "Terraria/Item_0";
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.aiStyle = 14;
			projectile.friendly = true;
			projectile.width = 16;
			projectile.height = 16;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 30;
			projectile.penetrate = 5;
			projectile.timeLeft = 120;
		}

		public override void AI()
		{
			base.AI();
			int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 6, 0f, 0f, 100);
			Main.dust[dustId].position.X -= 2f;
			Main.dust[dustId].position.Y += 2f;
			Main.dust[dustId].scale += Main.rand.NextFloat(0.5f);
			Main.dust[dustId].noGravity = true;
			Main.dust[dustId].velocity.Y -= 2f;
			if (Main.rand.Next(2) == 0)
			{
				dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 6, 0f, 0f, 100);
				Main.dust[dustId].position.X -= 2f;
				Main.dust[dustId].position.Y += 2f;
				Main.dust[dustId].scale += 0.3f + Main.rand.NextFloat(0.5f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity *= 0.1f;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return false;
		}

	}

	public class RavenMinion : HeadCirclingGroupAwareMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.Raven;
		private int framesSinceLastHit;
		private int cooldownAfterHitFrames = 16;
		bool isDashing = false;
		private Vector2[] myOldPos = new Vector2[5];
		protected override int BuffId => BuffType<RavenMinionBuff>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Raven") + " (AoMM Version)");
			Main.projFrames[projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			attackFrames = 60;
			targetSearchDistance = 900;
			idleBumble = true;
			idleBumbleFrames = 60;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			projectile.frameCounter++;
			if(vectorToTarget is Vector2 target && target.Length() < 256)
			{
				minFrame = 4;
				maxFrame = 8;
			} else
			{
				minFrame = 0;
				maxFrame = 4;
			}
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= (int)maxFrame)
				{
					projectile.frame = minFrame;
				}
			}
			if(projectile.velocity.X > 1)
			{
				projectile.spriteDirection = -1;
			} else if (projectile.velocity.X < -1)
			{
				projectile.spriteDirection = 1;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// need to draw sprites manually for some reason
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : 0;
			Texture2D texture = GetTexture(Texture);
			Texture2D glowTexture = GetTexture(base.Texture + "_Glow");
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			if(isDashing)
			{
				// lifted from ExampleMod's ExampleBullet
				// motion blur
				for (int k = 0; k < myOldPos.Length; k++)
				{
					if(myOldPos[k] == default)
					{
						break;
					}
					Vector2 blurPos = myOldPos[k] - Main.screenPosition + origin;
					Color color = projectile.GetAlpha(lightColor) * ((myOldPos.Length - k) / (float)myOldPos.Length);
					spriteBatch.Draw(texture, blurPos, bounds, color, r, origin, 1, effects, 0);
					spriteBatch.Draw(glowTexture, blurPos, bounds, color, r, origin, 1, effects, 0);
				}
			}
			// regular version
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r, origin, 1, effects, 0);
			// glow
			if(isDashing)
			{
				spriteBatch.Draw(glowTexture, pos - Main.screenPosition, bounds, Color.White, r, origin, 1, effects, 0);
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
				Vector2 turnVelocity = new Vector2(-projectile.velocity.Y, projectile.velocity.X) / 8;
				turnVelocity *= Math.Sign(projectile.velocity.X);
				projectile.velocity += turnVelocity;
			}
			else if (framesSinceLastHit++ > cooldownAfterHitFrames)
			{
				projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
			else
			{
				projectile.velocity.SafeNormalize();
				projectile.velocity *= 10; // kick it away from enemies that it's just hit
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
			if(isDashing)
			{
				for(int i = myOldPos.Length -1; i > 0; i--)
				{
					myOldPos[i] = myOldPos[i - 1];
				}
				myOldPos[0] = projectile.position;
				if(Main.rand.Next(2) == 0)
				{
					int dustId = Dust.NewDust(
						projectile.position, 
						projectile.width, projectile.height, 6, 
						projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 
						100, default, 2f);
					Main.dust[dustId].noGravity = true;
					Main.dust[dustId].velocity.X *= 0.3f;
					Main.dust[dustId].velocity.Y *= 0.3f;
					Main.dust[dustId].noLight = true;
				}
			} else
			{
				myOldPos = new Vector2[5];
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if(player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[ProjectileType<RavenGreekFire>()] < 8 && Main.rand.Next(5) == 0)
			{
				Vector2 lineOfFire = (Main.rand.NextFloat(MathHelper.Pi) + MathHelper.Pi).ToRotationVector2() * Main.rand.NextFloat(6, 8);
				Projectile.NewProjectile(
					projectile.Center,
					lineOfFire,
					ProjectileType<RavenGreekFire>(),
					projectile.damage,
					projectile.knockBack,
					Main.myPlayer);
			}
		}
	}
}
