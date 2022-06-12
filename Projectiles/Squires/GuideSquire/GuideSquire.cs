using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.GuideSquire
{
	public class GuideSquireMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<GuideSquireMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Guide Squire");
			Description.SetDefault("You can guide the Guide!");
		}
	}

	public class GuideSquireMinionItem : SquireMinionItem<GuideSquireMinionBuff, GuideSquireMinion>
	{
		protected override string SpecialName => "Flaming Arrow Volley";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Guide Friendship Bracelet");
			Tooltip.SetDefault("Summons a squire\nClick and hold to guide its attacks!\n'Maybe you're not such a terrible person...'");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 24;
			Item.value = Item.sellPrice(0, 0, 1, 0);
			Item.rare = ItemRarityID.Blue;
		}
	}


	// cosmetic arrow for the 'shooting up' animation
	public class AscendingGuideArrow : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FireArrow;

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 60;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6);
			//TODO screenposition
			Vector2 myScreenPosition = Main.player[Projectile.owner].Center 
				- new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
			if(Projectile.position.Y < myScreenPosition.Y)
			{
				Projectile.Kill();
			}
		}
	}

	public abstract class BaseGuideArrow : ModProjectile
	{

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FireArrow;
		public override void SetStaticDefaults()
		{
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}

		public override void Kill(int timeLeft)
		{
			// don't spawn the arrow
			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			for (int i = 0; i < 6; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 158);
			}
		}

		public override void AI()
		{
			base.AI();
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			return base.OnTileCollide(oldVelocity);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (Main.rand.NextBool(3))
			{
				target.AddBuff(BuffID.OnFire, 180);
			}
		}
	}

	public class GuideArrow : BaseGuideArrow
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.CloneDefaults(ProjectileID.FireArrow);
		}
	}

	public class DescendingGuideArrow : BaseGuideArrow
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
			projHitbox.Inflate(32, 32);
			return projHitbox.Intersects(targetHitbox);
		}

		public override void AI()
		{
			base.AI();
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			// start colliding with tiles 1/3 of the way down the screen
			Vector2 position = Projectile.position;
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
	}

	public class GuideSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<GuideSquireMinionBuff>();
		protected override int ItemType => ItemType<GuideSquireMinionItem>();
		protected override int AttackFrames => 35;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";
		protected override string WeaponTexturePath => "Terraria/Images/Item_" + ItemID.WoodenBow;

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override SoundStyle? attackSound => SoundID.Item5;

		protected override float projectileVelocity => 12;

		protected override int SpecialDuration => 2 * 60;
		protected override int SpecialCooldown => 9 * 60;
		protected override bool travelRangeCanBeModified => false;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Guide Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 22;
			Projectile.height = 32;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}


		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				Vector2 angleVector = UnitVectorFromWeaponAngle();
				angleVector *= ModifiedProjectileVelocity();
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(), 
						Projectile.Center,
						angleVector,
						ProjectileType<GuideArrow>(),
						Projectile.damage,
						Projectile.knockBack,
						Main.myPlayer);
				}
			}
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			weaponAngle = MathHelper.PiOver2;
			if (specialFrame <= 31 && specialFrame % 10 == 1 && Main.myPlayer == player.whoAmI)
			{
				float launchAngle = -(7 * MathHelper.Pi / 16 + Main.rand.NextFloat(MathHelper.Pi / 8));
				Vector2 launchVec = launchAngle.ToRotationVector2() * 20;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(), 
					Projectile.Center,
					launchVec,
					ProjectileType<AscendingGuideArrow>(),
					0,
					0,
					Main.myPlayer);
			}
			else if (specialFrame > 31 && specialFrame % 8 == 1 && Main.myPlayer == player.whoAmI)
			{
				// spawn a downward facing arrow about halfway between the player and the mouse,
				// angled towards the mouse
				int spawnPosRange = 64;
				float spawnAngleRange = MathHelper.Pi / 24;
				Vector2 mousePos = Main.MouseWorld; // only run on client player, MP safe
				float spawnX = (mousePos.X + player.position.X) / 2 + 
					Main.rand.Next(-spawnPosRange, spawnPosRange);
				Vector2 myScreenPosition = Main.player[Projectile.owner].Center 
					- new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
				float spawnY = myScreenPosition.Y;
				Vector2 spawnPos = new Vector2(spawnX, spawnY);
				Vector2 launchAngle = (mousePos - spawnPos).RotatedBy(
					Main.rand.NextFloat(spawnAngleRange) - spawnAngleRange/2);
				launchAngle.SafeNormalize();
				launchAngle *= 20;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(), 
					spawnPos,
					launchAngle,
					ProjectileType<DescendingGuideArrow>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer);
				if (attackSound.HasValue)
				{
					SoundEngine.PlaySound(attackSound.Value, spawnPos);
				}
			}

		}


		protected override float WeaponDistanceFromCenter() => 6;

		public override float ComputeIdleSpeed() => 8;

		public override float ComputeTargetedSpeed() => 8;

		public override float MaxDistanceFromPlayer() => 50;
	}
}
