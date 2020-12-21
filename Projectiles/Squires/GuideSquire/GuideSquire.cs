using AmuletOfManyMinions.Dusts;
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
		public GuideSquireMinionBuff() : base(ProjectileType<GuideSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Guide Squire");
			Description.SetDefault("You can guide the Guide!");
		}
	}

	public class GuideSquireMinionItem : SquireMinionItem<GuideSquireMinionBuff, GuideSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Guide Friendship Bracelet");
			Tooltip.SetDefault("Summons a squire\nClick and hold to guide its attacks!\n'Maybe you're not such a terrible person...'");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
			item.damage = 15;
			item.value = Item.sellPrice(0, 0, 1, 0);
			item.rare = ItemRarityID.Blue;
		}
	}


	public class GuideArrow : ModProjectile
	{

		public override string Texture => "Terraria/Projectile_" + ProjectileID.FireArrow;
		public override void SetStaticDefaults()
		{
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.CloneDefaults(ProjectileID.FireArrow);
		}

		public override void Kill(int timeLeft)
		{
			// don't spawn the arrow
			for(int i = 0; i < 6; i++)
			{
				Dust.NewDust(projectile.position, projectile.width, projectile.height, 158);
			}
		}

		public override void AI()
		{
			base.AI();
			Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Fire);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
			return base.OnTileCollide(oldVelocity);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if(Main.rand.Next(3) == 0)
			{
				target.AddBuff(BuffID.OnFire, 180);
			}
		}
	}

	public class GuideSquireMinion : WeaponHoldingSquire<GuideSquireMinionBuff>
	{
		protected override int AttackFrames => 40;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";
		protected override string WeaponTexturePath => "Terraria/Item_" + ItemID.WoodenBow;

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override LegacySoundStyle attackSound => new LegacySoundStyle(2, 5);

		protected override float projectileVelocity => 12;
		protected override bool travelRangeCanBeModified => false;


		public GuideSquireMinion() : base(ItemType<GuideSquireMinionItem>()) { }

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
				Vector2 angleVector = UnitVectorFromWeaponAngle();
				angleVector *= ModifiedProjectileVelocity();
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(projectile.Center,
						angleVector,
						ProjectileType<GuideArrow>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}
			}
		}


		protected override float WeaponDistanceFromCenter() => 6;

		public override float ComputeIdleSpeed() => 8;

		public override float ComputeTargetedSpeed() => 8;

		public override float MaxDistanceFromPlayer() => 50;
	}
}
