using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.AncientCobaltSquire
{
	public class AncientCobaltSquireMinionBuff : MinionBuff
	{
		public AncientCobaltSquireMinionBuff() : base(ProjectileType<AncientCobaltSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Ancient Cobalt Squire");
			Description.SetDefault("An ancient cobalt squire will follow your orders!");
		}
	}

	public class AncientCobaltSquireMinionItem : SquireMinionItem<AncientCobaltSquireMinionBuff, AncientCobaltSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ancient Crest of Cobalt");
			Tooltip.SetDefault("Summons a squire\nAn ancient cobalt squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
			item.damage = 18;
			item.value = Item.sellPrice(0, 0, 1, 0);
			item.rare = ItemRarityID.Orange;
		}
	}


	public class AncientCobaltBolt : ModProjectile
	{

		public override string Texture => "Terraria/Projectile_" + ProjectileID.SapphireBolt;

		public override void SetStaticDefaults()
		{
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.CloneDefaults(ProjectileID.SapphireBolt);
		}

		public override void AI()
		{
			base.AI();
			for (int i = 0; i < 2; i ++)
			{
				int dustSpawned = Dust.NewDust(projectile.position, projectile.width, projectile.height, 88, projectile.velocity.X, projectile.velocity.Y, 50, default, 1.2f);
				Main.dust[dustSpawned].noGravity = true;
				Main.dust[dustSpawned].velocity *= 0.3f;
			}
			if (projectile.localAI[0] == 0f)
			{
				projectile.localAI[0] = 1f;
				Main.PlaySound(SoundID.Item8, projectile.Center);
			}
		}
		public override void Kill(int timeLeft)
		{
			base.Kill(timeLeft);
			Main.PlaySound(SoundID.Dig, (int)projectile.position.X, (int)projectile.position.Y);
			for (int i = 0; i < 15; i++)
			{
				int dustCreated = Dust.NewDust(projectile.position, projectile.width, projectile.height, 88, projectile.oldVelocity.X, projectile.oldVelocity.Y, 50, default(Color), 1.2f);
				Main.dust[dustCreated].noGravity = true;
				Main.dust[dustCreated].scale *= 1.25f;
				Main.dust[dustCreated].velocity *= 0.5f;
			}
		}
	}

	public class AncientCobaltSquireMinion : WeaponHoldingSquire<AncientCobaltSquireMinionBuff>
	{
		protected override int AttackFrames => 30;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/AncientCobaltSquire/AncientCobaltStaff";

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.DIAGONAL;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected float projectileVelocity = 14;
		public AncientCobaltSquireMinion() : base(ItemType<AncientCobaltSquireMinionItem>()) { }

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
				angleVector *= projectileVelocity;
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(projectile.Center,
						angleVector,
						ProjectileType<AncientCobaltBolt>(),
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
