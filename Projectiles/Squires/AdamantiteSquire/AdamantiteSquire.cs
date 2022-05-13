using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.AdamantiteSquire
{
	public class AdamantiteSquireMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<AdamantiteSquireMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Adamantite Squire");
			Description.SetDefault("An adamantite squire will follow your orders!");
		}
	}

	public class AdamantiteSquireMinionItem : SquireMinionItem<AdamantiteSquireMinionBuff, AdamantiteSquireMinion>
	{

		protected override string SpecialName => "Pegasus Knight";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Adamantite Crest");
			Tooltip.SetDefault("Summons a squire\nAn adamantite squire will fight for you!\nClick and hold to guide its attacks");
			CrossMod.SummonersShineMinionPowerCollection minionCollection = new CrossMod.SummonersShineMinionPowerCollection();
			minionCollection.AddMinionPower(5f/4);
			CrossMod.BakeSummonersShineMinionPower_NoHooks(Item.type, minionCollection);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 5.5f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 33;
			Item.value = Item.buyPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.AdamantiteBar, 14).AddTile(TileID.MythrilAnvil).Register();
		}
	}

	public class AdamantiteSquireMinion : WeaponHoldingSquire
	{

		internal override int BuffId => BuffType<AdamantiteSquireMinionBuff>();
		protected override int ItemType => ItemType<AdamantiteSquireMinionItem>();
		protected override int AttackFrames => 25;

		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";

		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/AdamantiteSquire/AdamantiteSquireSword";

		protected override Vector2 WingOffset => new Vector2(0, 6);

		protected override float knockbackSelf => 5;
		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 6);

		protected override int SpecialDuration => 4 * 60;
		protected override int SpecialCooldown => 10 * 60;

		protected override LegacySoundStyle SpecialStartSound => new LegacySoundStyle(3, 12);

		private MotionBlurDrawer blurHelper;
		private int dashDirection = 1;
		private bool isDashing = false;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Adamantite Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
			CrossMod.SetSummonersShineProjMaxEnergy(Projectile.type, 0);
		}
		public override void LoadAssets()
		{
			base.LoadAssets();
			AddTexture(Texture + "_Pegasus");
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 30;
			Projectile.height = 32;
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
			int spearLength = ExtraTextures[1].Width(); //A decent aproximation of how long the spear is.
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
				damage = (int)(damage * CrossMod.ReplaceValueWithSummonersShineMinionPower(5f / 4, Projectile, 0));
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if(usingSpecial)
			{
				if(isDashing)
				{
					for (int k = 0; k < blurHelper.BlurLength; k++)
					{
						if(!blurHelper.GetBlurPosAndColor(k, lightColor, out Vector2 blurPos, out Color blurColor)) { break; }
						DrawHorse(blurColor, blurPos);
					}
				}
			} else
			{
				base.PreDraw(ref lightColor);
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

		private void DrawHorse(Color lightColor, Vector2 projCenter)
		{
			float r = 0;
			Vector2 offset = new Vector2(4 * Projectile.spriteDirection, 12).RotatedBy(r);
			Texture2D horseTexture = ExtraTextures[2].Value;
			int frameHeight = horseTexture.Height / 4;
			Rectangle bounds = new Rectangle(0, frameHeight * (wingFrame % 4), horseTexture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Vector2 pos = projCenter + offset;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(horseTexture, pos - Main.screenPosition,
				bounds, lightColor, r, origin, 1, effects, 0);

		}

		protected override void DrawWeapon(Color lightColor)
		{
			// draw the horse after the arms, but before the weapon
			if(usingSpecial)
			{
				DrawHorse(lightColor, Projectile.Center);
			}
			base.DrawWeapon(lightColor);
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			blurHelper.Update(Projectile.Center, usingSpecial);
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
