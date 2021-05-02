using AmuletOfManyMinions.Items.Materials;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.GuideSquire;
using AmuletOfManyMinions.Projectiles.Squires.Squeyere;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.WoFSquire
{
	public class GuideVoodooSquireMinionBuff : MinionBuff
	{
		public GuideVoodooSquireMinionBuff() : base(ProjectileType<GuideVoodooSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Guide Squire");
			Description.SetDefault("You can guide the Guide!");
		}
	}
	public class WoFSquireMinionBuff : MinionBuff
	{
		public WoFSquireMinionBuff() : base(ProjectileType<WoFSquireMinion>()) { }
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Wall of Flesh Squire");
			Description.SetDefault("You can guide the Wall of Flesh!");
		}
		public override void Update(Player player, ref int buffIndex)
		{
			// don't keep self active just because minion is alive
			if (player.ownedProjectileCounts[ProjectileType<WoFSquireMinion>()] == 0)
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}

	public class GuideVoodooSquireMinionItem : SquireMinionItem<GuideVoodooSquireMinionBuff, GuideVoodooSquireMinion>
	{
		private int wofType => ProjectileType<WoFSquireMinion>();
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("True Guide Voodoo Doll");
			Tooltip.SetDefault("Summons a squire\nClick and hold to guide its attacks!\n'You are a *REALLY* terrible person'");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 5f;
			item.width = 24;
			item.height = 38;
			item.damage = 130;
			item.value = Item.sellPrice(0, 0, 1, 0);
			item.rare = ItemRarityID.Red;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			if (player.ownedProjectileCounts[wofType] > 0)
			{
				return false;
			}
			return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
		}
		public override bool CanUseItem(Player player)
		{
			base.CanUseItem(player);
			if (player.ownedProjectileCounts[item.shoot] > 0 || player.ownedProjectileCounts[wofType] > 0)
			{
				item.UseSound = null;
				item.noUseGraphic = true;
				item.useStyle = ItemUseStyleID.HoldingOut;
			}
			else
			{
				item.useStyle = ItemUseStyleID.HoldingUp;
				item.noUseGraphic = false;
				item.UseSound = SoundID.Item44;
			}
			return true;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.GuideVoodooDoll, 1);
			recipe.AddIngredient(ItemType<GuideSquireMinionItem>(), 1);
			recipe.AddIngredient(ItemType<GuideHair>(), 1);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class WoFEyeLaser : SquireLaser
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.PurpleLaser;

		public override Color lightColor => Color.Purple;
	}

	public class WoFSquireMinion : SquireMinion
	{
		internal override int BuffId => BuffType<WoFSquireMinionBuff>();
		int dashDirection = 1;
		bool isDashing;

		private Vector2?[] laserTargets;
		private int[] laserFrames;

		public WoFSquireMinion() : base(ItemType<GuideVoodooSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Squire of Flesh");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 4;
		}
		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 32;
			frameSpeed = 7;
			laserTargets = new Vector2?[2];
			laserFrames = new int[2];
			isDashing = false;
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 0.75f);
			return base.IdleBehavior() + new Vector2(-player.direction * 8, 0);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			projectile.friendly = false;
			isDashing = false;
			base.IdleMovement(vectorToIdlePosition);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int maxDashDistance = 80;
			float dashCheckDistance = 1.25f * maxDashDistance;
			float searchDistance = 2 * dashCheckDistance;
			int collisionStep = 8;
			projectile.friendly = true;
			isDashing = false;
			Vector2? _nearestNPCVector = AnyEnemyInRange(searchDistance, projectile.Center);
			if (_nearestNPCVector is Vector2 nearestNPCVector)
			{
				vectorToTargetPosition = nearestNPCVector - projectile.Center;
			}
			if (_nearestNPCVector != null)
			{
				if (vectorToTargetPosition.Length() < dashCheckDistance)
				{
					isDashing = true;
					for (int i = 0; i < maxDashDistance; i += collisionStep)
					{
						vectorToTargetPosition.X += collisionStep * dashDirection;
						if (!Collision.CanHit(projectile.position, projectile.width, projectile.height, projectile.position + vectorToTargetPosition, 1, 1))
						{
							break;
						}
					}
					if (vectorToTargetPosition.Length() < 16f)
					{
						dashDirection *= -1;
					}
				}
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (isDashing)
			{
				DrawAferImage(spriteBatch);
			}
			if (vectorToTarget != null)
			{
				DrawClingers(spriteBatch, lightColor);
			}
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
			DrawEyes(spriteBatch, lightColor);
			return false;
		}

		private Vector2 EyePos(int idx)
		{
			Vector2 pos = projectile.Center;
			pos.X += 8 * projectile.spriteDirection;
			pos.Y += idx == 0 ? 13 : -15;
			return pos;
		}

		private void DrawEyes(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture + "_Eye");
			Rectangle bounds = texture.Bounds;
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			for (int i = 0; i < 2; i++)
			{
				float r;
				if (laserTargets[i] is Vector2 target && animationFrame - laserFrames[i] < 10)
				{
					r = target.ToRotation();
				}
				else if (projectile.spriteDirection == 1)
				{
					r = MathHelper.Pi;
				}
				else
				{
					r = 0;
				}
				spriteBatch.Draw(texture, EyePos(i) - Main.screenPosition,
					bounds, lightColor, r,
					origin, 1, 0, 0);
			}
		}

		private void DrawAferImage(SpriteBatch spriteBatch)
		{
			int nToDraw = 4;
			float velocityFraction = 0.75f;
			Vector2 velocity = -projectile.velocity;
			if (Math.Abs(velocity.Y) > 4)
			{
				velocity.Y = 4 * Math.Sign(velocity.Y);
			}
			float r = projectile.rotation;
			SpriteEffects effects = projectile.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			for (int i = 0; i < nToDraw; i++)
			{
				Vector2 pos = projectile.Center + i * velocity * velocityFraction;
				Color lightColor = Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16);
				lightColor.A = (byte)128;
				spriteBatch.Draw(texture, pos - Main.screenPosition,
					bounds, lightColor, r,
					origin, 1, effects, 0);
			}
		}
		private void DrawClingers(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture + "_Clingers");
			int nFrames = 2;
			int frameHeight = texture.Height / nFrames;
			float r = projectile.spriteDirection == 1 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
			for (int i = 0; i < 3; i++)
			{
				int frame = animationFrame % 60 < 30 ? 0 : 1;
				if (i == 1)
				{
					frame = frame == 1 ? 0 : 1; // flip the frame for the middle clinger
				}
				Rectangle bounds = new Rectangle(0, frame * frameHeight, texture.Width, frameHeight);
				Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
				Vector2 pos = projectile.Center;
				float yOffset = 15 + 4 * (float)Math.Sin(2 * Math.PI * animationFrame / 60f);
				if (i == 0)
				{
					pos.Y -= yOffset;
				}
				else if (i == 2)
				{
					pos.Y += yOffset;
				}
				pos.X += (float)(projectile.spriteDirection * 20 + 4 * Math.Sin(2 * Math.PI * (animationFrame / 60f + i / 3f)));
				spriteBatch.Draw(texture, pos - Main.screenPosition,
					bounds, lightColor, r,
					origin, 1, 0, 0);
			}

		}

		public override void Kill(int timeLeft)
		{
			Vector2 goreVelocity = projectile.velocity;
			goreVelocity.Normalize();
			goreVelocity *= 4f;
			int gore1 = Gore.NewGore(projectile.position, goreVelocity, mod.GetGoreSlot("Gores/WoFEyeGore"), 1f);
			int gore2 = Gore.NewGore(projectile.position, goreVelocity, mod.GetGoreSlot("Gores/WoFEyeGore"), 1f);
			int gore3 = Gore.NewGore(projectile.position, goreVelocity, mod.GetGoreSlot("Gores/WoFHammerGore"), 1f);
			foreach (int gore in new int[] { gore1, gore2, gore3 })
			{
				Main.gore[gore].timeLeft = 180; // make it last not as long
				Main.gore[gore].alpha = 128; // make it transparent
			}
			for (int i = 0; i < 8; i++)
			{
				Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Blood, projectile.velocity.X, projectile.velocity.Y);
			}
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (vectorToIdle.Length() < 32)
			{
				projectile.spriteDirection = player.direction;
			}
			else if (vectorToTarget is Vector2 target)
			{
				if (target.Length() < 64f && !isDashing)
				{
					projectile.spriteDirection = player.Center.X - projectile.Center.X > 0 ? -1 : 1;
				}
				else if (relativeVelocity.X > 2)
				{
					projectile.spriteDirection = 1;
				}
				else if (relativeVelocity.X < -2)
				{
					projectile.spriteDirection = -1;
				}
			}
			base.Animate(minFrame, maxFrame);
		}


		public override float ComputeIdleSpeed() => 18;

		// needs to slow down a little so dash is visible
		public override float ComputeTargetedSpeed() => isDashing ? 16 / player.GetModPlayer<SquireModPlayer>().squireAttackSpeedMultiplier : 18;

		public override float ComputeInertia() => isDashing ? 4 : base.ComputeInertia();

		public override float MaxDistanceFromPlayer() => 700f;
	}

	public class GuideVoodooSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<GuideVoodooSquireMinionBuff>();
		protected override int AttackFrames => 40;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";
		protected override string WeaponTexturePath => null;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override bool attackSpeedCanBeModified => false;

		protected int mockHealth;

		protected int baseDamage;
		protected float baseKnockback;

		protected static int MockMaxHealth = 4;

		protected int knockbackCounter = 0;

		protected override LegacySoundStyle attackSound => null;


		public GuideVoodooSquireMinion() : base(ItemType<GuideVoodooSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Guide Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 22;
			projectile.height = 32;
			mockHealth = MockMaxHealth;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 12;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			baseDamage = projectile.damage;
			baseKnockback = projectile.knockBack;
			projectile.damage = 1;
			projectile.knockBack = 0;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (knockbackCounter < 0)
			{
				Vector2 kbDirection = target.Center - projectile.Center;
				kbDirection.Normalize();
				kbDirection *= -3.5f;
				projectile.velocity = target.velocity + kbDirection;
				if (target.CanBeChasedBy())
				{
					mockHealth = Math.Max(0, mockHealth - 1);
				}
				knockbackCounter = 12;
			}
			if (mockHealth == 0)
			{
				projectile.Kill();
			}
		}

		private byte getGradient(byte b1, byte b2, float weight)
		{
			return (byte)(b2 + (b1 - b2) * (weight));
		}
		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (mockHealth != MockMaxHealth)
			{
				float widthFraction = new Dictionary<int, float>
				{
					[0] = 0.05f,
					[1] = 0.1f,
					[2] = 0.5f,
					[3] = 0.9f
				}[mockHealth];

				Color maxHealthColor = new Color(77, 230, 0);
				Color halfHealthColor = new Color(230, 230, 0);
				Color zeroHealthColor = new Color(230, 37, 0);
				byte r, g, b;
				if (widthFraction > 0.5f)
				{
					float weight = 2 * (widthFraction - 0.5f);
					r = getGradient(maxHealthColor.R, halfHealthColor.R, weight);
					g = getGradient(maxHealthColor.G, halfHealthColor.G, weight);
					b = getGradient(maxHealthColor.B, halfHealthColor.B, weight);
				}
				else
				{
					float weight = 2 * (widthFraction);
					r = getGradient(halfHealthColor.R, zeroHealthColor.R, weight);
					g = getGradient(halfHealthColor.G, zeroHealthColor.G, weight);
					b = getGradient(halfHealthColor.B, zeroHealthColor.B, weight);
				}
				Color drawColor = new Color(r, g, b);
				Texture2D healthBar = GetTexture("Terraria/HealthBar1");
				Texture2D healthBarBack = GetTexture("Terraria/HealthBar2");
				Rectangle bounds = new Rectangle(0, 0, (int)(healthBar.Width * widthFraction), healthBar.Height);
				Vector2 origin = healthBar.Bounds.Center.ToVector2();
				Vector2 pos = projectile.Bottom + new Vector2(0, 8);
				spriteBatch.Draw(healthBarBack, pos - Main.screenPosition,
					healthBarBack.Bounds, drawColor, 0,
					healthBarBack.Bounds.Center.ToVector2(), 1, 0, 0);
				spriteBatch.Draw(healthBar, pos - Main.screenPosition,
					bounds, drawColor, 0,
					origin, 1, 0, 0);
			}
			base.PostDraw(spriteBatch, lightColor);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (knockbackCounter-- < 0)
			{
				base.TargetedMovement(vectorToTargetPosition);
			}
			else
			{
				projectile.velocity *= 0.99f;
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (knockbackCounter-- < 0)
			{
				base.IdleMovement(vectorToIdlePosition);
			}
			else
			{
				projectile.velocity *= 0.99f;
			}
		}

		public override void Kill(int timeLeft)
		{
			if (mockHealth == 0)
			{
				Vector2 goreVelocity = projectile.velocity;
				goreVelocity.Normalize();
				goreVelocity *= 4f;
				int gore1 = Gore.NewGore(projectile.position, goreVelocity, mod.GetGoreSlot("Gores/GuideGore"), 1f);
				int gore2 = Gore.NewGore(projectile.position, goreVelocity, mod.GetGoreSlot("Gores/GuideBodyGore"), 1f);
				int gore3 = Gore.NewGore(projectile.position, goreVelocity, mod.GetGoreSlot("Gores/GuideLegsGore"), 1f);
				foreach (int gore in new int[] { gore1, gore2, gore3 })
				{
					Main.gore[gore].timeLeft = 180; // make it last not as long
					Main.gore[gore].alpha = 128; // make it transparent
				}
				for (int i = 0; i < 6; i++)
				{
					Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Blood, projectile.velocity.X, projectile.velocity.Y);
				}

				if (player.whoAmI == Main.myPlayer)
				{
					player.AddBuff(BuffType<WoFSquireMinionBuff>(), 60 * 20); // evolved form lasts 3 minutes
					Projectile.NewProjectile(projectile.Center, projectile.velocity, ProjectileType<WoFSquireMinion>(), baseDamage, baseKnockback, player.whoAmI);
				}
			}
		}
		protected override float WeaponDistanceFromCenter() => 6;

		public override float ComputeIdleSpeed() => 15;

		public override float ComputeTargetedSpeed() => 15;

		public override float MaxDistanceFromPlayer() => 600f;
	}
}
