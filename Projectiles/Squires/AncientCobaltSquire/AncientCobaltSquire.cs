using AmuletOfManyMinions.Projectiles.Minions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;

namespace AmuletOfManyMinions.Projectiles.Squires.AncientCobaltSquire
{
    public class AncientCobaltSquireMinionBuff: MinionBuff
    {
        public AncientCobaltSquireMinionBuff() : base(ProjectileType<AncientCobaltSquireMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Ancient Cobalt Squire");
			Description.SetDefault("An ancient cobalt squire will follow your orders!");
        }
    }

    public class AncientCobaltSquireMinionItem: SquireMinionItem<AncientCobaltSquireMinionBuff, AncientCobaltSquireMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ancient Crest of Cobalt");
			Tooltip.SetDefault("Summons a squire\nAn ancient cobalt squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
            item.damage = 18;
			item.value = Item.sellPrice(0, 0, 1, 0);
			item.rare = ItemRarityID.Orange;
		}
    }


    public class AncientCobaltArrow : ModProjectile
    {

        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
        }

        public override void Kill(int timeLeft)
        {
            // don't spawn a wood arrow on kill
        }
    }

    public class AncientCobaltSquireMinion : WeaponHoldingSquire<AncientCobaltSquireMinionBuff>
    {
        protected override int AttackFrames => 30;
        protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";
        protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/AncientCobaltSquire/AncientCobaltBow";

        protected override float IdleDistanceMulitplier => 2.5f;
        protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

        protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

        protected override Vector2 WingOffset => new Vector2(-4, 0);

        protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

        protected float projectileVelocity = 14;
        public AncientCobaltSquireMinion() : base(ItemType<AncientCobaltSquireMinionItem>()) { }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Ancient Cobalt Squire");
            // Sets the amount of frames this minion has on its spritesheet
            Main.projFrames[projectile.type] = 5;
        }

        public sealed override void SetDefaults() {
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
            if(attackFrame == 0)
            {
                Vector2 angleVector = UnitVectorFromWeaponAngle();
                angleVector *= projectileVelocity;
                Projectile.NewProjectile(projectile.Center,
                    angleVector, 
                    ProjectileType<AncientCobaltArrow>(), 
                    projectile.damage, 
                    projectile.knockBack, 
                    Main.myPlayer);
            }
        }

        protected override float WeaponDistanceFromCenter() => 12;

        public override float ComputeIdleSpeed() => 9;

        public override float ComputeTargetedSpeed() => 9;

        public override float MaxDistanceFromPlayer() => 50;
    }
}
