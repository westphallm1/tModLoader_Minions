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
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
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
			Item.damage = 20;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.buyPrice(0, 0, 2, 0);
			Item.rare = ItemRarityID.Green;
		}
	}

	/// <summary>
	/// uses ai[0] for the spawn frequency
	/// </summary>
	public class BeeQueenBucket : TransientMinion
	{

		private Vector2 rememberedEnemyAngle = new Vector2(0, -6);

		private int spawnFrequency => (int)Projectile.ai[0];
		private int timeToLive = 360;
		private bool inSummoningMode = false;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 22;
			Projectile.height = 26;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
		}

		private void EnterSummoningMode()
		{
			if (inSummoningMode)
			{
				return;
			}
			inSummoningMode = true;
			Projectile.netUpdate = true; //TODO investigate if this is enough
			Projectile.friendly = false;
			Projectile.timeLeft = timeToLive - (int)Projectile.ai[0] - 1;
			Projectile.velocity = Vector2.Zero;
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.Top, 16, 16, 153);
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

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = false;
			return true;
		}

		public override Vector2? FindTarget()
		{
			if (SelectedEnemyInRange(300f, 0, maxRangeFromPlayer: false) is Vector2 target)
			{
				rememberedEnemyAngle = target;
				rememberedEnemyAngle.Normalize();
				rememberedEnemyAngle *= 7;
			};
			return null;
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (Projectile.timeLeft % spawnFrequency == 0 && Projectile.timeLeft > 60)
			{

				for (int i = 0; i < 3; i++)
				{
					Dust.NewDust(Projectile.Top, 16, 16, 153);
				}
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						Projectile.GetProjectileSource_FromThis(),
						Projectile.Center,
						rememberedEnemyAngle,
						ProjectileType<HoneySlime>(),
						Projectile.damage,
						Projectile.knockBack,
						player.whoAmI);
				}
			}
			if (Projectile.timeLeft == 60)
			{
				for (int i = 0; i < 6; i++)
				{
					Dust.NewDust(Projectile.Top, 16, 16, 153);
				}
			}
			Projectile.velocity.Y += 0.5f;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			Projectile.frame = Projectile.timeLeft > 60 ? 0 : 1;
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.position, 1, 1, DustType<BucketDust>(), Main.rand.Next(6) - 3, -3);
			}
		}
	}
	public class BeeQueenCounterMinion : CounterMinion
	{

		internal override int BuffId => BuffType<BeeQueenMinionBuff>();
		protected override int MinionType => ProjectileType<BeeQueenMinion>();
	}

	public class BeeQueenMinion : EmpoweredMinion
	{
		internal override int BuffId => BuffType<BeeQueenMinionBuff>();
		int animationFrameCounter = 0;
		int reloadCycleLength => Math.Max(120, 300 - 20 * EmpowerCount);

		int reloadStartFrame = 0;
		protected override int dustType => 153;

		public override int CounterType => ProjectileType<BeeQueenCounterMinion>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bee Bombardier");
			Main.projFrames[Projectile.type] = 6;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}

		public override void LoadAssets()
		{
			AddTexture(Texture + "_Wings");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 32;
			frameSpeed = 15;
			animationFrameCounter = 0;
			reloadStartFrame = -reloadCycleLength;
			dealsContactDamage = false;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if (Projectile.velocity.X >= 2)
			{
				Projectile.spriteDirection = -1;
			}
			else if (Projectile.velocity.X <= -2)
			{
				Projectile.spriteDirection = 1;
			}
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			float idleAngle = (float)(2 * Math.PI * animationFrameCounter % 240) / 240;
			Vector2 idlePosition = player.Center;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, Projectile);
			idlePosition.Y += -35 + 5 * (float)Math.Sin(idleAngle);
			if (!Collision.CanHit(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition = player.Center;
				idlePosition.X += 30 * -player.direction;
				idlePosition.Y += -35;
			}
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
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
			// only check for exact position once close to target
			if (vectorToTargetPosition.LengthSquared() < 256 * 256)
			{
				for (int i = 16; i < targetAbove; i++)
				{
					vectorAbove = new Vector2(vectorToTargetPosition.X, vectorToTargetPosition.Y - i);
					if (!Collision.CanHit(Projectile.Center, 1, 1, Projectile.Center + vectorAbove, 1, 1))
					{
						break;
					}
				}
			}
			if (readyToAttack && Main.myPlayer == player.whoAmI && Math.Abs(vectorAbove.X) <= 32 && vectorToTargetPosition.Y > 0)
			{
				Projectile.NewProjectile(
					Projectile.GetProjectileSource_FromThis(),
					Projectile.Center,
					VaryLaunchVelocity(new Vector2(vectorAbove.X / 8, 2)),
					ProjectileType<BeeQueenBucket>(),
					Projectile.damage,
					Projectile.knockBack,
					player.whoAmI,
					ai0: Math.Max(45, 100 - 10 * EmpowerCount));
				reloadStartFrame = animationFrameCounter;
			}
			vectorAbove.SafeNormalize();
			vectorAbove *= 8;
			int inertia = 18;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorAbove) / inertia;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			DrawWings(lightColor);
			DrawBody(lightColor);
			return false;
		}

		private void DrawWings(Color lightColor)
		{
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 128);
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center + new Vector2(8 * Projectile.spriteDirection, 0);
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			int wingFrame = (animationFrameCounter % 12) / 3;
			Texture2D texture = ExtraTextures[0].Value;
			int frameHeight = texture.Height / 4;
			Rectangle bounds = new Rectangle(0, wingFrame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r,
				origin, 1f, effects, 0);
		}
		private void DrawBody(Color lightColor)
		{
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
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
			else if (Math.Abs(Projectile.velocity.Length()) > 4)
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
