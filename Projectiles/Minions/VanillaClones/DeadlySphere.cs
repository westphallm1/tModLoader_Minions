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
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using AmuletOfManyMinions.Core;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class DeadlySphereMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] {
			ProjectileType<DeadlySphereMinion>(),
			ProjectileType<DeadlySphereClingerMinion>(),
			ProjectileType<DeadlySphereFireMinion>()
		};

		public override LocalizedText DisplayName => AoMMSystem.AppendAoMMVersion(Language.GetText("BuffName.DeadlySphere"));

		public override LocalizedText Description => Language.GetText("BuffDescription.DeadlySphere");
	}

	public class DeadlySphereMinionItem : VanillaCloneMinionItem<DeadlySphereMinionBuff, DeadlySphereMinion>
	{
		internal override int VanillaItemID => ItemID.DeadlySphereStaff;

		internal override string VanillaItemName => "DeadlySphereStaff";

		[CloneByReference] //projTypes is fine to be shared across instances
		public int[] projTypes;

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ApplyBuff(player);

			if (projTypes == null)
			{
				projTypes = new int[]
				{
					ProjectileType<DeadlySphereMinion>(),
					ProjectileType<DeadlySphereFireMinion>(),
					ProjectileType<DeadlySphereClingerMinion>(),
				};
			}
			int spawnCycle = projTypes.Select(v => player.ownedProjectileCounts[v]).Sum();
			var p = Projectile.NewProjectileDirect(source, position, Vector2.Zero, projTypes[spawnCycle % 3], damage, knockback, player.whoAmI);
			p.originalDamage = Item.damage;
			return false;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.UseSound = SoundID.Item113;
		}
	}

	/// <summary>
	/// Uses ai[0] to determine whether it's damaging or cosmetic
	/// </summary>
	public class DeadlySphereFire : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.EyeFire;
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Flames);
			Projectile.aiStyle = 0; // unset default flames AI
			base.SetDefaults();
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.timeLeft = 36;
		}
		public override void AI()
		{
			base.AI();
			Projectile.localAI[0]++;
			if(Projectile.localAI[0] < 4 || !Main.rand.NextBool(2))
			{
				return;
			}
			Projectile.friendly = Projectile.ai[0] == 0;
			float dustScale = Math.Min(1, 0.25f * (Projectile.localAI[0] - 3));
			int dustType = 135;
			int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100);
			Main.dust[dustId].scale *= 1.5f * dustScale;
			Main.dust[dustId].velocity.X *= 1.2f;
			Main.dust[dustId].velocity.Y *= 1.2f;
			Main.dust[dustId].noGravity = true;
			if (Main.rand.NextBool(3))
			{
				Main.dust[dustId].scale *= 2f;
				Main.dust[dustId].velocity.X *= 2f;
				Main.dust[dustId].velocity.Y *= 2f;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Frostburn, 600);
		}
	}

	public class DeadlySphereMinion : HoverShooterMinion
	{
		private bool isDashing;
		private Vector2 dashVector;
		private MotionBlurDrawer blurHelper;

		public override int BuffId => BuffType<DeadlySphereMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeadlySphere;

		public override LocalizedText DisplayName => AoMMSystem.AppendAoMMVersion(Language.GetText("ProjectileName.DeadlySphere"));

		internal override int? FiredProjectileId => null;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 21;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			attackFrames = 90;
			targetSearchDistance = 950;
			blurHelper = new MotionBlurDrawer(5);
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 15;
			hsHelper.projectileVelocity = 6;
			hsHelper.targetInnerRadius = 96;
			hsHelper.targetOuterRadius = 160;
			hsHelper.targetShootProximityRadius = 112;
			DealsContactDamage = true;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(5, 10);
			Projectile.rotation += Projectile.velocity.Length() * 0.05f;
			Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.5f);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int framesSinceShoot = AnimationFrame - hsHelper.lastShootFrame;
			if(framesSinceShoot > 20 && framesSinceShoot % 15 < 10)
			{
				// dash at the target
				isDashing = true;
				if(dashVector == default)
				{
					dashVector = vectorToTargetPosition;
					dashVector.SafeNormalize();
					dashVector *= (hsHelper.travelSpeed + 2);
					if(TargetNPCIndex is int idx)
					{
						dashVector += Main.npc[idx].velocity / 8;
					}
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
			float oldRotation = Projectile.rotation;
			base.IdleMovement(vectorToIdlePosition);
			Projectile.rotation = oldRotation;
			isDashing = false;
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

		public override void AfterMoving()
		{
			// left shift old position
			blurHelper.Update(Projectile.Center, isDashing);

		}
	}
	public class DeadlySphereClingerMinion : HoverShooterMinion
	{
		bool isClinging = false;
		float clingDistanceTolerance = 24f;
		Vector2 targetOffset = default;

		public override int BuffId => BuffType<DeadlySphereMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeadlySphere;

		public override LocalizedText DisplayName => AoMMSystem.AppendAoMMVersion(Language.GetText("ProjectileName.DeadlySphere"));

		internal override int? FiredProjectileId => null;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 21;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			attackFrames = 90;
			targetSearchDistance = 950;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 15;
			// this should probably use a different base class instead of 
			// very small parameters for target radius, but...
			hsHelper.targetInnerRadius = 0;
			hsHelper.targetOuterRadius = 0;
			hsHelper.travelSpeedAtTarget = 15;
			DealsContactDamage = true;
		}

		public override void OnSpawn()
		{
			// cut down damage since it's got such a high rate of fire
			Projectile.originalDamage = (int)(Projectile.originalDamage * 0.67f);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(14, 17);
			if(isClinging)
			{
				Projectile.rotation += MathHelper.TwoPi / 15;
			} else
			{
				Projectile.rotation += MathHelper.TwoPi / 60;
			}
			Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.5f);
		}

		public override Vector2? FindTarget()
		{
			Vector2? target = base.FindTarget();
			if (TargetNPCIndex is int idx && OldTargetNpcIndex != idx)
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
				Vector2 clingRotation = (AnimationFrame * MathHelper.TwoPi / 60f).ToRotationVector2() * 8;
				Projectile.Center += vectorToTargetPosition + clingRotation;
				Projectile.velocity = Vector2.Zero;
			} else
			{
				isClinging = false;
				clingDistanceTolerance = 24;
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			float oldRotation = Projectile.rotation;
			base.IdleMovement(vectorToIdlePosition);
			Projectile.rotation = oldRotation;
			isClinging = false;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			// need to draw sprites manually to get spinning animation centered
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : 0;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition, bounds, lightColor, r, bounds.GetOrigin(), 1, effects, 0);
			return false;
		}
	}
	public class DeadlySphereFireMinion : HoverShooterMinion
	{

		public override int BuffId => BuffType<DeadlySphereMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeadlySphere;

		public override LocalizedText DisplayName => AoMMSystem.AppendAoMMVersion(Language.GetText("ProjectileName.DeadlySphere"));

		internal override SoundStyle? ShootSound => SoundID.Item34 with { Volume = 0.5f };

		internal override int? FiredProjectileId => null;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 21;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
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
			Projectile.originalDamage = (int)(Projectile.originalDamage * 0.5f);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(17, 21);
			if(VectorToTarget == null || AnimationFrame - hsHelper.lastShootFrame > 60)
			{
				Projectile.rotation += MathHelper.TwoPi/60;
				if (Main.rand.NextBool(2))
				{
					for (float angle = 0; angle < MathHelper.TwoPi; angle += MathHelper.PiOver2)
					{
						if (!Main.rand.NextBool(2))
						{
							int dustType = new int[] { 226, 228, 75 }[Main.rand.Next(3)];
							Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, dustType);
							Vector2 rotationVector = (Projectile.rotation + MathHelper.PiOver4 + angle).ToRotationVector2();
							dust.position = Projectile.Center + rotationVector * 14.2f;
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
					Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, dustType);
					Vector2 rotationVector = (Projectile.rotation + MathHelper.PiOver4 + angle).ToRotationVector2();
					dust.position = Projectile.Center + rotationVector * 14.2f;
					dust.velocity = rotationVector;
					dust.scale = 0.6f + Main.rand.NextFloat() * 0.5f;
					dust.noGravity = true;
				}
			} 
			Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.5f);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			int framesSinceShoot = AnimationFrame - hsHelper.lastShootFrame;
			if (framesSinceShoot < 60 && framesSinceShoot % 6 == 0)
			{
				if(TargetNPCIndex is int idx) 
				{
					vectorToTargetPosition += Main.npc[idx].velocity / 4;
				}
				Projectile.rotation = vectorToTargetPosition.ToRotation() + MathHelper.Pi/4;
				Vector2 lineOfFire = vectorToTargetPosition;
				lineOfFire.Normalize();
				lineOfFire *= hsHelper.projectileVelocity;
				lineOfFire += Projectile.velocity / 3;
				for(int i = 0; i < 3; i++)
				{
					if(Player.whoAmI == Main.myPlayer)
					{
						hsHelper.FireProjectile(lineOfFire, ProjectileType<DeadlySphereFire>(), (framesSinceShoot % 12) + i);
					}
				}
				AfterFiringProjectile();
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			float oldRotation = Projectile.rotation;
			base.IdleMovement(vectorToIdlePosition);
			Projectile.rotation = oldRotation;
		}
	}
}
