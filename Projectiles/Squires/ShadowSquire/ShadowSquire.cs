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

namespace AmuletOfManyMinions.Projectiles.Squires.ShadowSquire
{
    public class ShadowSquireMinionBuff: MinionBuff
    {
        public ShadowSquireMinionBuff() : base(ProjectileType<ShadowSquireMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Shadow Squire");
			Description.SetDefault("A shadow squire will follow your orders!");
        }
    }

    public class ShadowSquireMinionItem: SquireMinionItem<ShadowSquireMinionBuff, ShadowSquireMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Shadows");
			Tooltip.SetDefault("Summons a squire\nA shadow squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
            item.damage = 19;
			item.value = Item.sellPrice(0, 0, 20, 0);
			item.rare = ItemRarityID.Green;
		}

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.DemoniteBar, 12);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }


    public class ShadowSquireMinion : WeaponHoldingSquire<ShadowSquireMinionBuff>
    {
        protected override int AttackFrames => 25;
        protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/DemonWings";
        protected override string WeaponTexturePath => "Terraria/Item_"+ItemID.WarAxeoftheNight;

        protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

        protected override Vector2 WingOffset => new Vector2(-4, 0);
        public ShadowSquireMinion() : base(ItemType<ShadowSquireMinionItem>()) { }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Shadow Squire");
            // Sets the amount of frames this minion has on its spritesheet
            Main.projFrames[projectile.type] = 5;
        }

        public sealed override void SetDefaults() {
            base.SetDefaults();
            projectile.width = 22;
            projectile.height = 32;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.immune[projectile.owner] = AttackFrames;
            base.OnHitNPC(target, damage, knockback, crit);
        }

        protected override float WeaponDistanceFromCenter() => 20;

        protected override int WeaponHitboxEnd() => 45;

        public override float ComputeIdleSpeed() => 9;

        public override float ComputeTargetedSpeed() => 9;

        public override float MaxDistanceFromPlayer() => 140;
    }
}
