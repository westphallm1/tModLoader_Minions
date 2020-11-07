using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
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

		protected float projectileVelocity = 20;

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
				Vector2 angleVector = UnitVectorFromWeaponAngle();
				angleVector *= projectileVelocity;
				Vector2 weaponCenter = WeaponCenterOfRotation;
				weaponCenter.X *= projectile.spriteDirection;
				Vector2 tipCenter = projectile.Center + weaponCenter;
				Projectile.NewProjectile(tipCenter,
					angleVector,
					ProjectileID.GreenLaser,
					projectile.damage,
					projectile.knockBack,
					Main.myPlayer);

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
