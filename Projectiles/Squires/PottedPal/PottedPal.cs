using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.PottedPal
{
	public class PottedPalMinionBuff : MinionBuff
	{
		public PottedPalMinionBuff() : base(ProjectileType<PottedPalMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Potted Pal");
			Description.SetDefault("A friendly plant will follow your orders!");
		}
	}

	public class PottedPalMinionItem : SquireMinionItem<PottedPalMinionBuff, PottedPalMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Potted Pal");
			Tooltip.SetDefault("Summons a squire\nA friendly plant will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
			item.damage = 51;
			item.value = Item.sellPrice(0, 5, 0, 0);
			item.rare = ItemRarityID.Pink;
		}
	}
	public class PottedPalSeedProjectile : ModProjectile
	{
		const int TIME_TO_LIVE = 60;
		private bool didCollide;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.timeLeft = TIME_TO_LIVE;
			projectile.tileCollide = true;
			projectile.penetrate = 1;
			projectile.friendly = true;
			projectile.localNPCHitCooldown = 20; // hit a little slower than usual
		}

		public override void AI()
		{
			if (TIME_TO_LIVE - projectile.timeLeft > 6)
			{
				projectile.velocity.Y += 0.5f;
			}
			projectile.rotation += projectile.velocity.X * 0.05f;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = false;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			didCollide = true;
			return true;
		}

		public override void Kill(int timeLeft)
		{
			if (projectile.owner == Main.myPlayer)
			{
				Projectile.NewProjectile(
					projectile.Center,
					-projectile.velocity,
					ProjectileType<PottedPalJrMinion>(),
					projectile.damage,
					projectile.knockBack,
					projectile.owner,
					ai1: didCollide ? 1 : 0); ;
			}
		}
	}

	public class PottedPalJrMinion : TransientMinion
	{

		private Vector2 spawnPos;
		private Vector2 idleDirection;

		internal override bool tileCollide => false;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.timeLeft = 12 * 60;
			attackThroughWalls = false;
			projectile.width = 16;
			projectile.height = 16;
			frameSpeed = 15;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			SpawnDust();
			spawnPos = projectile.Center;
			idleDirection = projectile.velocity;
			idleDirection.SafeNormalize();
			projectile.velocity = Vector2.Zero;
		}

		public override Vector2 IdleBehavior()
		{
			projectile.rotation = (projectile.Center - spawnPos).ToRotation() + MathHelper.PiOver2;
			Vector2 vectorToIdle = spawnPos - projectile.position;
			float offsetMagnitude = 16 + 8 * (float)Math.Sin(MathHelper.TwoPi * animationFrame / 60);
			return vectorToIdle + idleDirection * offsetMagnitude;
		}

		public override Vector2? FindTarget()
		{
			if(GetClosestEnemyToPosition(projectile.Center, 400f, true) is NPC targetNPC && 
				Vector2.DistanceSquared(targetNPC.Center, spawnPos) < 600 * 600)
			{
				return targetNPC.Center - projectile.Center;
			} else
			{
				return null;
			}
		}

		private void Move(Vector2 target)
		{
			int moveSpeed = 10;
			int inertia = 12;
			if(target.LengthSquared() > moveSpeed * moveSpeed)
			{
				target.Normalize();
				target *= moveSpeed;
			}
			projectile.velocity = (projectile.velocity * (inertia - 1) + target) / inertia;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			Move(vectorToIdlePosition);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			Move(vectorToTargetPosition);
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			projectile.tileCollide = vectorToTarget != null;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// draw chain
			Texture2D texture = Main.projectileTexture[projectile.type];
			ChainDrawer chainDrawer = new ChainDrawer(new Rectangle(0, 36, 16, 16), new Rectangle(0, 54, 16, 16));
			chainDrawer.DrawChain(spriteBatch, texture, spawnPos, projectile.Center);

			// draw dirt block
			Rectangle bounds = new Rectangle(0, 18 * 4, 16, 16);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			lightColor = Lighting.GetColor((int)spawnPos.X / 16, (int)spawnPos.Y / 16);
			spriteBatch.Draw(texture, spawnPos - Main.screenPosition,
				bounds, lightColor, 0, origin, 1, SpriteEffects.None, 0);
			return true;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, 2);
		}

		public override void Kill(int timeLeft)
		{
			base.Kill(timeLeft);
			SpawnDust();
		}
		private void SpawnDust()
		{
			for(int i = 0; i < 4; i++)
			{
				Dust.NewDust(projectile.TopLeft, 24, 24, 39);
			}
		}
	}

	public class PottedPalMinion : SquireMinion
	{
		internal override int BuffId => BuffType<PottedPalMinionBuff>();
		public PottedPalMinion() : base(ItemType<PottedPalMinionItem>()) { }
		protected int wingFrameCounter = 0;

		protected override float projectileVelocity => 12;

		static int hitCooldown = 6;
		static int cooldownCounter;

		protected override int SpecialDuration => 5; // very short
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Potted Pal");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 4;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			frameSpeed = 15;
			projectile.localNPCHitCooldown = 10;
			cooldownCounter = hitCooldown;
		}


		public override Vector2 IdleBehavior()
		{
			wingFrameCounter++;
			cooldownCounter++;
			return base.IdleBehavior();
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (vectorToIdle.Length() > 32)
			{
				Vector2 vectorFromPlayer = player.Center - projectile.Center;
				projectile.rotation = vectorFromPlayer.ToRotation() - (float)Math.PI / 2;
			}
			else
			{
				projectile.rotation = 0;
			}
			base.IdleMovement(vectorToIdlePosition);
		}


		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			projectile.velocity = -projectile.velocity;
			projectile.velocity.SafeNormalize();
			projectile.velocity *= ModifiedTargetedSpeed();
		}

		public override Vector2? FindTarget()
		{
			if (cooldownCounter >= hitCooldown)
			{
				return base.FindTarget();
			}
			return null;
		}
		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (cooldownCounter < hitCooldown)
			{
				return;
			}
			// Cling to the closest enemy a little bit
			if (vectorToTargetPosition.Length() < 100f && GetClosestEnemyToPosition(syncedMouseWorld, 100f, true) is NPC target)
			{
				base.StandardTargetedMovement(target.Center - projectile.Center);
			}
			else
			{
				base.StandardTargetedMovement(vectorToTargetPosition);
			}
			Vector2 vectorFromPlayer = player.Center - projectile.Center;
			projectile.rotation = vectorFromPlayer.ToRotation() - (float)Math.PI / 2;
		}

		public override void OnStartUsingSpecial()
		{
			if(player.whoAmI == Main.myPlayer)
			{
				int projType = ProjectileType<PottedPalJrMinion>();
				if(player.ownedProjectileCounts[projType] > 1)
				{
					// "prune" the oldest child (heh, plants)
					Projectile oldestChild = Main.projectile.Where(p =>
							p.active && p.owner == Main.myPlayer && p.type == projType)
						.OrderBy(p => p.timeLeft)
						.FirstOrDefault();
					if(oldestChild != default)
					{
						oldestChild.Kill();
					}
				}
				Vector2 vector2Mouse = Vector2.DistanceSquared(projectile.Center, Main.MouseWorld) < 48 * 48 ?
					Main.MouseWorld - player.Center : Main.MouseWorld - projectile.Center;
				vector2Mouse.SafeNormalize();
				vector2Mouse *= ModifiedProjectileVelocity();
				Projectile.NewProjectile(
					projectile.Center,
					vector2Mouse,
					ProjectileType<PottedPalSeedProjectile>(),
					projectile.damage / 2,
					projectile.knockBack,
					player.whoAmI);
			}
		}


		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Vector2 center = projectile.Center;
			Vector2 vineEnd = center + vectorToIdle + new Vector2(0, 8);
			ChainDrawer chainDrawer = new ChainDrawer(new Rectangle(0, 36, 16, 16), new Rectangle(0, 54, 16, 16));
			chainDrawer.DrawChain(spriteBatch, Main.projectileTexture[projectile.type], center, vineEnd);
			return true;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float r;
			Vector2 pos;
			int wingFrame = (wingFrameCounter % 20) / 5;
			Texture2D potTexture = GetTexture(Texture + "_Pot");
			int frameHeight = potTexture.Height / 4;
			Rectangle bounds = new Rectangle(0, wingFrame * frameHeight, potTexture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			if (vectorToIdle.Length() > 16 || vectorToTarget is Vector2 target)
			{
				pos = projectile.Center + vectorToIdle + new Vector2(0, 8); // move pot down a bit;
				r = player.velocity.X * 0.05f;
			}
			else
			{
				pos = projectile.Center + new Vector2(0, 12);
				r = projectile.rotation;
			}
			lightColor = Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16);
			spriteBatch.Draw(potTexture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, SpriteEffects.None, 0);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (vectorToTarget is null)
			{
				projectile.frame = 0;
			}
			else
			{
				maxFrame = 2;
				base.Animate(minFrame, maxFrame);
			}
			projectile.spriteDirection = 1;
		}

		public override float ComputeIdleSpeed() => 14;

		public override float ComputeTargetedSpeed() => 15;

		public override float MaxDistanceFromPlayer() => 600f;
	}
}
