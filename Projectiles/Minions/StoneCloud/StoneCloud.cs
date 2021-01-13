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
			DisplayName.SetDefault("Stonecloud Staff");
			Tooltip.SetDefault("Summons an extremely dense cloud to fight for you!\nDeals high damage, but attacks very slowly");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 15;
			item.knockBack = 3.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 2, 0);
			item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Wood, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class StoneCloudMinion : HeadCirclingGroupAwareMinion<StoneCloudMinionBuff>, IGroundAwareMinion
	{
		new int animationFrame = 0;
		int stoneStartFrame;
		bool didHitGround = false;
		bool isStone = false;
		int shockwaveStartFrame;
		private GroundAwarenessHelper gHelper;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Stone Cloud");
			Main.projFrames[projectile.type] = 6;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// hit wide on either side
			projHitbox.X -= 8;
			projHitbox.Width += 16;
			return base.Colliding(projHitbox, targetHitbox);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 32;
			drawOffsetX = (projectile.width - 40) / 2;
			attackFrames = 60;
			animationFrame = 0;
			idleInertia = 8;
			frameSpeed = 5;
			projectile.localNPCHitCooldown = 30;
			gHelper = new GroundAwarenessHelper(this);
		}
		public override void OnSpawn()
		{
			// give a hidden damage boost to make up for low attack rate
			projectile.damage = (int)(projectile.damage * 1.5f);
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

		public override Vector2 IdleBehavior()
		{
			animationFrame++;
			projectile.friendly = isStone;
			Vector2 vectorToIdle = base.IdleBehavior();
			int framesAsStone = animationFrame - stoneStartFrame;
			if(didHitGround && shockwaveStartFrame <= stoneStartFrame)
			{
				shockwaveStartFrame = animationFrame;
			}
			if(didHitGround && animationFrame - shockwaveStartFrame == 5)
			{
				SpawnShockwaveDust(projectile.Bottom + new Vector2(0, -2));
			}
			if(isStone && (framesAsStone > 60 || (vectorToIdle.Y <= -240 && framesAsStone > 30)))
			{
				// un-stone if the projectile gets too far below the player
				isStone = false;
				didHitGround = false;
				DoStoneDust();
			}
			return vectorToIdle;
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
			float velocity = 8;
			float initialRadius = 16;
			for(float i = -MathHelper.Pi/16; i < 17 * MathHelper.Pi /16; i += MathHelper.Pi/32)
			{
				Vector2 angle = i.ToRotationVector2();
				angle.Y *= -0.7f;
				Vector2 pos = center + angle * initialRadius;
				int dustIdx = Dust.NewDust(pos, 1, 1, DustType<ShockwaveDust>());
				Main.dust[dustIdx].velocity = angle * velocity;
				Main.dust[dustIdx].scale = 1f;
				Main.dust[dustIdx].alpha = 128;

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
			projectile.velocity.Y = 12;
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
			if (IsMyTurn() && Math.Abs(vectorAbove.X) <= 16 && Math.Abs(vectorAbove.Y) <= 16 && animationFrame - stoneStartFrame > 90)
			{
				DoStoneDust();
				isStone = true;
				stoneStartFrame = animationFrame;
				if(targetNPCIndex is int idx && Main.npc[idx].active)
				{
					//// approximately home in
					//float targetX = Main.npc[idx].velocity.X;
					//float projX = Math.Sign(targetX) * Math.Min(12, Math.Abs(targetX));
					//projectile.velocity.X = projX + (Main.rand.Next() - 0.5f)/4;
					projectile.velocity.X = 0;
				}
				return;
			}
			if(vectorAbove.Y > 16)
			{
				gHelper.DropThroughPlatform();
			}
			int speed = Math.Abs(vectorAbove.X) < 64 ? 12 : 9;
			DistanceFromGroup(ref vectorAbove);
			vectorAbove.SafeNormalize();
			vectorAbove *= speed;
			int inertia = 16;
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorAbove) / inertia;
		}
	}
}
