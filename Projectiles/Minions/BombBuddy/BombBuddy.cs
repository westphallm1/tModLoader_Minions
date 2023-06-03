using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.Audio;

namespace AmuletOfManyMinions.Projectiles.Minions.BombBuddy
{
	public class BombBuddyMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BombBuddyMinion>() };
	}

	public class BombBuddyMinionItem : MinionItem<BombBuddyMinionBuff, BombBuddyMinion>
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 24;
			Item.knockBack = 5.5f;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.buyPrice(0, 0, 5, 0);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddRecipeGroup(AoMMSystem.EvilBarRecipeGroup, 12).AddIngredient(ItemID.Bomb, 20).AddTile(TileID.Anvils).Register();
		}
	}

	public class BombBuddyMinion : SimpleGroundBasedMinion
	{

		public override int BuffId => BuffType<BombBuddyMinionBuff>();
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

		private bool didJustRespawn => AnimationFrame - lastExplosionFrame == explosionRespawnTime;
		private bool canAttack => AnimationFrame - lastExplosionFrame >= explosionAttackRechargeTime;
		private bool isRespawning => AnimationFrame - lastExplosionFrame < explosionRespawnTime;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 12;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 30;
			DrawOriginOffsetY = -4;
			attackFrames = 60;
			NoLOSPursuitTime = 300;
			StartFlyingHeight = 96;
			StartFlyingDist = 64;
			DefaultJumpVelocity = 4;
			MaxJumpVelocity = 12;
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
			return projHitbox.Intersects(targetHitbox);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (isRespawning)
			{
				// clamp to the player while respawning
				Projectile.position = Player.position;
				Projectile.velocity = Player.velocity;
			}
			else if (didJustRespawn)
			{
				Projectile.position += VectorToIdle;
				Projectile.velocity = Player.velocity;
				DoRespawnEffects();
			}
			else
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}
		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -Projectile.height && Math.Abs(vector.X) < StartFlyingHeight)
			{
				GHelper.DoJump(vector);
			}
			float xInertia = GHelper.stuckInfo.overLedge && !GHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 1.25f : 8;
			float xMaxSpeed = 9.5f;
			if (VectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = Player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (isRespawning)
			{
				// clamp to the player while respawning
				Projectile.position = Player.position;
				Projectile.velocity = Player.velocity;
			}
			else if (vectorToTargetPosition.Length() < explosionRadius / 2 && !UsingBeacon)
			{
				lastExplosionFrame = AnimationFrame;
				explosionLocation = Projectile.Center;
				SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
				DoExplosionEffects();
			}
			else
			{
				base.TargetedMovement(canAttack ? vectorToTargetPosition : VectorToIdle);
			}
		}

		public override Vector2? FindTarget()
		{

			return canAttack ? base.FindTarget() : null;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return !isRespawning;
		}

		private void DoRespawnEffects()
		{
			float goreVel = 0.4f;
			var source = Projectile.GetSource_FromThis();
			foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
			{
				int goreIdx = Gore.NewGore(source, Projectile.Center, default, Main.rand.Next(61, 64));
				Main.gore[goreIdx].velocity *= goreVel;
				Main.gore[goreIdx].velocity += 0.5f * offset;
				Main.gore[goreIdx].scale = 0.75f;
				Main.gore[goreIdx].alpha = 128;
			}

		}
		private void DoExplosionEffects()
		{
			Vector2 position = Projectile.position;
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
			var source = Projectile.GetSource_FromThis();
			for (float goreVel = 0.4f; goreVel < 0.8f; goreVel += 0.4f)
			{
				foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
				{
					int goreIdx = Gore.NewGore(source, position, default, Main.rand.Next(61, 64));
					Main.gore[goreIdx].velocity *= goreVel;
					Main.gore[goreIdx].velocity += offset;
				}
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = GHelper.DoGroundAnimation(frameInfo, base.Animate);
			if (state == GroundAnimationState.FLYING && AnimationFrame % 3 == 0)
			{
				int idx = Dust.NewDust(Projectile.Bottom, 8, 8, 16, -Projectile.velocity.X / 2, -Projectile.velocity.Y / 2);
				Main.dust[idx].alpha = 112;
				Main.dust[idx].scale = .9f;
			}
		}

		public override void AfterMoving()
		{
			Projectile.friendly = isRespawning && AnimationFrame - lastExplosionFrame <= 15;
		}
	}
}
