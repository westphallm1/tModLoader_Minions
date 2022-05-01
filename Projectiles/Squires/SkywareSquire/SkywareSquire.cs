using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.SkywareSquire
{
	public class SkywareSquireMinionBuff : MinionBuff
	{
		public SkywareSquireMinionBuff() : base(ProjectileType<SkywareSquireMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Skyware Centurion");
			Description.SetDefault("A Skyware Centurion is fighting for you!");
		}
	}

	public class SkywareSquireMinionItem : SquireMinionItem<SkywareSquireMinionBuff, SkywareSquireMinion>
	{
		protected override string SpecialName => "Barrage";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of the Sky");
			Tooltip.SetDefault("Summons a squire\nA skyware centurion will fight for you\nClick and hold to guide its attacks!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 18;
			Item.value = Item.sellPrice(0, 0, 1, 0);
			Item.rare = ItemRarityID.Blue;
		}
	}


	public abstract class BaseSkywareArrow : ModProjectile
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.width = 16;
			Projectile.height = 16;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// make the arrow a bit bigger to hit things more reliably
			projHitbox.Inflate(32, 0);
			return projHitbox.Intersects(targetHitbox);
		}

		public override void AI()
		{
			base.AI();
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			// start colliding with tiles 1/3 of the way down the screen
			Vector2 position = Projectile.position;
			//TODO not depend on screen
			Vector2 myScreenPosition = Main.player[Projectile.owner].Center 
				- new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
			float collideCutoff = myScreenPosition.Y + Main.screenHeight / 3f;
			if(position.Y >= collideCutoff)
			{
				Tile tile = Framing.GetTileSafely((int)position.X / 16, (int)position.Y / 16);
				if(!tile.HasTile || position.Y > Main.player[Projectile.owner].position.Y)
				{
					Projectile.tileCollide = true;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			float r = Projectile.rotation;
			Rectangle bounds = texture.Bounds;
			Vector2 origin = bounds.Center();
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				bounds, translucentColor, r, origin, 1, 0, 0);
			return false;
		}

		public override void Kill(int timeLeft)
		{
			base.Kill(timeLeft);
			for (float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 6)
			{
				Vector2 velocity = 2f * new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
				Dust dust = Dust.NewDustPerfect(Projectile.Center, 135, velocity, 0, Color.White, 1.5f);
				dust.noGravity = true;
				dust.velocity *= Main.rand.NextFloat(0.8f, 1.2f);
				dust.fadeIn = 1f;
				dust.noLight = true;
			}
		}
	}

	public class SkywareArrow : BaseSkywareArrow
	{

		public override void AI()
		{
			base.AI();
			if(Projectile.timeLeft % 2 == 0)
			{
				int dustId = Dust.NewDust(Projectile.Center, 8, 8, 135, 0f, 0f, 0, Color.White, 1.25f);
				Main.dust[dustId].noGravity = true;
				// Main.dust[dustId].fadeIn = 2f;
				Main.dust[dustId].noLight = true;
			}
		}

	}

	public class SkywareLargeArrow : BaseSkywareArrow
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.penetrate = 3;
			Projectile.usesLocalNPCImmunity = true;
		}

		public override void AI()
		{
			base.AI();
			float myScreenBottom = Main.player[Projectile.owner].Center.Y + Main.screenHeight / 2;
			Projectile.tileCollide = Projectile.Center.Y > myScreenBottom;
			if (Projectile.timeLeft % 2 == 0)
			{
				int dustId = Dust.NewDust(Projectile.Center, 8, 8, 135, 0f, 0f, 0, Color.White, 2f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].fadeIn = 2f;
				Main.dust[dustId].noLight = true;
			}
			Lighting.AddLight(Projectile.Center, Color.SkyBlue.ToVector3() * 0.5f);
		}
	}

	public class SkywareSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<SkywareSquireMinionBuff>();
		protected override int ItemType => ItemType<SkywareSquireMinionItem>();
		protected override int AttackFrames => 35;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";
		protected override string WeaponTexturePath => base.Texture + "_Bow";

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override LegacySoundStyle attackSound => new LegacySoundStyle(2, 5);

		protected override float projectileVelocity => 16;

		protected override int SpecialDuration => 45;
		protected override bool travelRangeCanBeModified => false;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Skyware Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 22;
			Projectile.height = 32;
			attackThroughWalls = true;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}


		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			// position squire about halfway between the player and the mouse,
			// near the top of the screen
			Vector2 mousePos = syncedMouseWorld;
			float targetX = (mousePos.X + player.position.X) / 2;
			// should factor into an extension method
			Vector2 myScreenPosition = player.Center - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
			float targetY = myScreenPosition.Y + Main.screenHeight * 0.05f;
			Vector2 targetPos = new Vector2(targetX, targetY);
			vectorToTargetPosition = targetPos - Projectile.Center;
			base.StandardTargetedMovement(vectorToTargetPosition);
			Lighting.AddLight(Projectile.Center, Color.SkyBlue.ToVector3() * 0.25f);
			if (!usingSpecial && attackFrame == 0 && Math.Abs(Projectile.Center.Y - targetY) < 64f)
			{
				Vector2 angleVector = UnitVectorFromWeaponAngle();
				angleVector *= ModifiedProjectileVelocity();
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(), 
						Projectile.Center,
						angleVector,
						ProjectileType<SkywareArrow>(),
						3 * Projectile.damage / 2,
						Projectile.knockBack,
						Main.myPlayer);
				}
			}
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			StandardTargetedMovement(vectorToTargetPosition);
			if(specialFrame % 12 == 1 && player.whoAmI == Main.myPlayer)
			{
				Vector2 angleVector = UnitVectorFromWeaponAngle();
				angleVector *= 1.5f * ModifiedProjectileVelocity();
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(), 
						Projectile.Center,
						angleVector,
						ProjectileType<SkywareLargeArrow>(),
						2 * Projectile.damage,
						Projectile.knockBack,
						Main.myPlayer);
				}
				SoundEngine.PlaySound(attackSound, Projectile.Center);
			}
		}


		protected override float WeaponDistanceFromCenter() => 6;

		public override float ComputeIdleSpeed() => 8;

		// go faster while ascending
		public override float ComputeTargetedSpeed() => 
			Math.Abs(player.Center.Y - Main.screenHeight/2 - Projectile.Center.Y) < 64 ? 8 : 24;

		public override float MaxDistanceFromPlayer() => 2000;
	}
}
