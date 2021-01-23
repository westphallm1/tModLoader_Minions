using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.StoneCloud
{
	public class StoneCloudMinionBuff : MinionBuff
	{
		public StoneCloudMinionBuff() : base(ProjectileType<StoneCloudMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
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
			item.damage = 34;
			item.knockBack = 5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 2, 0);
			item.rare = ItemRarityID.LightRed;
		}
	}

	public class StoneCloudMinion : HeadCirclingGroupAwareMinion
	{
		protected override int BuffId => BuffType<StoneCloudMinionBuff>();

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
			Main.projFrames[projectile.type] = 6;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(animationFrame - shockwaveStartFrame < ShockwaveTotalFrames && SemiCircleColliding(targetHitbox))
			{
				return true;
			} else
			{
				return base.Colliding(projHitbox, targetHitbox);
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
			projectile.width = 24;
			projectile.height = 32;
			drawOffsetX = (projectile.width - 40) / 2;
			attackFrames = 45;
			animationFrame = 0;
			idleInertia = 8;
			frameSpeed = 5;
			projectile.localNPCHitCooldown = 10;
			gHelper = new GroundAwarenessHelper(this);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			minFrame = isStone ? 3 : 0;
			maxFrame = isStone ? 6 : 3;
			if(!isStone && Math.Abs(projectile.velocity.X) > 1)
			{
				projectile.spriteDirection = -Math.Sign(projectile.velocity.X);
			}
			base.Animate(minFrame, maxFrame);
		}

		public override void OnSpawn()
		{
			defaultKnockback = projectile.knockBack;
		}

		public override Vector2 IdleBehavior()
		{
			projectile.friendly = isStone;
			Vector2 vectorToIdle = base.IdleBehavior();
			int framesAsStone = animationFrame - stoneStartFrame;
			doShockwaveCalculations();
			if(isStone && (framesAsStone > 60 || (!didHitGround && framesAsStone > 40)))
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
			projectile.knockBack = shockwaveFramesElapsed < ShockwaveTotalFrames ? defaultKnockback + 2 : defaultKnockback;
			if(shockwaveFramesElapsed > ShockwaveMaxSpeedFrames)
			{
				shockwaveHitboxSpeed *= ShockwaveDecay;
			}
			shockwaveHitboxRadius += shockwaveHitboxSpeed;

			if(didHitGround && shockwaveStartFrame <= stoneStartFrame)
			{
				shockwaveStartFrame = animationFrame;
				shockwaveHitboxRadius = 0;
				shockwaveHitboxSpeed = 0;
			}
			if(isStone && animationFrame - shockwaveStartFrame == 3)
			{
				shockwaveStartPosition = projectile.Bottom;
				SpawnShockwaveDust(projectile.Bottom + new Vector2(0, -2));
				shockwaveHitboxRadius = ShockwaveInitialRadius;
				shockwaveHitboxSpeed = ShockwaveSpeed;
			}
		}

		public override void OnHitTarget(NPC target)
		{
			if(isStone && shockwaveStartFrame <= stoneStartFrame)
			{
				shockwaveStartFrame = animationFrame;
				shockwaveHitboxRadius = 0;
				shockwaveHitboxSpeed = 0;
			}
			base.OnHitTarget(target);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if(isStone && Math.Abs(projectile.velocity.Y) < 1 && oldVelocity.Y > 0)
			{
				projectile.velocity.X = 0;
				if(!didHitGround)
				{
					didHitGround = true;
					Collision.HitTiles(projectile.BottomLeft, oldVelocity, 40, 8);
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
			for(float i = -0; i < MathHelper.TwoPi; i += MathHelper.Pi/32)
			{
				effectsIdx++;
				Vector2 angle = i.ToRotationVector2();
				angle.Y *= -1;
				Vector2 pos = center + angle * ShockwaveInitialRadius;
				int dustIdx = Dust.NewDust(pos, 1, 1, DustType<ShockwaveDust>());
				Main.dust[dustIdx].velocity = angle * ShockwaveSpeed;
				Main.dust[dustIdx].scale = 0.9f + Main.rand.NextFloat(0.2f);
				Main.dust[dustIdx].alpha = ShockwaveDustAlpha;
				if(effectsIdx % smokeStep == smokeStart)
				{
					int goreIdx = Gore.NewGore(pos, angle, Main.rand.Next(61, 64));
					Main.gore[goreIdx].scale = 0.5f;
				}

			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(isStone)
			{
				DoStoneMovement();
				return;
			}
			base.IdleMovement(vectorToIdlePosition);
		}

		private void DoStoneDust()
		{
			// cleverly hide the lack of a transformation animation with some well placed dust
			for(int i = 0; i < 10; i++)
			{
				int dustIdx = Dust.NewDust(projectile.Center, 8, 8, 192, newColor: Color.LightGray, Scale: 1.2f);
				Main.dust[dustIdx].noLight = false;
				Main.dust[dustIdx].noGravity = true;
			}

		}
		private void DoStoneMovement()
		{
			projectile.tileCollide = true;
			projectile.velocity.Y = FallSpeed;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = false;
			return true;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(isStone)
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
			projectile.friendly = false;
			for (int i = 16; i < targetAbove; i+= 8)
			{
				vectorAbove = new Vector2(vectorToTargetPosition.X, vectorToTargetPosition.Y - i);
				if (!Collision.CanHit(projectile.Center, 1, 1, projectile.Center + vectorAbove, 1, 1))
				{
					break;
				}
			}
			myTurnToDrop |= Math.Abs(vectorAbove.X) < 64 && IsMyTurn();
			if (myTurnToDrop && Math.Abs(vectorAbove.X) <= 16 && Math.Abs(vectorAbove.Y) <= 16 && animationFrame - stoneStartFrame > 80)
			{
				DoStoneDust();
				isStone = true;
				myTurnToDrop = false;
				stoneStartFrame = animationFrame;
				if(targetNPCIndex is int idx && Main.npc[idx].active)
				{
					//// approximately home in
					if(vectorToTargetPosition.Y > 16)
					{
						float xSpeed = FallSpeed * vectorToTargetPosition.X / vectorToTargetPosition.Y;
						projectile.velocity.X = Math.Min(xSpeed, 8);

					} else
					{
						projectile.velocity.X = 0;
					}
				}
				return;
			}
			if(vectorAbove.Y > 16)
			{
				gHelper.DropThroughPlatform();
			}
			int speed = Math.Abs(vectorAbove.X) < 64 ? 14 : 11;
			DistanceFromGroup(ref vectorAbove);
			vectorAbove.SafeNormalize();
			vectorAbove *= speed;
			int inertia = 16;
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorAbove) / inertia;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			hitDirection = Math.Sign(target.Center.X - projectile.Center.X);
		}
	}
}
