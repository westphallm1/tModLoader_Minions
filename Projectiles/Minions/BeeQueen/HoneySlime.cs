using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.BeeQueen
{
	public class HoneySlime : BumblingTransientMinion
	{
		protected override int timeToLive => 60 * 3; // 3 seconds;
		protected override float inertia => didLand ? 1 : 12;

		protected override bool onAttackCooldown => false;
		protected override float idleSpeed => maxSpeed * 0.75f;
		protected override float searchDistance => 350f;
		protected override float noLOSSearchDistance => 350f;
		protected override float distanceToBumbleBack => 8000f; // don't bumble back

		int defaultMaxSpeed = 4;
		int defaultJumpVelocity = 6;
		int minFrame;
		bool didLand = false;

		GroundAwarenessHelper gHelper;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = -1;
			AttackThroughWalls = false;
			Projectile.tileCollide = true;
			minFrame = 2 * Main.rand.Next(3);
			gHelper = new GroundAwarenessHelper(this);
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = false;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.Y == 0 && oldVelocity.Y >= 0)
			{
				didLand = true;
			}
			if (Projectile.velocity.Y != 0 && Projectile.velocity.X == 0 && oldVelocity.X != 0)
			{
				initialVelocity = -initialVelocity;
			}
			return false;
		}

		public override Vector2 IdleBehavior()
		{
			if (maxSpeed == default)
			{
				maxSpeed = defaultMaxSpeed;
				initialVelocity = new Vector2(Math.Sign(Projectile.velocity.X) * maxSpeed, 0);
			}
			Projectile.velocity.Y += 0.5f;
			return base.IdleBehavior();
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (!didLand)
			{
				return;
			}
			if (vectorToTargetPosition.Y > 32 && gHelper.DropThroughPlatform())
			{
				didLand = false; // now falling through the air again
				return;
			}
			if (vectorToTargetPosition.Y < -4 * defaultJumpVelocity)
			{
				Projectile.velocity.Y = Math.Max(-12, vectorToTargetPosition.Y / 4);
			}
			else
			{
				Projectile.velocity.Y = -defaultJumpVelocity;
			}
			base.TargetedMovement(vectorToTargetPosition);
			didLand = false;
		}

		protected override void Move(Vector2 vector2Target, bool isIdle = false)
		{
			float oldY = Projectile.velocity.Y;
			float oldMaxSpeed = maxSpeed;
			maxSpeed = Math.Min(maxSpeed, Math.Abs(vector2Target.X / 6));
			maxSpeed = Math.Max(maxSpeed, 2);
			base.Move(vector2Target, isIdle);
			Projectile.velocity.Y = oldY; // y can only be modified by jump & gravity
			maxSpeed = oldMaxSpeed; // may accidentally jump over enemies if we let it move too fast
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			int direction = Math.Sign(Projectile.velocity.X);
			direction = direction == 0 ? 1 : direction;
			initialVelocity = new Vector2(direction * maxSpeed, 0);
			if (Main.rand.NextBool(10))
			{
				target.AddBuff(BuffID.Slow, 120);
			}
			base.OnHitNPC(target, damage, knockback, crit);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (didLand)
			{
				Projectile.velocity.Y = -defaultJumpVelocity;
				didLand = false;
			}
			base.IdleMovement(vectorToIdlePosition);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			minFrame = this.minFrame;
			maxFrame = minFrame + 2;
			base.Animate(minFrame, maxFrame);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 128);
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.velocity.X < 0 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r, bounds.GetOrigin(), 0.75f, effects, 0);
			return false;
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.Center, 16, 16, 153);
			}
		}
	}
}
