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

namespace AmuletOfManyMinions.Projectiles.Squires.MushroomSquire
{
    public class MushroomSquireMinionBuff: MinionBuff
    {
        public MushroomSquireMinionBuff() : base(ProjectileType<MushroomSquireMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Mushroom Squire");
			Description.SetDefault("A mushroom squire will follow your orders!");
        }
    }

    public class MushroomSquireMinionItem: SquireMinionItem<MushroomSquireMinionBuff, MushroomSquireMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of the Forest");
			Tooltip.SetDefault("Summons a squire\nA mushroom squire will fight for you!\nClick to guide its attacks");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 24;
			item.height = 38;
            item.damage = 12;
			item.value = Item.buyPrice(0, 20, 0, 0);
			item.rare = ItemRarityID.White;
		}

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SpectreBar, 12);
            recipe.AddIngredient(ItemID.IllegalGunParts, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }


    public class MushroomSquireSword : ModProjectile
    {
        // just to load a texture, probably overkill
    }

    public class MushroomSquireMinion : WeaponHoldingSquire<MushroomSquireMinionBuff>
    {


        protected override int AttackFrames { get => 20; }
        public MushroomSquireMinion(): base(ItemType<MushroomSquireMinionItem>()) {}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Mushroom Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 8;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 26;
			projectile.height = 30;
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.immune[projectile.owner] = AttackFrames;
            base.OnHitNPC(target, damage, knockback, crit);
        }

        protected override float GetWeaponAngle()
        {
            if(!usingWeapon && attackFrame == 0)
            {
                return 0;
            }
            float angle0 = 5 * (float)Math.PI / 8;
            float angle1 = -(float)Math.PI/4;
            float angleStep = (angle1 - angle0) / AttackFrames;
            return angle0 + angleStep * attackFrame;
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            base.Animate(minFrame, 4);
        }

        protected override float WeaponOffset()
        {
            return 20;
        }

        protected override Texture2D GetWeaponTexture()
        {
            return Main.projectileTexture[ProjectileType<MushroomSquireSword>()];
        }
    }
}
