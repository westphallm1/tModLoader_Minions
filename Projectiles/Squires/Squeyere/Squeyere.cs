using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.Squeyere
{
	public class SqueyereMinionBuff : MinionBuff
	{
		public SqueyereMinionBuff() : base(ProjectileType<SqueyereMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Squeyere");
			Description.SetDefault("A Squeyere will follow your orders!");
		}
	}

	public class SqueyereMinionItem : SquireMinionItem<SqueyereMinionBuff, SqueyereMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Eyes");
			Tooltip.SetDefault("Summons a squire\nA Squeyere will fight for you!\nClick and hold to guide its attacks\n" +
				"'Sq-Eye-Re. Get it?'");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
			item.damage = 47;
			item.value = Item.sellPrice(0, 4, 0, 0);
			item.rare = ItemRarityID.Pink;
		}
	}

	public abstract class SquireLaser: ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.friendly = true;
			projectile.penetrate = 1;
			projectile.timeLeft = 60;
		}

		public virtual Color lightColor => Color.Green;

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// manually draw at 2x scale with transparency
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 128);
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.velocity.X < 0 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r,
				origin, 1.5f, effects, 0);
			return false;
		}
		public override void AI()
		{
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2; 
			Lighting.AddLight(projectile.position, this.lightColor.ToVector3());
			base.AI();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, 16, 16);
			return true;
		}

	}

	public class SqueyereLaser: SquireLaser
	{
		public override string Texture => "Terraria/Projectile_"+ProjectileID.GreenLaser;
	}

	public class SqueyereMinion : WeaponHoldingSquire<SqueyereMinionBuff>
	{
		protected override int AttackFrames => 30;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/DemonWings";
		protected override string WeaponTexturePath => null;

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override Vector2 WingOffset => new Vector2(-4, 0);

		//unfortunately just flipping the the direction doesn't look great for this one
		protected override Vector2 WeaponCenterOfRotation => projectile.spriteDirection == 1 ? new Vector2(4, -6) : new Vector2(8, -6);

		protected float projectileVelocity = 30;

		public SqueyereMinion() : base(ItemType<SqueyereMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ancient Cobalt Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 20;
			projectile.height = 32;
		}


		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				if (Main.myPlayer == player.whoAmI)
				{
					Vector2 angleVector = UnitVectorFromWeaponAngle();
					angleVector *= projectileVelocity;
					Vector2 weaponCenter = WeaponCenterOfRotation;
					weaponCenter.X *= projectile.spriteDirection;
					Vector2 tipCenter = projectile.Center + weaponCenter;
					Projectile.NewProjectile(
						tipCenter,
						angleVector,
						ProjectileType<SqueyereLaser>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}

				Main.PlaySound(SoundID.Item33, projectile.Center);
			}
		}


		protected override float WeaponDistanceFromCenter() => 6;

		protected override int WeaponHitboxEnd() => 6;

		public override float ComputeIdleSpeed() => 14;

		public override float ComputeTargetedSpeed() => 14;

		public override float MaxDistanceFromPlayer() => 60;
	}
}
