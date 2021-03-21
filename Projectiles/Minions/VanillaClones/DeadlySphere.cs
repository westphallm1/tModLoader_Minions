using System;
using System.Collections.Generic;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class DeadlySphereMinionBuff : MinionBuff
	{
		public DeadlySphereMinionBuff() : base(ProjectileType<DeadlySphereMinion>(), ProjectileType<DeadlySphereFireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("DeadlySphere");
			Description.SetDefault("A winged acorn will fight for you!");
		}
	}

	public class DeadlySphereMinionItem : MinionItem<DeadlySphereMinionBuff, DeadlySphereMinion>
	{
		public override string Texture => "Terraria/Item_" + ItemID.DeadlySphereStaff;
		public int[] projTypes = new int[]
		{
			ProjectileType<DeadlySphereFireMinion>(),
			ProjectileType<DeadlySphereMinion>(),
		};
		int spawnCycle = 0;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("DeadlySphere Staff");
			Tooltip.SetDefault("Summons a winged acorn to fight for you!");
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.DeadlySphereStaff);
			base.SetDefaults();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			item.shoot = projTypes[spawnCycle % 2];
			spawnCycle++;
			return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
		}
	}

	/// <summary>
	/// Uses ai[0] to determine whether it's damaging or cosmetic
	/// </summary>
	public class DeadlySphereFire : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.EyeFire;
		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.Flames);
			projectile.aiStyle = 0; // unset default flames AI
			base.SetDefaults();
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 10;
			projectile.timeLeft = 36;
		}
		public override void AI()
		{
			base.AI();
			projectile.localAI[0]++;
			if(projectile.localAI[0] < 4 || Main.rand.Next(2) != 0)
			{
				return;
			}
			projectile.friendly = projectile.ai[0] == 0;
			float dustScale = Math.Min(1, 0.25f * (projectile.localAI[0] - 3));
			int dustType = 135;
			int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100);
			Main.dust[dustId].scale *= 1.5f * dustScale;
			Main.dust[dustId].velocity.X *= 1.2f;
			Main.dust[dustId].velocity.Y *= 1.2f;
			Main.dust[dustId].noGravity = true;
			if (Main.rand.Next(3) == 0)
			{
				Main.dust[dustId].scale *= 2f;
				Main.dust[dustId].velocity.X *= 2f;
				Main.dust[dustId].velocity.Y *= 2f;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Frostburn, 600);
		}
	}

	public class DeadlySphereMinion : HoverShooterMinion
	{
		private bool isDashing;
		private Vector2 dashVector;
		// something in the ai overrides seems to prevent projectile.oldPos from populating properly,
		// so just replicate it manually
		private Vector2[] myOldPos = new Vector2[5];

		protected override int BuffId => BuffType<DeadlySphereMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.DeadlySphere;

		internal override int? FiredProjectileId => null;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying DeadlySphere");
			Main.projFrames[projectile.type] = 21;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			attackFrames = 90;
			travelSpeed = 15;
			targetSearchDistance = 950;
			projectileVelocity = 6;
			targetInnerRadius = 96;
			targetOuterRadius = 160;
			targetShootProximityRadius = 112;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(5, 10);
			projectile.rotation += projectile.velocity.Length() * 0.05f;
			Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 0.5f);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int framesSinceShoot = animationFrame - lastShootFrame;
			if(framesSinceShoot > 20 && framesSinceShoot % 15 < 10)
			{
				// dash at the target
				isDashing = true;
				if(dashVector == default)
				{
					dashVector = vectorToTargetPosition;
					dashVector.SafeNormalize();
					dashVector *= (travelSpeed + 2);
					if(targetNPCIndex is int idx)
					{
						dashVector += Main.npc[idx].velocity / 8;
					}
					projectile.velocity = dashVector;
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
			float oldRotation = projectile.rotation;
			base.IdleMovement(vectorToIdlePosition);
			projectile.rotation = oldRotation;
			isDashing = false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// need to draw sprites manually for some reason
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : 0;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			// motion blur
			if(isDashing)
			{
				// lifted from ExampleMod's ExampleBullet
				for (int k = 0; k < myOldPos.Length; k++)
				{
					Vector2 blurPos = myOldPos[k] - Main.screenPosition + origin;
					Color color = projectile.GetAlpha(lightColor) * ((myOldPos.Length - k) / (float)myOldPos.Length);
					spriteBatch.Draw(texture, blurPos, bounds, color, r, origin, 1, effects, 0);
				}
			}
			// regular version
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r, origin, 1, effects, 0);
			return false;
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
				myOldPos[0] = projectile.position;
			} else
			{
				myOldPos = new Vector2[5];
			}

		}
	}
	public class DeadlySphereFireMinion : HoverShooterMinion
	{

		protected override int BuffId => BuffType<DeadlySphereMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.DeadlySphere;

		internal override int? FiredProjectileId => null;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying DeadlySphere");
			Main.projFrames[projectile.type] = 21;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			attackFrames = 90;
			travelSpeed = 15;
			targetSearchDistance = 950;
			projectileVelocity = 6;
			targetInnerRadius = 96;
			targetOuterRadius = 160;
			targetShootProximityRadius = 112;
		}
		public override void OnSpawn()
		{
			// cut down damage since it's got such a high rate of fire
			projectile.damage = (int)(projectile.damage * 0.5f);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(17, 21);
			if(vectorToTarget == null || animationFrame - lastShootFrame > 60)
			{
				projectile.rotation += MathHelper.TwoPi/60;
				if (Main.rand.Next(2) == 0)
				{
					for (float angle = 0; angle < MathHelper.TwoPi; angle += MathHelper.PiOver2)
					{
						if (Main.rand.Next(2) != 0)
						{
							int dustType = new int[] { 226, 228, 75 }[Main.rand.Next(3)];
							Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, dustType);
							Vector2 rotationVector = (projectile.rotation + MathHelper.PiOver4 + angle).ToRotationVector2();
							dust.position = projectile.Center + rotationVector * 14.2f;
							dust.velocity = rotationVector;
							dust.scale = 0.3f + Main.rand.NextFloat() * 0.5f;
							dust.noGravity = true;
							dust.noLight = true;
						}
					}
				}
			} else
			{
				for (float angle = 0; angle < MathHelper.TwoPi; angle += MathHelper.PiOver2)
				{
					int dustType = new int[] { 226, 228, 75 }[Main.rand.Next(3)];
					Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, dustType);
					Vector2 rotationVector = (projectile.rotation + MathHelper.PiOver4 + angle).ToRotationVector2();
					dust.position = projectile.Center + rotationVector * 14.2f;
					dust.velocity = rotationVector;
					dust.scale = 0.6f + Main.rand.NextFloat() * 0.5f;
					dust.noGravity = true;
				}
			} 
			Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 0.5f);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			int framesSinceShoot = animationFrame - lastShootFrame;
			if (framesSinceShoot < 60 && framesSinceShoot % 6 == 0)
			{
				if(targetNPCIndex is int idx) 
				{
					vectorToTargetPosition += Main.npc[idx].velocity / 4;
				}
				projectile.rotation = vectorToTargetPosition.ToRotation() + MathHelper.Pi/4;
				Vector2 lineOfFire = vectorToTargetPosition;
				lineOfFire.Normalize();
				lineOfFire *= projectileVelocity;
				lineOfFire += projectile.velocity / 3;
				for(int i = 0; i < 3; i++)
				{
					if(player.whoAmI == Main.myPlayer)
					{
						FireProjectile(lineOfFire, ProjectileType<DeadlySphereFire>(), ai0: i);
					}
				}
				AfterFiringProjectile();
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			float oldRotation = projectile.rotation;
			base.IdleMovement(vectorToIdlePosition);
			projectile.rotation = oldRotation;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// need to draw sprites manually for some reason
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : 0;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			// regular version
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r, origin, 1, effects, 0);
			return false;
		}
	}
}
