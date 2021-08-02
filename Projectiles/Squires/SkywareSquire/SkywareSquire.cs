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
		public override void SetDefaults()
		{
			base.SetDefaults();
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
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
			item.damage = 18;
			item.value = Item.sellPrice(0, 0, 1, 0);
			item.rare = ItemRarityID.Blue;
		}
	}


	public abstract class BaseSkywareArrow : ModProjectile
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.tileCollide = false;
			projectile.friendly = true;
			projectile.width = 16;
			projectile.height = 16;
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
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
			// start colliding with tiles 1/3 of the way down the screen
			Vector2 position = projectile.position;
			Vector2 myScreenPosition = Main.player[projectile.owner].Center 
				- new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
			float collideCutoff = myScreenPosition.Y + Main.screenHeight / 3f;
			if(position.Y >= collideCutoff)
			{
				Tile tile = Framing.GetTileSafely((int)position.X / 16, (int)position.Y / 16);
				if(!tile.active() || position.Y > Main.player[projectile.owner].position.Y)
				{
					projectile.tileCollide = true;
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
			Texture2D texture = Main.projectileTexture[projectile.type];
			float r = projectile.rotation;
			Rectangle bounds = texture.Bounds;
			Vector2 origin = bounds.Center();
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
				bounds, translucentColor, r, origin, 1, 0, 0);
			return false;
		}

		public override void Kill(int timeLeft)
		{
			base.Kill(timeLeft);
			for (float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 6)
			{
				Vector2 velocity = 2f * new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
				Dust dust = Dust.NewDustPerfect(projectile.Center, 135, velocity, 0, Color.White, 1.5f);
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
			if(projectile.timeLeft % 2 == 0)
			{
				int dustId = Dust.NewDust(projectile.Center, 8, 8, 135, 0f, 0f, 0, Color.White, 1.25f);
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
			projectile.penetrate = 3;
			projectile.usesLocalNPCImmunity = true;
		}

		public override void AI()
		{
			base.AI();
			float myScreenBottom = Main.player[projectile.owner].Center.Y + Main.screenHeight / 2;
			projectile.tileCollide = projectile.Center.Y > myScreenBottom;
			if (projectile.timeLeft % 2 == 0)
			{
				int dustId = Dust.NewDust(projectile.Center, 8, 8, 135, 0f, 0f, 0, Color.White, 2f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].fadeIn = 2f;
				Main.dust[dustId].noLight = true;
			}
			Lighting.AddLight(projectile.Center, Color.SkyBlue.ToVector3() * 0.5f);
		}
	}

	public class SkywareSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<SkywareSquireMinionBuff>();
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


		public SkywareSquireMinion() : base(ItemType<SkywareSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Skyware Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 22;
			projectile.height = 32;
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
			vectorToTargetPosition = targetPos - projectile.Center;
			base.StandardTargetedMovement(vectorToTargetPosition);
			Lighting.AddLight(projectile.Center, Color.SkyBlue.ToVector3() * 0.25f);
			if (!usingSpecial && attackFrame == 0 && Math.Abs(projectile.Center.Y - targetY) < 64f)
			{
				Vector2 angleVector = UnitVectorFromWeaponAngle();
				angleVector *= ModifiedProjectileVelocity();
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(projectile.Center,
						angleVector,
						ProjectileType<SkywareArrow>(),
						3 * projectile.damage / 2,
						projectile.knockBack,
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
					Projectile.NewProjectile(projectile.Center,
						angleVector,
						ProjectileType<SkywareLargeArrow>(),
						2 * projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}
				Main.PlaySound(attackSound, projectile.Center);
			}
		}


		protected override float WeaponDistanceFromCenter() => 6;

		public override float ComputeIdleSpeed() => 8;

		// go faster while ascending
		public override float ComputeTargetedSpeed() => 
			Math.Abs(player.Center.Y - Main.screenHeight/2 - projectile.Center.Y) < 64 ? 8 : 24;

		public override float MaxDistanceFromPlayer() => 2000;
	}
}
