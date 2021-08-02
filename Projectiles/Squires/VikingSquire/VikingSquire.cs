using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.VikingSquire
{
	public class VikingSquireMinionBuff : MinionBuff
	{
		public VikingSquireMinionBuff() : base(ProjectileType<VikingSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Viking Squire");
			Description.SetDefault("A dual-wielding viking squire will follow your orders!");
		}
	}

	public class VikingSquireMinionItem : SquireMinionItem<VikingSquireMinionBuff, VikingSquireMinion>
	{
		protected override string SpecialName => "Icy Axe";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of the Frozen North");
			Tooltip.SetDefault("Summons a squire\nA viking squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 4.5f;
			item.width = 24;
			item.height = 38;
			item.damage = 9;
			item.value = Item.sellPrice(0, 0, 50, 0);
			item.rare = ItemRarityID.Blue;
		}
	}


	public class VikingSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<VikingSquireMinionBuff>();
		protected override int AttackFrames => 18;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/BoneWings";

		private string baseWeaponPath = "AmuletOfManyMinions/Projectiles/Squires/VikingSquire/VikingSquireAxe";
		protected override string WeaponTexturePath => usingSpecial ? baseWeaponPath + "_Frozen" : baseWeaponPath;

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override float knockbackSelf => 5f;
		protected override Vector2 WingOffset => new Vector2(-4, 2);
		protected override Vector2 WeaponCenterOfRotation => new Vector2(2, 4);

		protected override int SpecialDuration => 4 * 60;
		protected override int SpecialCooldown => 10 * 60;

		protected int swingDirection = 1;
		public VikingSquireMinion() : base(ItemType<VikingSquireMinionItem>()) { }

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			bool doPreDraw = base.PreDraw(spriteBatch, lightColor);
			if (usingWeapon)
			{
				float oppositeWeaponAngle = SwingAngle1 - weaponAngle + SwingAngle0;
				float myWeaponAngle = weaponAngle;
				// draw the weapon again offset 180 degrees
				weaponAngle = oppositeWeaponAngle;
				DrawWeapon(spriteBatch, lightColor);
				weaponAngle = myWeaponAngle;
			}
			return doPreDraw;
		}
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
			DisplayName.SetDefault("Viking Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public override Vector2 IdleBehavior()
		{
			if (attackFrame == AttackFrames - 1)
			{
				swingDirection *= -1;
			}
			return base.IdleBehavior();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			bool? reallyColliding = base.Colliding(projHitbox, targetHitbox);
			float oppositeWeaponAngle = SwingAngle1 - weaponAngle + SwingAngle0;
			float myWeaponAngle = weaponAngle;
			// draw the weapon again offset 180 degrees
			weaponAngle = oppositeWeaponAngle;
			bool? otherColliding = base.Colliding(projHitbox, targetHitbox);
			weaponAngle = myWeaponAngle;
			return (reallyColliding ?? false) || (otherColliding ?? false);
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(usingSpecial && attackFrame == 0)
			{
				Main.PlaySound(attackSound, projectile.Center);
			}
			base.StandardTargetedMovement(vectorToTargetPosition);
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.SpecialTargetedMovement(vectorToTargetPosition);
			if (Main.rand.Next(8) == 0)
			{
				int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<SnowDust>(), 0f, 0f, 100, default, 1f);
				Main.dust[dustId].velocity *= 0.3f;
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].noLight = true;
			}
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if(usingSpecial)
			{
				damage = 3 * damage / 2;
				knockback *= 0.75f;
			}
		}

		protected override float GetFixedWeaponAngle()
		{
			float swingAngle = base.GetFixedWeaponAngle();
			return swingDirection == 1 ? swingAngle : SwingAngle1 - swingAngle + SwingAngle0;
		}
		protected override float WeaponDistanceFromCenter() => 25;

		protected override int WeaponHitboxStart() => 25;
		protected override int WeaponHitboxEnd() => usingSpecial ? 60 : 45;

		public override float ComputeIdleSpeed() => usingSpecial ? 10 : 8;

		public override float ComputeTargetedSpeed() => usingSpecial ? 10 : 8;

		public override float MaxDistanceFromPlayer() => usingSpecial ? 220 : 140;
	}
}
