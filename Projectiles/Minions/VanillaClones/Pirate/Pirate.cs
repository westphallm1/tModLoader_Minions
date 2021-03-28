using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones.Pirate
{
	public class PirateMinionBuff : MinionBuff
	{
		public PirateMinionBuff() : base(ProjectileType<PirateMinion>(), ProjectileType<ParrotMinion>(), ProjectileType<PirateDeadeyeMinion>(), ProjectileType<FlyingDutchmanMinion>()) { }
			
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.PirateMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.PirateMinion"));
		}

	}

	public class PirateMinionItem : VanillaCloneMinionItem<PirateMinionBuff, PirateMinion>
	{
		public int[] projTypes = new int[]
		{
			ProjectileType<PirateDeadeyeMinion>(),
			ProjectileType<ParrotMinion>(),
			ProjectileType<FlyingDutchmanMinion>(),
			ProjectileType<PirateMinion>(),
		};
		int spawnCycle = 0;
		internal override int VanillaItemID => ItemID.PirateStaff;

		internal override string VanillaItemName => "PirateStaff";

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			item.shoot = projTypes[spawnCycle++ % 4];
			return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
		}
	}
	public class PirateDeadeyeBullet : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.BulletDeadeye;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.friendly = true;
			projectile.penetrate = 1;
			projectile.tileCollide = true;
			projectile.timeLeft = 60;
		}

		public override void AI()
		{
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
	}
	public class PirateCannonball : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.CannonballFriendly;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.friendly = true;
			projectile.penetrate = 1;
			projectile.tileCollide = true;
			projectile.timeLeft = 60;
		}

		public override void AI()
		{
			base.AI();
			if(projectile.timeLeft < 50 && projectile.velocity.Y < 16)
			{
				projectile.velocity.Y += 0.5f;
			}
			projectile.rotation += 0.01f;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			// damage comes from explosion rather than projectile itself
			damage = 0;
			knockback = 0;
			crit = false;
		}

		public override void Kill(int timeLeft)
		{
			Vector2 position = projectile.Center;
			int width = 22;
			int height = 22;
			for (int i = 0; i < 20; i++)
			{
				Dust.NewDust(position, width, height, 31, 0f, 0f, 100, default, 1.5f);
			}
			for (int i = 0; i < 10; i++)
			{
				int dustIdx = Dust.NewDust(position, width, height, 6, 0f, 0f, 100, default, 2f);
				Main.dust[dustIdx].noGravity = true;
				Main.dust[dustIdx].velocity *= 3f;
				dustIdx = Dust.NewDust(position, width, height, 6, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 1.5f;
			}
			for (float goreVel = 0.25f; goreVel < 0.5f; goreVel += 0.25f)
			{
				foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
				{
					int goreIdx = Gore.NewGore(position, default, Main.rand.Next(61, 64));
					Main.gore[goreIdx].velocity *= goreVel;
					Main.gore[goreIdx].velocity += offset;
				}
			}
			if(projectile.owner == Main.myPlayer)
			{
				Projectile.NewProjectile(
					projectile.Center,
					Vector2.Zero,
					ProjectileType<PirateCannonballExplosion>(),
					projectile.damage,
					projectile.knockBack,
					projectile.owner);
			}
		}
	}

	public class PirateCannonballExplosion: ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.CannonballFriendly;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 1;
			projectile.height = 1;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 12;
			projectile.tileCollide = false;
			projectile.timeLeft = 12;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			projHitbox = new Rectangle((int)projectile.Center.X - 32, (int)projectile.Center.Y - 32, 64, 64);
			return projHitbox.Intersects(targetHitbox);
		}
	}

	public abstract class BasePirateMinion : SimpleGroundBasedMinion
	{
		protected override int BuffId => BuffType<PirateMinionBuff>();

		protected Dictionary<GroundAnimationState, (int, int?)> frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
		{
			[GroundAnimationState.FLYING] = (10, 14),
			[GroundAnimationState.JUMPING] = (14, 14),
			[GroundAnimationState.STANDING] = (0, 0),
			[GroundAnimationState.WALKING] = (0, 4),
		};

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 15;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 26;
			drawOffsetX = -2;
			drawOriginOffsetY = -14;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			searchDistance = 850;
			maxJumpVelocity = 12;
		}
		public override void OnSpawn()
		{
			base.OnSpawn();
			projectile.damage = (int)(projectile.damage * 0.8f);
		}
		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(projectile.velocity.X) < 2 ? 1.25f : 7;
			int xMaxSpeed = 10;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			if (animationFrame - lastHitFrame > 10)
			{
				projectile.velocity.X = (projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
			else
			{
				projectile.velocity.X = Math.Sign(projectile.velocity.X) * xMaxSpeed * 0.75f;
			}
		}
	}

	public class PirateMinion : BasePirateMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.SoulscourgePirate;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.SoulscourgePirate") + " (AoMM Version)");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			// vanilla damage is high because vanilla pirate minion is really bad, so don't hit so frequently
			projectile.localNPCHitCooldown = 25;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(!gHelper.isFlying && vectorToTarget is Vector2 target && target.Length() < 48)
			{
				if(gHelper.didJustLand)
				{
					base.Animate(4, 7);
				} else
				{
					base.Animate(7, 10);
				}
			} else
			{
				GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			}
		}
	}
	public class PirateDeadeyeMinion : BasePirateMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.OneEyedPirate;
		int lastFiredFrame = 0;
		// don't get too close
		int preferredDistanceFromTarget = 96;
		private float shootAngle;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.OneEyedPirate") + " (AoMM Version)");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			attackFrames = 45;
			drawOriginOffsetY = -4;
		}

		protected override void IdleFlyingMovement(Vector2 vector)
		{
			if(animationFrame - lastFiredFrame < 10)
			{
				// don't fly while throwing the spear
				gHelper.didJustLand = false;
				gHelper.isFlying = false;
				gHelper.ApplyGravity();
			} else
			{
				base.IdleFlyingMovement(vector);
			}
		}

		private void FireGun()
		{
			int bulletVelocity = 24;
			lastFiredFrame = animationFrame;
			Main.PlaySound(new LegacySoundStyle(6, 1), projectile.position);
			if (player.whoAmI == Main.myPlayer)
			{
				Vector2 angleToTarget = (Vector2)vectorToTarget;
				angleToTarget.SafeNormalize();
				angleToTarget *= bulletVelocity;
				shootAngle = (MathHelper.TwoPi + angleToTarget.ToRotation()) % MathHelper.TwoPi;
				if(targetNPCIndex is int idx)
				{
					Vector2 targetVelocity = Main.npc[idx].velocity;
					if(targetVelocity.Length() > 32)
					{
						targetVelocity.Normalize();
						targetVelocity *= 32;
					}
					angleToTarget += targetVelocity / 4;
				}
				Projectile.NewProjectile(
					projectile.Center,
					angleToTarget,
					ProjectileType<PirateDeadeyeBullet>(),
					projectile.damage,
					projectile.knockBack,
					player.whoAmI);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{

			if (Math.Abs(vectorToTargetPosition.X) < 4 * preferredDistanceFromTarget &&
				Math.Abs(vectorToTargetPosition.Y) < 4 * preferredDistanceFromTarget &&
				animationFrame - lastFiredFrame >= attackFrames)
			{
				FireGun();
			}

			// don't move if we're in range-ish of the target
			if (Math.Abs(vectorToTargetPosition.X) < 2 * preferredDistanceFromTarget && Math.Abs(vectorToTargetPosition.X) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X = 0;
			}
			if(Math.Abs(vectorToTargetPosition.Y) < 2 * preferredDistanceFromTarget && Math.Abs(vectorToTargetPosition.Y) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.Y = 0;
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override void AfterMoving()
		{
			projectile.friendly = false;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (animationFrame - lastFiredFrame < 20)
			{
				if(shootAngle > 5 * MathHelper.PiOver4 && shootAngle < 7 * MathHelper.PiOver4)
				{
					projectile.frame = 4;
				} else if (shootAngle > MathHelper.PiOver4 && shootAngle < 3 * MathHelper.PiOver4)
				{
					projectile.frame = 6;
				} else
				{
					projectile.frame = 5;
				}
				if(!gHelper.didJustLand)
				{
					projectile.frame += 3;
				}
			} else
			{
				GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			}
			if (vectorToTarget is Vector2 target && Math.Abs(target.X) < 3 * preferredDistanceFromTarget)
			{
				projectile.spriteDirection = Math.Sign(target.X);
			} 
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float r = projectile.rotation;
			Vector2 pos = projectile.Center + new Vector2(drawOriginOffsetX, drawOriginOffsetY);
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(Texture);
			Texture2D maskTexture= GetTexture(base.Texture+ "_Mask");
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition, bounds, lightColor, r, origin, 1, effects, 0);
			spriteBatch.Draw(maskTexture, pos - Main.screenPosition, bounds, lightColor, r, origin, 1, effects, 0);
			return false;
		}
	}
	public class ParrotMinion : HeadCirclingGroupAwareMinion
	{
		private int framesSinceLastHit;
		private int cooldownAfterHitFrames = 16;
		protected override int BuffId => BuffType<PirateMinionBuff>();
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Parrot") + " (AoMM Version)");
			Main.projFrames[projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			attackFrames = 60;
			targetSearchDistance = 850;
			frameSpeed = 5;
			idleBumble = true;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			projectile.damage = (int)(projectile.damage * 0.8f);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			if(vectorToTarget is Vector2 target && target.Length() < 128)
			{
				base.Animate(4, 8);
			} else
			{
				base.Animate(0, 4);
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			projectile.spriteDirection = -Math.Sign(projectile.velocity.X);
			base.IdleMovement(vectorToIdlePosition);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			float inertia = 18;
			float speed = 13;
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
			projectile.spriteDirection = -Math.Sign(projectile.velocity.X);
		}

		public override void OnHitTarget(NPC target)
		{
			framesSinceLastHit = 0;
		}
	}

	public class FlyingDutchmanMinion : HoverShooterMinion
	{
		protected override int BuffId => BuffType<PirateMinionBuff>();

		internal override int? FiredProjectileId => ProjectileType<PirateCannonball>();
		internal override LegacySoundStyle ShootSound => SoundID.Item17;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.PirateCaptain") + " (AoMM Version)");
			Main.projFrames[projectile.type] = 4;
			IdleLocationSets.trailingInAir.Add(projectile.type);
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			frameSpeed = 15;
			projectile.width = 32;
			projectile.height = 32;
			drawOriginOffsetY = (32 - 46) / 2;
			drawOffsetX = (32 - 54) / 2;
			attackFrames = 75;
			targetSearchDistance = 850;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 12;
			hsHelper.projectileVelocity = 14;
			hsHelper.targetInnerRadius = 160;
			hsHelper.targetOuterRadius = 200;
			hsHelper.targetShootProximityRadius = 256;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if(projectile.velocity.X > 1)
			{
				projectile.spriteDirection = -1;
			} else if (projectile.velocity.X < -1)
			{
				projectile.spriteDirection = 1;
			}
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			idlePosition.X += -player.direction * (16 + IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, projectile));
			idlePosition.Y += -16 + 6 * (float)Math.Sin(MathHelper.TwoPi * animationFrame / 120);
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition.X = player.Top.X;
				idlePosition.Y = player.Top.Y - 16;
			}
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}
		internal override void AfterFiringProjectile()
		{
			base.AfterFiringProjectile();
			for (int i = 0; i < 10; i++)
			{
				int dustIdx = Dust.NewDust(projectile.Center - new Vector2(8, 8), 16, 16, 31, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 0.25f;
			}
			for (float goreVel = 0.2f; goreVel < 0.4f; goreVel += 0.2f)
			{
				foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
				{
					int goreIdx = Gore.NewGore(projectile.Center, default, Main.rand.Next(61, 64));
					Main.gore[goreIdx].velocity *= goreVel;
					Main.gore[goreIdx].velocity += offset;
					Main.gore[goreIdx].scale *= Main.rand.NextFloat(0.25f, 0.4f);
				}
			}
			Vector2 target = (Vector2)vectorToTarget;
			target.Normalize();
			target *= -4;
			projectile.velocity = target;
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(animationFrame - hsHelper.lastShootFrame > 6)
			{
				base.TargetedMovement(vectorToTargetPosition);
			}
		}
	}
}
