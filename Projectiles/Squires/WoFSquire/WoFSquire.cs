using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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

	public class GuideVoodooSquireMinionItem : SquireMinionItem<GuideVoodooSquireMinionBuff, GuideVoodooSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Guide Crest");
			Tooltip.SetDefault("Summons a squire\nGuide the guide (or an unholy manifestation assuming his image)!\nClick and hold to guide its attacks!\n'You are a *REALLY* terrible person'");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
			item.damage = 13;
			item.value = Item.sellPrice(0, 0, 1, 0);
			item.rare = ItemRarityID.Orange;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			if(player.ownedProjectileCounts[ProjectileType<WoFSquireMinion>()] > 0) {
				return false;
			}
			return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
		}
	}

	public class WoFClingersMinion : TransientMinion
	{
		public override string Texture => "Terraria/Item_0";
		int animationFrame = 0;
		public override Vector2 IdleBehavior()
		{
			Projectile wof = GetMinionsOfType(ProjectileType<WoFSquireMinion>()).FirstOrDefault();
			if(wof == default)
			{
				return projectile.velocity;
			}
			projectile.timeLeft = 2;
			animationFrame++;
			return base.IdleBehavior();
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			throw new NotImplementedException();
		}

		public override Vector2? FindTarget()
		{
			return base.FindTarget();
		}
	}
	public class WoFSquireMinion : SquireMinion<GuideVoodooSquireMinionBuff>
	{
		public override string Texture => "Terraria/Item_0";
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
			projectile.width = 16;
			projectile.height = 16;
			frameSpeed = 15;
			projectile.friendly = false;
		}
	}

	public class GuideVoodooSquireMinion : WeaponHoldingSquire<GuideVoodooSquireMinionBuff>
	{
		protected override int AttackFrames => 40;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";
		protected override string WeaponTexturePath => null;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override bool attackSpeedCanBeModified => false;

		protected float projectileVelocity = 12;

		protected int mockHealth;

		protected int baseDamage;
		protected float baseKnockback;

		protected static int MockMaxHealth = 100;

		protected int knockbackCounter = 0;


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
			if(knockbackCounter < 0)
			{
				Vector2 kbDirection = target.Center - projectile.Center;
				kbDirection.Normalize();
				kbDirection *= -3.5f;
				projectile.velocity =  target.velocity + kbDirection;
				mockHealth = Math.Max(0, mockHealth - target.damage);
				knockbackCounter = 12;
			}
			if(mockHealth == 0)
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
			if(mockHealth != MockMaxHealth)
			{
				float widthFraction = mockHealth / (float)MockMaxHealth;
				Color maxHealthColor = new Color(77, 230, 0);
				Color halfHealthColor = new Color(230, 230, 0);
				Color zeroHealthColor = new Color(230, 37, 0);
				byte r, g, b;
				if(widthFraction > 0.5f)
				{
					float weight = 2 * (widthFraction - 0.5f);
					r = getGradient(maxHealthColor.R, halfHealthColor.R, weight);
					g = getGradient(maxHealthColor.G, halfHealthColor.G, weight);
					b = getGradient(maxHealthColor.B, halfHealthColor.B, weight);
				} else
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
			if(knockbackCounter-- < 0)
			{
				base.TargetedMovement(vectorToTargetPosition);
			} else
			{
				projectile.velocity *= 0.99f;
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (knockbackCounter-- < 0)
			{
				base.IdleMovement(vectorToIdlePosition);
			} else
			{
				projectile.velocity *= 0.99f;
			}
		}

		public override void Kill(int timeLeft)
		{
			if(mockHealth == 0)
			{
				Vector2 goreVelocity = projectile.velocity;
				goreVelocity.Normalize();
				goreVelocity *= 4f;
				Gore.NewGore(projectile.position, goreVelocity, mod.GetGoreSlot("Gores/GuideGore"), 1f);
				Gore.NewGore(projectile.position, goreVelocity, mod.GetGoreSlot("Gores/GuideBodyGore"), 1f);
				Gore.NewGore(projectile.position, goreVelocity, mod.GetGoreSlot("Gores/GuideLegsGore"), 1f);
				for(int i = 0; i < 6; i++)
				{
					Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Blood, projectile.velocity.X, projectile.velocity.Y);
				}
			}
		}
		protected override float WeaponDistanceFromCenter() => 6;

		public override float ComputeIdleSpeed() => 15;

		public override float ComputeTargetedSpeed() => 15;

		public override float MaxDistanceFromPlayer() => 600f;
	}
}
