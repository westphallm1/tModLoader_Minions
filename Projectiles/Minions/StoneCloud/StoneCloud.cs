using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.StoneCloud
{
	public class StoneCloudMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<StoneCloudMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Stonecloud");
			Description.SetDefault("An extremely dense cloud will fight for you!");
		}
	}

	public class StoneCloudMinionItem : MinionItem<StoneCloudMinionBuff, StoneCloudMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Cloud in a Boulder");
			Tooltip.SetDefault("Summons an extremely dense cloud to fight for you!\nDeals high damage, but attacks very slowly");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 34;
			Item.knockBack = 5f;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.buyPrice(0, 0, 2, 0);
			Item.rare = ItemRarityID.LightRed;
		}
	}

	public class StoneCloudMinion : HeadCirclingGroupAwareMinion
	{
		internal override int BuffId => BuffType<StoneCloudMinionBuff>();

		private static int ShockwaveSpeed = 8;
		private static int ShockwaveInitialRadius = 16;
		public static int ShockwaveTotalFrames = 25;
		public static int ShockwaveMaxSpeedFrames = 5;
		public static int ShockwaveDustAlpha = 60;
		public static float ShockwaveDecay = 0.85f;

		private float shockwaveHitboxRadius;
		private float shockwaveHitboxSpeed;
		int stoneStartFrame;
		bool didHitGround = false;
		bool isStone = false;
		private bool myTurnToDrop;
		int shockwaveStartFrame = -ShockwaveTotalFrames;
		const int FallSpeed = 12;
		private GroundAwarenessHelper gHelper;
		private float defaultKnockback;
		private Vector2 shockwaveStartPosition;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Stone Cloud");
			Main.projFrames[Projectile.type] = 6;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (animationFrame - shockwaveStartFrame < ShockwaveTotalFrames && SemiCircleColliding(targetHitbox))
			{
				return true;
			}
			else
			{
				return projHitbox.Intersects(targetHitbox);
			}
		}

		private bool SemiCircleColliding(Rectangle targetHitbox)
		{
			// just check the rectangle
			Rectangle approxRectangle = new Rectangle(
				(int)(shockwaveStartPosition.X - shockwaveHitboxRadius),
				(int)(shockwaveStartPosition.Y - shockwaveHitboxRadius),
				2 * (int)shockwaveHitboxRadius,
				2 * (int)shockwaveHitboxRadius);
			return approxRectangle.Intersects(targetHitbox) && Collision.CanHitLine(shockwaveStartPosition, 1, 1, targetHitbox.Center.ToVector2(), 1, 1);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 32;
			DrawOffsetX = (Projectile.width - 40) / 2;
			circleHelper.idleBumbleFrames = 90;
			circleHelper.idleBumbleRadius = 96;
			bumbleSpriteDirection = -1;
			attackFrames = 45;
			animationFrame = 0;
			idleInertia = 8;
			frameSpeed = 5;
			Projectile.localNPCHitCooldown = 10;
			gHelper = new GroundAwarenessHelper(this);
			pathfinder.modifyPath = gHelper.ModifyPathfinding;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			minFrame = isStone ? 3 : 0;
			maxFrame = isStone ? 6 : 3;
			if (!isStone && Math.Abs(Projectile.velocity.X) > 1 && vectorToTarget != null)
			{
				Projectile.spriteDirection = -Math.Sign(Projectile.velocity.X);
			}
			base.Animate(minFrame, maxFrame);
		}

		public override void OnSpawn()
		{
			defaultKnockback = Projectile.knockBack;
		}

		public override Vector2 IdleBehavior()
		{
			dealsContactDamage = isStone;
			Vector2 vectorToIdle = base.IdleBehavior();
			int framesAsStone = animationFrame - stoneStartFrame;
			doShockwaveCalculations();
			if (isStone && (framesAsStone > 60 || (!didHitGround && framesAsStone > 40)))
			{
				// un-stone if the projectile gets too far below the player
				isStone = false;
				didHitGround = false;
				DoStoneDust();
			}
			return vectorToIdle;
		}

		private void doShockwaveCalculations()
		{
			int shockwaveFramesElapsed = animationFrame - shockwaveStartFrame;
			Projectile.knockBack = shockwaveFramesElapsed < ShockwaveTotalFrames ? defaultKnockback + 2 : defaultKnockback;
			if (shockwaveFramesElapsed > ShockwaveMaxSpeedFrames)
			{
				shockwaveHitboxSpeed *= ShockwaveDecay;
			}
			shockwaveHitboxRadius += shockwaveHitboxSpeed;

			if (didHitGround && shockwaveStartFrame <= stoneStartFrame)
			{
				shockwaveStartFrame = animationFrame;
				shockwaveHitboxRadius = 0;
				shockwaveHitboxSpeed = 0;
			}
			if (isStone && animationFrame - shockwaveStartFrame == 3)
			{
				shockwaveStartPosition = Projectile.Bottom;
				SpawnShockwaveDust(Projectile.Bottom + new Vector2(0, -2));
				shockwaveHitboxRadius = ShockwaveInitialRadius;
				shockwaveHitboxSpeed = ShockwaveSpeed;
			}
		}

		public override void OnHitTarget(NPC target)
		{
			if (isStone && shockwaveStartFrame <= stoneStartFrame)
			{
				shockwaveStartFrame = animationFrame;
				shockwaveHitboxRadius = 0;
				shockwaveHitboxSpeed = 0;
			}
			base.OnHitTarget(target);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (isStone && Math.Abs(Projectile.velocity.Y) < 1 && oldVelocity.Y > 0)
			{
				Projectile.velocity.X = 0;
				if (!didHitGround)
				{
					didHitGround = true;
					Collision.HitTiles(Projectile.BottomLeft, oldVelocity, 40, 8);
					SoundEngine.PlaySound(new LegacySoundStyle(3, 7), Projectile.position);
				}
			}
			return false;
		}

		// visual effect for spawning shockwave
		private void SpawnShockwaveDust(Vector2 center)
		{
			int effectsIdx = 0;
			int smokeStep = Main.rand.Next(10, 14);
			int smokeStart = Main.rand.Next(4);
			var source = Projectile.GetSource_FromThis();
			for (float i = -0; i < MathHelper.TwoPi; i += MathHelper.Pi / 32)
			{
				effectsIdx++;
				Vector2 angle = i.ToRotationVector2();
				angle.Y *= -1;
				Vector2 pos = center + angle * ShockwaveInitialRadius;
				int dustIdx = Dust.NewDust(pos, 1, 1, DustType<ShockwaveDust>());
				Main.dust[dustIdx].velocity = angle * ShockwaveSpeed;
				Main.dust[dustIdx].scale = 0.9f + Main.rand.NextFloat(0.2f);
				Main.dust[dustIdx].alpha = ShockwaveDustAlpha;
				if (effectsIdx % smokeStep == smokeStart)
				{
					int goreIdx = Gore.NewGore(source, pos, angle, Main.rand.Next(61, 64));
					Main.gore[goreIdx].scale = 0.5f;
				}

			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (isStone)
			{
				DoStoneMovement();
				return;
			}
			base.IdleMovement(vectorToIdlePosition);
		}

		private void DoStoneDust()
		{
			// cleverly hide the lack of a transformation animation with some well placed dust
			for (int i = 0; i < 10; i++)
			{
				int dustIdx = Dust.NewDust(Projectile.Center, 8, 8, 192, newColor: Color.LightGray, Scale: 1.2f);
				Main.dust[dustIdx].noLight = false;
				Main.dust[dustIdx].noGravity = true;
			}

		}
		private void DoStoneMovement()
		{
			Projectile.tileCollide = true;
			Projectile.velocity.Y = FallSpeed;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = false;
			return true;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (isStone)
			{
				if (vectorToTargetPosition.Y > 16)
				{
					gHelper.DropThroughPlatform();
				}
				DoStoneMovement();
				return;
			}
			int targetAbove = 80;
			Vector2 vectorAbove = vectorToTargetPosition;
			// only check for exact position once close to target
			if (vectorToTargetPosition.LengthSquared() < 256 * 256)
			{
				for (int i = 16; i < targetAbove; i += 8)
				{
					vectorAbove = new Vector2(vectorToTargetPosition.X, vectorToTargetPosition.Y - i);
					if (!Collision.CanHit(Projectile.Center, 1, 1, Projectile.Center + vectorAbove, 1, 1))
					{
						break;
					}
				}
			}
			myTurnToDrop |= Math.Abs(vectorAbove.X) < 64 && IsMyTurn();
			if (myTurnToDrop && Math.Abs(vectorAbove.X) <= 16 && Math.Abs(vectorAbove.Y) <= 16 && animationFrame - stoneStartFrame > 80)
			{
				DoStoneDust();
				isStone = true;
				myTurnToDrop = false;
				stoneStartFrame = animationFrame;
				if (targetNPCIndex is int idx && Main.npc[idx].active)
				{
					//// approximately home in
					if (vectorToTargetPosition.Y > 16)
					{
						float xSpeed = FallSpeed * vectorToTargetPosition.X / vectorToTargetPosition.Y;
						Projectile.velocity.X = Math.Min(xSpeed, 8);

					}
					else
					{
						Projectile.velocity.X = 0;
					}
				}
				return;
			}
			if (vectorAbove.Y > 16)
			{
				gHelper.DropThroughPlatform();
			}
			int speed = Math.Abs(vectorAbove.X) < 64 ? 14 : 11;
			DistanceFromGroup(ref vectorAbove);
			vectorAbove.SafeNormalize();
			vectorAbove *= speed;
			int inertia = 16;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorAbove) / inertia;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			hitDirection = Math.Sign(target.Center.X - Projectile.Center.X);
		}
	}
}
