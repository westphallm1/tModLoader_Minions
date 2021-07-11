using System;
using System.Collections.Generic;
using System.Linq;
using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class DeadlySphereMinionBuff : MinionBuff
	{
		public DeadlySphereMinionBuff() : base(
			ProjectileType<DeadlySphereMinion>(), 
			ProjectileType<DeadlySphereClingerMinion>(),
			ProjectileType<DeadlySphereFireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.DeadlySphere") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.DeadlySphere"));
		}
	}

	public class DeadlySphereMinionItem : VanillaCloneMinionItem<DeadlySphereMinionBuff, DeadlySphereMinion>
	{
		internal override int VanillaItemID => ItemID.DeadlySphereStaff;

		internal override string VanillaItemName => "DeadlySphereStaff";

		public int[] projTypes;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Tooltip.SetDefault("");
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
			if(projTypes == null)
			{
				projTypes = new int[]
				{
					ProjectileType<DeadlySphereMinion>(),
					ProjectileType<DeadlySphereFireMinion>(),
					ProjectileType<DeadlySphereClingerMinion>(),
				};
			}
			int spawnCycle = projTypes.Select(v => player.ownedProjectileCounts[v]).Sum();
			Projectile.NewProjectile(position, Vector2.Zero, projTypes[spawnCycle % 3], damage, knockBack, player.whoAmI);
			return false;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.UseSound = new LegacySoundStyle(2, 113);
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
		private MotionBlurDrawer blurHelper;

		internal override int BuffId => BuffType<DeadlySphereMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.DeadlySphere;

		internal override int? FiredProjectileId => null;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.DeadlySphere") + " (AoMM Version)");
			Main.projFrames[projectile.type] = 21;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			attackFrames = 90;
			targetSearchDistance = 950;
			blurHelper = new MotionBlurDrawer(5);
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 15;
			hsHelper.projectileVelocity = 6;
			hsHelper.targetInnerRadius = 96;
			hsHelper.targetOuterRadius = 160;
			hsHelper.targetShootProximityRadius = 112;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(5, 10);
			projectile.rotation += projectile.velocity.Length() * 0.05f;
			Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 0.5f);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int framesSinceShoot = animationFrame - hsHelper.lastShootFrame;
			if(framesSinceShoot > 20 && framesSinceShoot % 15 < 10)
			{
				// dash at the target
				isDashing = true;
				if(dashVector == default)
				{
					dashVector = vectorToTargetPosition;
					dashVector.SafeNormalize();
					dashVector *= (hsHelper.travelSpeed + 2);
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
				for (int k = 0; k < blurHelper.BlurLength; k++)
				{
					if(!blurHelper.GetBlurPosAndColor(k, lightColor, out Vector2 blurPos, out Color blurColor)) { break; }
					blurPos = blurPos - Main.screenPosition + origin;
					spriteBatch.Draw(texture, blurPos, bounds, blurColor, r, origin, 1, effects, 0);
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
			blurHelper.Update(projectile.Center, isDashing);

		}
	}
	public class DeadlySphereClingerMinion : HoverShooterMinion
	{
		bool isClinging = false;
		float clingDistanceTolerance = 24f;
		Vector2 targetOffset = default;

		internal override int BuffId => BuffType<DeadlySphereMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.DeadlySphere;

		internal override int? FiredProjectileId => null;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.DeadlySphere") + " (AoMM Version)");
			Main.projFrames[projectile.type] = 21;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			attackFrames = 90;
			targetSearchDistance = 950;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 15;
			// this should probably use a different base class instead of 
			// very small parameters for target radius, but...
			hsHelper.targetInnerRadius = 0;
			hsHelper.targetOuterRadius = 0;
			hsHelper.travelSpeedAtTarget = 15;
		}

		public override void OnSpawn()
		{
			// cut down damage since it's got such a high rate of fire
			projectile.damage = (int)(projectile.damage * 0.67f);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(14, 17);
			if(isClinging)
			{
				projectile.rotation += MathHelper.TwoPi / 15;
			} else
			{
				projectile.rotation += MathHelper.TwoPi / 60;
			}
			Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 0.5f);
		}

		public override Vector2? FindTarget()
		{
			Vector2? target = base.FindTarget();
			if (targetNPCIndex is int idx && oldTargetNpcIndex != idx)
			{
				// choose a new preferred location on the enemy to cling to
				targetOffset = new Vector2(
					Main.rand.Next(Main.npc[idx].width) - Main.npc[idx].width / 2,
					Main.rand.Next(Main.npc[idx].height) - Main.npc[idx].height / 2);
			}
			if(target is Vector2 tgt)
			{
				return tgt + targetOffset;
			} else
			{
				return null;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(vectorToTargetPosition.Length() < clingDistanceTolerance)
			{
				// slowly decrease the distance that we're allowed to cling
				if(clingDistanceTolerance > 8f)
				{
					clingDistanceTolerance *= 0.995f;
				}
				isClinging = true;
				// move in a small circle around the cling location
				Vector2 clingRotation = (animationFrame * MathHelper.TwoPi / 60f).ToRotationVector2() * 8;
				projectile.Center += vectorToTargetPosition + clingRotation;
				projectile.velocity = Vector2.Zero;
			} else
			{
				isClinging = false;
				clingDistanceTolerance = 24;
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			float oldRotation = projectile.rotation;
			base.IdleMovement(vectorToIdlePosition);
			projectile.rotation = oldRotation;
			isClinging = false;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// need to draw sprites manually to get spinning animation centered
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : 0;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition, bounds, lightColor, r, origin, 1, effects, 0);
			return false;
		}
	}
	public class DeadlySphereFireMinion : HoverShooterMinion
	{

		internal override int BuffId => BuffType<DeadlySphereMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.DeadlySphere;
		internal override LegacySoundStyle ShootSound => new LegacySoundStyle(2, 34).WithVolume(.5f);

		internal override int? FiredProjectileId => null;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.DeadlySphere") + " (AoMM Version)");
			Main.projFrames[projectile.type] = 21;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			attackFrames = 90;
			targetSearchDistance = 950;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 15;
			hsHelper.projectileVelocity = 6;
			hsHelper.targetInnerRadius = 96;
			hsHelper.targetOuterRadius = 160;
			hsHelper.targetShootProximityRadius = 112;
		}
		public override void OnSpawn()
		{
			// cut down damage since it's got such a high rate of fire
			projectile.damage = (int)(projectile.damage * 0.5f);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(17, 21);
			if(vectorToTarget == null || animationFrame - hsHelper.lastShootFrame > 60)
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
			int framesSinceShoot = animationFrame - hsHelper.lastShootFrame;
			if (framesSinceShoot < 60 && framesSinceShoot % 6 == 0)
			{
				if(targetNPCIndex is int idx) 
				{
					vectorToTargetPosition += Main.npc[idx].velocity / 4;
				}
				projectile.rotation = vectorToTargetPosition.ToRotation() + MathHelper.Pi/4;
				Vector2 lineOfFire = vectorToTargetPosition;
				lineOfFire.Normalize();
				lineOfFire *= hsHelper.projectileVelocity;
				lineOfFire += projectile.velocity / 3;
				for(int i = 0; i < 3; i++)
				{
					if(player.whoAmI == Main.myPlayer)
					{
						hsHelper.FireProjectile(lineOfFire, ProjectileType<DeadlySphereFire>(), (framesSinceShoot % 12) + i);
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
	}
}
