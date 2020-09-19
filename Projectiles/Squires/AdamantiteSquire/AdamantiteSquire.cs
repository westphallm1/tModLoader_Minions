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

namespace AmuletOfManyMinions.Projectiles.Squires.AdamantiteSquire
{
    public class AdamantiteSquireMinionBuff: MinionBuff
    {
        public AdamantiteSquireMinionBuff() : base(ProjectileType<AdamantiteSquireMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Adamantite Squire");
			Description.SetDefault("An adamantite squire will follow your orders!");
        }
    }

    public class AdamantiteSquireMinionItem: SquireMinionItem<AdamantiteSquireMinionBuff, AdamantiteSquireMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Adamantite Crest");
			Tooltip.SetDefault("Summons a squire\nAn adamantite squire will fight for you!\nClick to guide its attacks");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 5.5f;
			item.mana = 10;
			item.width = 24;
			item.height = 38;
            item.damage = 36;
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


    public class AdamantiteSquireSword : ModProjectile
    {
        // just to load a texture, probably overkill
    }

    public class AdamantiteSquireMinion : WeaponHoldingSquire<AdamantiteSquireMinionBuff>
    {


        protected override int AttackFrames { get => 30; }
        public AdamantiteSquireMinion(): base(ItemType<AdamantiteSquireMinionItem>()) {}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Adamantite Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 8;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
            projectile.width = 36;
			projectile.height = 32;
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.immune[projectile.owner] = AttackFrames/2;
            base.OnHitNPC(target, damage, knockback, crit);
        }

        protected override float GetWeaponAngle()
        {
            if(!usingWeapon && attackFrame == 0)
            {
                return 0;
            }

            Vector2 attackVector;
            // when the squire is close enough to the mouse, attack along the 
            // mouse-player line
            if(Vector2.Distance(Main.MouseWorld, projectile.Center) < 48)
            {
                attackVector = Main.MouseWorld - player.Center;
            } else
            {
                //otherwise, attack along the mouse-squire line
                attackVector = Main.MouseWorld - WeaponCenter();
            }
            if(projectile.spriteDirection == 1)
            {
                return -attackVector.ToRotation();
            } else
            {
                // this code is rather unfortunate, but need to normalize
                // everything to [-Math.PI/2, Math.PI/2] for arm drawing to work
                float angle = (float) -Math.PI + attackVector.ToRotation();
                if(angle < -Math.PI / 2)
                {
                    angle += 2*(float)Math.PI;
                }
                return angle;
            }
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            base.Animate(minFrame, 4);
        }

        protected override float WeaponOffset()
        {
            if(attackFrame <= 20)
            {
                return 4*attackFrame - 30;
            } else
            {
                return (4 * 20 - 30) - 4 * (attackFrame - 20);
            }
        }

        protected override int WeaponHitboxStart()
        {
            return (int)WeaponOffset() + 20;
        }

        protected override int WeaponHitboxEnd()
        {
            return (int)WeaponOffset() + 60;
        }

        protected override Vector2 WeaponCenter()
        {
            Vector2 center = projectile.Center;
            center.X += projectile.spriteDirection * 8;
            center.Y += 4;
            return center;
        }

        public override float MaxDistanceFromPlayer()
        {
            return 180;
        }

        public override float ComputeTargetedSpeed()
        {
            return 12;
        }

        public override float ComputeIdleSpeed()
        {
            return 12;
        }

        protected override Texture2D GetWeaponTexture()
        {
            return Main.projectileTexture[ProjectileType<AdamantiteSquireSword>()];
        }
    }
}
