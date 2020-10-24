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

namespace AmuletOfManyMinions.Projectiles.Squires.TitaniumSquire
{
    public class TitaniumSquireMinionBuff: MinionBuff
    {
        public TitaniumSquireMinionBuff() : base(ProjectileType<TitaniumSquireMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Titanium Squire");
			Description.SetDefault("An adamantite squire will follow your orders!");
        }
    }

    public class TitaniumSquireMinionItem: SquireMinionItem<TitaniumSquireMinionBuff, TitaniumSquireMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Titanium Crest");
			Tooltip.SetDefault("Summons a squire\nA titanium squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 5.5f;
			item.width = 24;
			item.height = 38;
            item.damage = 39;
			item.value = Item.buyPrice(0, 2, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.TitaniumBar, 14);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class TitaniumSquireMinion : WeaponHoldingSquire<TitaniumSquireMinionBuff>
    {
        protected override int AttackFrames => 35;

        protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";

        protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/TitaniumSquire/TitaniumSquireSpear";

        protected override Vector2 WingOffset => new Vector2(-6, 6);

        protected override float knockbackSelf => 5;
        protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 6);
        public TitaniumSquireMinion(): base(ItemType<TitaniumSquireMinionItem>()) {}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Titanium Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
            projectile.width = 22;
			projectile.height = 32;
		}

        protected override float WeaponDistanceFromCenter()
        {
            if(attackFrame <= 23)
            {
                return 4*attackFrame - 30;
            } else
            {
                return (4 * 23 - 30) - 4 * (attackFrame - 23);
            }
        }

        protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() + 20;

        protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 60; 

        public override float MaxDistanceFromPlayer() => 290;

        public override float ComputeTargetedSpeed() => 11;

        public override float ComputeIdleSpeed() => 11;
    }
}
