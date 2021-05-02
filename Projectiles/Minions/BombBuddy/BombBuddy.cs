using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.BombBuddy
{
	public class BombBuddyMinionBuff : MinionBuff
	{
		public BombBuddyMinionBuff() : base(ProjectileType<BombBuddyMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Bomb Buddy");
			Description.SetDefault("A bomb buddy will explode for you!");
		}
	}

	public class BombBuddyMinionItem : MinionItem<BombBuddyMinionBuff, BombBuddyMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Unstable Detonator");
			Tooltip.SetDefault("Summons a bomb buddy to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 24;
			item.knockBack = 5.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 5, 0);
			item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup("AmuletOfManyMinions:EvilBars", 12);
			recipe.AddIngredient(ItemID.Bomb, 20);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class BombBuddyMinion : SimpleGroundBasedMinion
	{

		internal override int BuffId => BuffType<BombBuddyMinionBuff>();
		private int slowFrameCount = 0;
		const int explosionRespawnTime = 90;
		const int explosionRadius = 64;
		const int explosionAttackRechargeTime = 120;
		int lastExplosionFrame = -explosionAttackRechargeTime;
		private Vector2 explosionLocation;
		private Dictionary<GroundAnimationState, (int, int?)> frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
		{
			[GroundAnimationState.FLYING] = (1, 1),
			[GroundAnimationState.JUMPING] = (1, 1),
			[GroundAnimationState.STANDING] = (0, 0),
			[GroundAnimationState.WALKING] = (2, 12),
		};

		private bool didJustRespawn => animationFrame - lastExplosionFrame == explosionRespawnTime;
		private bool canAttack => animationFrame - lastExplosionFrame >= explosionAttackRechargeTime;
		private bool isRespawning => animationFrame - lastExplosionFrame < explosionRespawnTime;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bomb Buddy");
			Main.projFrames[projectile.type] = 12;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 30;
			drawOriginOffsetY = -4;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// use a rectangular hitbox for the explosion. Easier than the alternative
			projHitbox = new Rectangle(
				(int)explosionLocation.X - explosionRadius,
				(int)explosionLocation.Y - explosionRadius,
				2 * explosionRadius,
				2 * explosionRadius);
			if (Vector2.DistanceSquared(explosionLocation, targetHitbox.Center.ToVector2()) < explosionRadius * explosionRadius)
			{
				return true;
			}
			return base.Colliding(projHitbox, targetHitbox);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (isRespawning)
			{
				// clamp to the player while respawning
				projectile.position = player.position;
				projectile.velocity = player.velocity;
			}
			else if (didJustRespawn)
			{
				projectile.position += vectorToIdle;
				projectile.velocity = player.velocity;
				DoRespawnEffects();
			}
			else
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}
		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(projectile.velocity.X) < 2 ? 1.25f : 8;
			float xMaxSpeed = 9.5f;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			projectile.velocity.X = (projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (isRespawning)
			{
				// clamp to the player while respawning
				projectile.position = player.position;
				projectile.velocity = player.velocity;
			}
			else if (vectorToTargetPosition.Length() < explosionRadius / 2 && !usingBeacon)
			{
				lastExplosionFrame = animationFrame;
				explosionLocation = projectile.Center;
				Main.PlaySound(SoundID.Item62, projectile.Center);
				DoExplosionEffects();
			}
			else
			{
				base.TargetedMovement(canAttack ? vectorToTargetPosition : vectorToIdle);
			}
		}

		public override Vector2? FindTarget()
		{

			return canAttack ? base.FindTarget() : null;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return !isRespawning;
		}

		private void DoRespawnEffects()
		{
			float goreVel = 0.4f;
			foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
			{
				int goreIdx = Gore.NewGore(projectile.Center, default, Main.rand.Next(61, 64));
				Main.gore[goreIdx].velocity *= goreVel;
				Main.gore[goreIdx].velocity += 0.5f * offset;
				Main.gore[goreIdx].scale = 0.75f;
				Main.gore[goreIdx].alpha = 128;
			}

		}
		private void DoExplosionEffects()
		{
			Vector2 position = projectile.position;
			int width = 22;
			int height = 22;
			for (int i = 0; i < 30; i++)
			{
				int dustIdx = Dust.NewDust(position, width, height, 31, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 1.4f;
			}
			for (int i = 0; i < 20; i++)
			{
				int dustIdx = Dust.NewDust(position, width, height, 6, 0f, 0f, 100, default, 3.5f);
				Main.dust[dustIdx].noGravity = true;
				Main.dust[dustIdx].velocity *= 7f;
				dustIdx = Dust.NewDust(position, width, height, 6, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 3f;
			}
			for (float goreVel = 0.4f; goreVel < 0.8f; goreVel += 0.4f)
			{
				foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
				{
					int goreIdx = Gore.NewGore(position, default, Main.rand.Next(61, 64));
					Main.gore[goreIdx].velocity *= goreVel;
					Main.gore[goreIdx].velocity += offset;
				}
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			if (state == GroundAnimationState.FLYING && animationFrame % 3 == 0)
			{
				int idx = Dust.NewDust(projectile.Bottom, 8, 8, 16, -projectile.velocity.X / 2, -projectile.velocity.Y / 2);
				Main.dust[idx].alpha = 112;
				Main.dust[idx].scale = .9f;
			}
		}

		public override void AfterMoving()
		{
			projectile.friendly = isRespawning && animationFrame - lastExplosionFrame <= 15;
		}
	}
}
