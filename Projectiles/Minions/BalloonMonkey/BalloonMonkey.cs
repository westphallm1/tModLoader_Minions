using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.Pirate;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.BalloonMonkey
{
	public class BalloonMonkeyMinionBuff : MinionBuff
	{
		public BalloonMonkeyMinionBuff() : base(ProjectileType<BalloonMonkeyMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Dart Monkeys");
			Description.SetDefault("Dart-throwing Monkeys will fight for you!");
		}
	}

	public class BalloonMonkeyMinionItem : MinionItem<BalloonMonkeyMinionBuff, BalloonMonkeyMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Staff of Darts");
			Tooltip.SetDefault("Summons a dart-throwing monkey to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 16;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.buyPrice(0, 0, 5, 0);
			Item.rare = ItemRarityID.Green;
		}
	}

	public class BalloonMonkeyBalloon : ModProjectile
	{
		int frame = -1;

		static int TIME_TO_LIVE = 80;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 3;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = TIME_TO_LIVE;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
		}

		public override void AI()
		{
			if(frame == -1)
			{
				frame = Main.rand.Next(3);
				Projectile.frame = frame;
			}
			Projectile.velocity *= 0.95f;
			float idleAngle = (float)Math.Sin(MathHelper.TwoPi * Projectile.timeLeft / 120);
			Projectile.position.Y += idleAngle / 2;
			Projectile.rotation = -MathHelper.Pi / 16 + MathHelper.Pi/8 * idleAngle;
			
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity = -oldVelocity;
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			lightColor.A = 128;
			int framesAlive = TIME_TO_LIVE - Projectile.timeLeft;
			float scale = framesAlive > 30 ? 1 : framesAlive / 30f;
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition, bounds, lightColor, r, origin, scale, effects, 0);
			return false;
		}

		public override void Kill(int timeLeft)
		{
			// todo more unique animation
			Vector2 position = Projectile.Center;
			int width = 22;
			int height = 22;
			for (int i = 0; i < 20; i++)
			{
				Dust.NewDust(position, width, height, 31, 0f, 0f, 100, default, 1.5f);
			}
			var source = Projectile.GetSource_Death();
			for (float goreVel = 0.25f; goreVel < 0.5f; goreVel += 0.25f)
			{
				foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
				{
					int goreIdx = Gore.NewGore(source, position, default, Main.rand.Next(61, 64));
					Main.gore[goreIdx].velocity *= goreVel;
					Main.gore[goreIdx].velocity += offset;
				}
			}
			if(Projectile.owner == Main.myPlayer)
			{
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					Vector2.Zero,
					// repurpose an existing explosion projectile
					ProjectileType<PirateCannonballExplosion>(),
					Projectile.damage * 2,
					Projectile.knockBack * 4,
					Projectile.owner);
			}
		}

	}

	public class BalloonMonkeyDart : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PoisonDartBlowgun;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.PygmySpear);
			base.SetDefaults();
			Projectile.timeLeft = 180;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			// float in the same approximate direction that the npc is travelling
			Vector2 launchVector = target.velocity;
			launchVector.SafeNormalize();
			launchVector *= 3f;
			if(Main.rand.Next(2) > 0)
			{
				// only called for owner, no need to check ownership
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					launchVector,
					ProjectileType<BalloonMonkeyBalloon>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner);
			}
		}
	}

	public class BalloonMonkeyMinion : SimpleGroundBasedMinion
	{
		internal override int BuffId => BuffType<BalloonMonkeyMinionBuff>();
		int lastFiredFrame = 0;
		// don't get too close
		int preferredDistanceFromTarget = 128;
		private Dictionary<GroundAnimationState, (int, int?)> frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
		{
			[GroundAnimationState.FLYING] = (10, 14),
			[GroundAnimationState.JUMPING] = (14, 14),
			[GroundAnimationState.STANDING] = (0, 0),
			[GroundAnimationState.WALKING] = (0, 4),
		};

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Balloon Monkey");
			Main.projFrames[Projectile.type] = 15;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 26;
			DrawOffsetX = -2;
			DrawOriginOffsetY = -14;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			searchDistance = 650;
			maxJumpVelocity = 12;
			dealsContactDamage = false;
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

		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -3 * Projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = 8;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			// only change speed if the target is a decent distance away
			if (Math.Abs(vector.X) < 4 && targetNPCIndex is int idx && Math.Abs(Main.npc[idx].velocity.X) < 7)
			{
				Projectile.velocity.X = Main.npc[idx].velocity.X;
			}
			else
			{
				Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
		}

		private void FireDart()
		{
			int dartVelocity = 15;
			lastFiredFrame = animationFrame;
			SoundEngine.PlaySound(new LegacySoundStyle(2, 17), Projectile.position);
			if (player.whoAmI == Main.myPlayer)
			{
				Vector2 angleToTarget = (Vector2)vectorToTarget;
				angleToTarget.SafeNormalize();
				angleToTarget *= dartVelocity;
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
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					VaryLaunchVelocity(angleToTarget),
					ProjectileType<BalloonMonkeyDart>(),
					Projectile.damage,
					Projectile.knockBack,
					player.whoAmI,
					ai0: targetNPCIndex ?? -1);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{

			if (Math.Abs(vectorToTargetPosition.X) < 4 * preferredDistanceFromTarget &&
				Math.Abs(vectorToTargetPosition.Y) < 4 * preferredDistanceFromTarget &&
				animationFrame - lastFiredFrame >= attackFrames)
			{
				FireDart();
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

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (animationFrame - lastFiredFrame < 5)
			{
				Projectile.frame = gHelper.didJustLand ? 4 : 7;
			} else if (animationFrame - lastFiredFrame < 10)
			{
				Projectile.frame = gHelper.didJustLand ? 5 : 8;
			} else if (animationFrame - lastFiredFrame < 15)
			{
				Projectile.frame = gHelper.didJustLand ? 6 : 9;
			} else
			{
				GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			}
			if (targetNPCIndex is int idx && animationFrame - lastFiredFrame < 30)
			{
				Projectile.spriteDirection = Math.Sign(Main.npc[idx].position.X - Projectile.position.X);
			} 
		}
	}
}
