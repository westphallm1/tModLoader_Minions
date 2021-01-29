using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
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
            item.damage = 10;
			item.value = Item.sellPrice(0, 0, 1, 0);
			item.rare = ItemRarityID.Orange;
		}
	}


	public class AncientCobaltBolt : ModProjectile
	{

		public override string Texture => "Terraria/Projectile_" + ProjectileID.WaterStream;

		public override void SetStaticDefaults()
		{
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.CloneDefaults(ProjectileID.WaterStream);
            projectile.magic = false; //Bandaid fix
		}
	}

	public class AncientCobaltSquireMinion : WeaponHoldingSquire
	{
		protected override int BuffId => BuffType<AncientCobaltSquireMinionBuff>();
        protected override int AttackFrames => 8;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/AncientCobaltSquire/AncientCobaltStaff";

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.DIAGONAL;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override LegacySoundStyle attackSound => new LegacySoundStyle(2, 13);

		protected override float projectileVelocity => 8;

		protected override bool travelRangeCanBeModified => false;

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
				angleVector *= ModifiedProjectileVelocity();
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
