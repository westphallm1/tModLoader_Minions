using AmuletOfManyMinions.Projectiles.Minions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

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
			DisplayName.SetDefault("Mushroom Squire Staff");
			Tooltip.SetDefault("Summons a squire\nA mushroom squire will fight for you!\nClick to guide its attacks!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
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


    public class MushroomSquireMinion : SquireMinion<MushroomSquireMinionBuff>
    {


        private bool usingWeapon = false;
        private int attackFrame = 0;
        private float weaponAngle = 0;
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
			projectile.tileCollide = false;
            projectile.type = ProjectileType<MushroomSquireMinion>();
            projectile.friendly = false;
            useBeacon = false;
		}

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Rectangle bounds = new Rectangle(0, 4 * 42, 26, 30);
            Vector2 pos = projectile.Center;
            float r = projectile.rotation;
            if(usingWeapon)
            {

            }
            SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
            Vector2 origin = new Vector2(bounds.Width / 2f, bounds.Height / 2f);
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                bounds, lightColor, r,
                origin, 1, effects, 0);
            return;
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            maxFrame = 4;
            if(vectorToTarget is Vector2 target)
            {
                projectile.spriteDirection = Math.Sign((Main.MouseWorld - player.position).X);
            } else if(projectile.velocity.X < -1)
            {
                projectile.spriteDirection = -1;
            } else if (projectile.velocity.X > 1)
            {
                projectile.spriteDirection = 1;
            }
            projectile.rotation = projectile.velocity.X * 0.05f;
            base.Animate(minFrame, maxFrame);
        }
    }
}
