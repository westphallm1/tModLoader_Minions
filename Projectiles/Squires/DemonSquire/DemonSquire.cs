using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.DemonSquire
{
	public class DemonSquireMinionBuff : MinionBuff
	{
		public DemonSquireMinionBuff() : base(ProjectileType<DemonSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Demon Squire");
			Description.SetDefault("A bone squire will follow your orders!");
		}
	}

	public class DemonSquireMinionItem : SquireMinionItem<DemonSquireMinionBuff, DemonSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Demons");
			Tooltip.SetDefault("Summons a squire\nA bone squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 7.0f;
			item.width = 24;
			item.height = 38;
			item.damage = 32;
			item.value = Item.sellPrice(0, 0, 50, 0);
			item.rare = ItemRarityID.Orange;
		}
	}


	public class DemonSquireUnholyTrident: BaseMinionUnholyTrident
	{
		private Vector2 baseVelocity;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.timeLeft = 30;
		}
		public override void AI()
		{
			base.AI();
			Projectile parent = Main.projectile[(int)projectile.ai[0]];
			if (baseVelocity == default)
			{
				baseVelocity = projectile.velocity;
			}
			if (parent.active)
			{
				projectile.velocity = parent.velocity + baseVelocity;
			}
			projectile.rotation = baseVelocity.ToRotation() + MathHelper.Pi/4;
		}
	}


	public class DemonSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<DemonSquireMinionBuff>();
		protected override int AttackFrames => 25;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/DemonWings";
		protected override string WeaponTexturePath => null;

		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override float knockbackSelf => 5f;
		protected override Vector2 WingOffset => new Vector2(-4, 2);

		protected override int SpecialDuration => 8 * 60;
		protected override int SpecialCooldown => 12 * 60;

		protected override float projectileVelocity => 12;
		public DemonSquireMinion() : base(ItemType<DemonSquireMinionItem>()) { }

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 22;
			projectile.height = 30;
			drawOriginOffsetY = -8;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Demon Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0 && Main.myPlayer == player.whoAmI)
			{
				Vector2 vector2Mouse = UnitVectorFromWeaponAngle();
				vector2Mouse *= ModifiedProjectileVelocity();
				Projectile.NewProjectile(projectile.Center,
					vector2Mouse,
					ProjectileType<DemonSquireUnholyTrident>(),
					projectile.damage,
					projectile.knockBack,
					Main.myPlayer,
					ai0: projectile.whoAmI);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

		protected override float WeaponDistanceFromCenter() => 12;

		public override float ComputeIdleSpeed() => 11;

		public override float ComputeTargetedSpeed() => 11;

		public override float MaxDistanceFromPlayer() => 232;
	}
}
