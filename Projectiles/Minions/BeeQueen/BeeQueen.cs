using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.BeeQueen
{
	public class BeeQueenMinionBuff : MinionBuff
	{
		public BeeQueenMinionBuff() : base(ProjectileType<BeeQueenCounterMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Bumble Bombardier");
			Description.SetDefault("A bee assistant will fight for you!");
		}
	}

	public class BeeQueenMinionItem : MinionItem<BeeQueenMinionBuff, BeeQueenCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bee Queen's Crown");
			Tooltip.SetDefault("Summons a bee assistant to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 20;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 2, 0);
			item.rare = ItemRarityID.Green;
		}
	}

	/// <summary>
	/// uses ai[0] for the spawn frequency
	/// </summary>
	public class BeeQueenBucket : TransientMinion
	{

		private Vector2 rememberedEnemyAngle = new Vector2(0, -6);

		private int spawnFrequency => (int)projectile.ai[0];
		private int timeToLive = 360;
		private bool inSummoningMode = false;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
			Main.projFrames[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 22;
			projectile.height = 26;
			projectile.tileCollide = true;
			projectile.penetrate = -1;
			projectile.friendly = true;
			projectile.usesLocalNPCImmunity = true;
		}

		private void EnterSummoningMode()
		{
			if (inSummoningMode)
			{
				return;
			}
			inSummoningMode = true;
			projectile.netUpdate = true; //TODO investigate if this is enough
			projectile.friendly = false;
			projectile.timeLeft = timeToLive - (int)projectile.ai[0] - 1;
			projectile.velocity = Vector2.Zero;
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(projectile.Top, 16, 16, 153);
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			//Calling clientside
			EnterSummoningMode();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			EnterSummoningMode();
			return false;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = false;
			return true;
		}

		public override Vector2? FindTarget()
		{
			if (ClosestEnemyInRange(300f, projectile.Center, 0, false) is Vector2 target)
			{
				rememberedEnemyAngle = target;
				rememberedEnemyAngle.Normalize();
				rememberedEnemyAngle *= 7;
			};
			return null;
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (projectile.timeLeft % spawnFrequency == 0 && projectile.timeLeft > 60)
			{

				for (int i = 0; i < 3; i++)
				{
					Dust.NewDust(projectile.Top, 16, 16, 153);
				}
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						projectile.Center,
						rememberedEnemyAngle,
						ProjectileType<HoneySlime>(),
						projectile.damage,
						projectile.knockBack,
						player.whoAmI);
				}
			}
			if (projectile.timeLeft == 60)
			{
				for (int i = 0; i < 6; i++)
				{
					Dust.NewDust(projectile.Top, 16, 16, 153);
				}
			}
			projectile.velocity.Y += 0.5f;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			projectile.frame = projectile.timeLeft > 60 ? 0 : 1;
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(projectile.position, 1, 1, DustType<BucketDust>(), Main.rand.Next(6) - 3, -3);
			}
		}
	}
	public class BeeQueenCounterMinion : CounterMinion {
		
		protected override int BuffId => BuffType<BeeQueenMinionBuff>();
		protected override int MinionType => ProjectileType<BeeQueenMinion>();
	}

	public class BeeQueenMinion : EmpoweredMinion
	{
		protected override int BuffId => BuffType<BeeQueenMinionBuff>();
		int animationFrameCounter = 0;
		int reloadCycleLength => Math.Max(120, 300 - 20 * EmpowerCount);

		int reloadStartFrame = 0;
		protected override int dustType => 153;

		protected override int CounterType => ProjectileType<BeeQueenCounterMinion>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bee Bombardier");
			Main.projFrames[projectile.type] = 6;
			IdleLocationSets.trailingInAir.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 32;
			frameSpeed = 15;
			animationFrameCounter = 0;
			reloadStartFrame = -reloadCycleLength;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if (projectile.velocity.X >= 2)
			{
				projectile.spriteDirection = -1;
			}
			else if (projectile.velocity.X <= -2)
			{
				projectile.spriteDirection = 1;
			}
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			float idleAngle = (float)(2 * Math.PI * animationFrameCounter % 240) / 240;
			Vector2 idlePosition = player.Center;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, projectile);
			idlePosition.Y += -35 + 5 * (float)Math.Sin(idleAngle);
			if (!Collision.CanHit(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition = player.Center;
				idlePosition.X += 30 * -player.direction;
				idlePosition.Y += -35;
			}
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			animationFrameCounter++;
			if (animationFrameCounter - reloadStartFrame == reloadCycleLength)
			{
				player.AddBuff(BuffID.Honey, 60);
			}

			return vectorToIdlePosition;
		}

		private bool readyToAttack => animationFrameCounter - reloadStartFrame >= reloadCycleLength;

		public override Vector2? FindTarget()
		{
			if (!readyToAttack)
			{
				return null;
			}
			return base.FindTarget();
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int targetAbove = 80;
			Vector2 vectorAbove = vectorToTargetPosition;
			projectile.friendly = false;
			for (int i = 16; i < targetAbove; i++)
			{
				vectorAbove = new Vector2(vectorToTargetPosition.X, vectorToTargetPosition.Y - i);
				if (!Collision.CanHit(projectile.Center, 1, 1, projectile.Center + vectorAbove, 1, 1))
				{
					break;
				}
			}
			if (readyToAttack && Main.myPlayer == player.whoAmI && Math.Abs(vectorAbove.X) <= 32 && vectorToTargetPosition.Y > 0)
			{
				Projectile.NewProjectile(
					projectile.Center,
					new Vector2(vectorAbove.X / 8, 2),
					ProjectileType<BeeQueenBucket>(),
					projectile.damage,
					projectile.knockBack,
					player.whoAmI,
					ai0: Math.Max(45, 100 - 10 * EmpowerCount));
				reloadStartFrame = animationFrameCounter;
			}
			vectorAbove.SafeNormalize();
			vectorAbove *= 8;
			int inertia = 18;
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorAbove) / inertia;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			DrawWings(spriteBatch, lightColor);
			DrawBody(spriteBatch, lightColor);
			return false;
		}

		private void DrawWings(SpriteBatch spriteBatch, Color lightColor)
		{
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 128);
			float r = projectile.rotation;
			Vector2 pos = projectile.Center + new Vector2(8 * projectile.spriteDirection, 0);
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			int wingFrame = (animationFrameCounter % 12) / 3;
			Texture2D texture = GetTexture(Texture + "_Wings");
			int frameHeight = texture.Height / 4;
			Rectangle bounds = new Rectangle(0, wingFrame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r,
				origin, 1f, effects, 0);
		}
		private void DrawBody(SpriteBatch spriteBatch, Color lightColor)
		{
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 0.75f, effects, 0);
		}

		protected override int ComputeDamage()
		{
			return baseDamage + (int)(baseDamage * EmpowerCount / 4);
		}

		protected override float ComputeSearchDistance()
		{
			return 550f + 20f * EmpowerCount;
		}

		protected override float ComputeInertia()
		{
			return 14;
		}

		protected override float ComputeTargetedSpeed()
		{
			return Math.Min(11.5f, 8f + 0.5f * EmpowerCount);
		}

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			if (readyToAttack)
			{
				minFrame = 0;
				maxFrame = 2;
			}
			else if (Math.Abs(projectile.velocity.Length()) > 4)
			{
				minFrame = 2;
				maxFrame = 4;
			}
			else
			{
				minFrame = 4;
				maxFrame = 6;
			}
		}
	}
}
