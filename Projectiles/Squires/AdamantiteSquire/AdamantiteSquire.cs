using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.AdamantiteSquire
{
	public class AdamantiteSquireMinionBuff : MinionBuff
	{
		public AdamantiteSquireMinionBuff() : base(ProjectileType<AdamantiteSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Adamantite Squire");
			Description.SetDefault("An adamantite squire will follow your orders!");
		}
	}

	public class AdamantiteSquireMinionItem : SquireMinionItem<AdamantiteSquireMinionBuff, AdamantiteSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Adamantite Crest");
			Tooltip.SetDefault("Summons a squire\nAn adamantite squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 5.5f;
			item.width = 24;
			item.height = 38;
			item.damage = 33;
			item.value = Item.buyPrice(0, 2, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.AdamantiteBar, 14);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class AdamantiteSquireMinion : WeaponHoldingSquire
	{
		private Texture2D horseTexture;

		internal override int BuffId => BuffType<AdamantiteSquireMinionBuff>();
		protected override int AttackFrames => 25;

		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";

		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/AdamantiteSquire/AdamantiteSquireSword";

		protected override Vector2 WingOffset => new Vector2(0, 6);

		protected override float knockbackSelf => 5;
		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 6);

		protected override int SpecialDuration => 8 * 60;
		protected override int SpecialCooldown => 12 * 60;

		private MotionBlurDrawer blurHelper;
		private int dashDirection = 1;
		private bool isDashing = false;

		public AdamantiteSquireMinion() : base(ItemType<AdamantiteSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Adamantite Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 30;
			projectile.height = 32;
			horseTexture = GetTexture(Texture + "_Pegasus");
			blurHelper = new MotionBlurDrawer(5);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(base.Colliding(projHitbox, targetHitbox) is bool colliding && colliding)
			{
				return true;
			} 
			if(usingSpecial)
			{
				// proj body also deals damage during special
				projHitbox.Inflate(24, 24);
				return projHitbox.Intersects(targetHitbox);
			}
			return false;
		}

		protected override float WeaponDistanceFromCenter()
		{
			//All of this is based on the weapon sprite and AttackFrames above.
			int reachFrames = AttackFrames / 2; //A spear should spend half the AttackFrames extending, and half retracting by default.
			int spearLength = GetTexture(WeaponTexturePath).Width; //A decent aproximation of how long the spear is.
			int spearStart = (spearLength / 3); //Two thirds of the spear starts behind by default.
			float spearSpeed = spearLength / reachFrames; //A calculation of how quick the spear should be moving.

			// permanently stick spear out to "joust" while using special
			if(usingSpecial)
			{
				return spearLength / 2;
			} else if (attackFrame <= reachFrames)
			{
				return spearSpeed * attackFrame - spearStart;
			}
			else
			{
				return (spearSpeed * reachFrames - spearStart) - spearSpeed * (attackFrame - reachFrames);
			}
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if(usingSpecial)
			{
				damage = 5 * damage / 4;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if(usingSpecial)
			{
				if(isDashing)
				{
					for (int k = 0; k < blurHelper.BlurLength; k++)
					{
						if(!blurHelper.GetBlurPosAndColor(k, lightColor, out Vector2 blurPos, out Color blurColor)) { break; }
						DrawHorse(spriteBatch, blurColor, blurPos);
					}
				}
			} else
			{
				base.PreDraw(spriteBatch, lightColor);
			}
			return true;
		}
		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			isDashing = false;
			base.StandardTargetedMovement(vectorToTargetPosition);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			isDashing = false;
			base.IdleMovement(vectorToIdlePosition);
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			Vector2 target = vectorToTargetPosition;
			isDashing = target.LengthSquared() < 128 * 128;
			if(isDashing)
			{
				for(int i = 0; i < 4; i++)
				{
					Vector2 nextPos = target + dashDirection * 16 * Vector2.UnitX;
					if(Collision.CanHitLine(target, 1, 1, nextPos, 1, 1))
					{
						target = nextPos;
					} else
					{
						break;
					}
				}
				if(target.LengthSquared() < 32 * 32)
				{
					dashDirection *= -1;
				} 
			} 
			base.StandardTargetedMovement(target);
		}

		private void DrawHorse(SpriteBatch spriteBatch, Color lightColor, Vector2 projCenter)
		{
			float r = 0;
			Vector2 offset = new Vector2(4 * projectile.spriteDirection, 12).RotatedBy(r);
			int frameHeight = horseTexture.Height / 4;
			Rectangle bounds = new Rectangle(0, frameHeight * (wingFrame % 4), horseTexture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Vector2 pos = projCenter + offset;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			spriteBatch.Draw(horseTexture, pos - Main.screenPosition,
				bounds, lightColor, r, origin, 1, effects, 0);

		}

		protected override void DrawWeapon(SpriteBatch spriteBatch, Color lightColor)
		{
			// draw the horse after the arms, but before the weapon
			if(usingSpecial)
			{
				DrawHorse(spriteBatch, lightColor, projectile.Center);
			}
			base.DrawWeapon(spriteBatch, lightColor);
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			blurHelper.Update(projectile.Center, usingSpecial);
		}

		protected override int? GetSpriteDirection()
		{
			if(isDashing)
			{
				return dashDirection;
			}
			return base.GetSpriteDirection();
		}

		protected override float GetWeaponAngle()
		{
			if(isDashing)
			{
				return 0;
			}
			return base.GetWeaponAngle();
		}

		protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() + 35;

		protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 45;

		public override float MaxDistanceFromPlayer() => usingSpecial ? 450 : 300;

		public override float ComputeTargetedSpeed() => usingSpecial ? isDashing ? 20 : 14 : 12;

		public override float ComputeIdleSpeed() => usingSpecial ? 14 : 12;
	}
}
