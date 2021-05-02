using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire
{
	public class PumpkinSquireMinionBuff : MinionBuff
	{
		public PumpkinSquireMinionBuff() : base(ProjectileType<PumpkinSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Pumpkin Squire");
			Description.SetDefault("An pumpkin squire will follow your orders!");
		}
	}

	public class PumpkinSquireMinionItem : SquireMinionItem<PumpkinSquireMinionBuff, PumpkinSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Pumpkin Crest");
			Tooltip.SetDefault("Summons a squire\nA pumpkin squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3.5f;
			item.width = 24;
			item.height = 38;
			item.damage = 17;
			item.value = Item.sellPrice(0, 0, 1, 0);
			item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Pumpkin, 15);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}


	public class PumpkinBomb : ModProjectile
	{

		const int TIME_TO_LIVE = 120;
		int bounces;
		bool startFalling;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.timeLeft = TIME_TO_LIVE;
			projectile.friendly = true;
			projectile.tileCollide = true;
			projectile.penetrate = 3;
			bounces = 3;
			startFalling = false;
		}

		public override void AI()
		{
			if (projectile.timeLeft < TIME_TO_LIVE - 15)
			{
				startFalling = true;
			}
			if (startFalling)
			{
				projectile.velocity.Y += 0.5f;
				projectile.rotation += 0.2f * Math.Sign(projectile.velocity.X);
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (oldVelocity.Y > 0 && projectile.velocity.Y == 0)
			{
				projectile.velocity.Y = -3 * bounces;
				// make sure not to collide right away again
				projectile.position.Y -= 8;
				projectile.velocity.X *= 0.67f;
				bounces--;
				Main.PlaySound(SoundID.Dig, (int)projectile.position.X, (int)projectile.position.Y, 1, 1f, Main.rand.Next(1));
			}
			if (oldVelocity.Y < 0)
			{
				startFalling = true;
			}
			if (oldVelocity.X != 0 && projectile.velocity.X == 0)
			{
				projectile.velocity.X = -Math.Sign(oldVelocity.X) * 1.5f * bounces;
			}
			return bounces == 0;
		}

		public override void Kill(int timeLeft)
		{
			// don't explode
			Main.PlaySound(new LegacySoundStyle(4, 1).WithPitchVariance(.5f), projectile.position);
			Vector2 direction = -projectile.velocity;
			direction.Normalize();
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(projectile.position, 1, 1, DustType<PumpkinDust>(), -direction.X, -direction.Y, Alpha: 255, Scale: 2);
			}
		}
	}

	public class PumpkinSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<PumpkinSquireMinionBuff>();
		protected override int AttackFrames => 30;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/SpookyWings";
		protected override string WeaponTexturePath => null;

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override LegacySoundStyle attackSound => new LegacySoundStyle(2, 19);
		protected override float projectileVelocity => 8;

		protected override bool travelRangeCanBeModified => false;
		public PumpkinSquireMinion() : base(ItemType<PumpkinSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Pumpkin Squire");
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

		protected override float GetFixedWeaponAngle()
		{
			float angleStep = (SwingAngle1 - SwingAngle0) / 20;
			if (attackFrame <= 20)
			{
				return SwingAngle0 + angleStep * attackFrame;
			}
			else
			{
				return SwingAngle1;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				if (Main.myPlayer == player.whoAmI)
				{
					Vector2 vector2Mouse = UnitVectorFromWeaponAngle();
					vector2Mouse *= ModifiedProjectileVelocity();
					Projectile.NewProjectile(projectile.Center,
						vector2Mouse,
						ProjectileType<PumpkinBomb>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}
			}
		}

		protected override float WeaponDistanceFromCenter() => 12;

		public override float ComputeIdleSpeed() => 9;

		public override float ComputeTargetedSpeed() => 9;

		public override float MaxDistanceFromPlayer() => 50;
	}
}
