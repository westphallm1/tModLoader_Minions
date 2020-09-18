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

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Main.NewText("I'm being called!");
            return false;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Main.NewText("I'm also being called!");
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

    public class MushroomSquireMinion : SquireMinion<MushroomSquireMinionBuff>
    {


        private bool usingWeapon = false;
        private const int AttackFrames = 20;
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
            projectile.friendly = true;
            projectile.usesLocalNPCImmunity = true;
            useBeacon = false;
		}

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if(!usingWeapon)
            {
                return false;
            }
            if(!Collision.CanHitLine(projectile.Center, 1, 1, targetHitbox.Center.ToVector2(), 1, 1)) {
                return false;
            }
            Vector2 unitAngle = UnitVectorFromWeaponAngle();
            int weaponLength = 40;
            for(int i = 16; i < weaponLength; i+= 8)
            {
                Vector2 tipCenter = projectile.Center + i * unitAngle;
                Rectangle tipHitbox = new Rectangle((int)tipCenter.X - 8, (int)tipCenter.Y - 8, 16, 16);
                if(tipHitbox.Intersects(targetHitbox))
                {
                    return true;
                }
            }
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.immune[projectile.owner] = AttackFrames;
            base.OnHitNPC(target, damage, knockback, crit);
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            usingWeapon = true;
            weaponAngle = GetWeaponAngle();
            base.TargetedMovement(vectorToTargetPosition);
        }

        public override void IdleMovement(Vector2 vectorToIdlePosition)
        {
            usingWeapon = false;
            weaponAngle = GetWeaponAngle();
            base.IdleMovement(vectorToIdlePosition);
        }

        private float GetWeaponAngle()
        {
            if(!usingWeapon && attackFrame == 0)
            {
                return 0;
            }
            float angle0 = 5 * (float)Math.PI / 8;
            float angle1 = -(float)Math.PI/4;
            float angleStep = (angle1 - angle0) / AttackFrames;
            attackFrame = (attackFrame + 1) % AttackFrames;
            return angle0 + angleStep * attackFrame;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Rectangle bounds = new Rectangle(0, 4 * 42, 26, 30);
            Vector2 origin = new Vector2(bounds.Width / 2f, bounds.Height / 2f);
            Vector2 pos = projectile.Center;
            float r = projectile.rotation;
            SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
            if(usingWeapon)
            {
                if(weaponAngle > (float)Math.PI / 8)
                {
                    bounds = new Rectangle(0, 5 * 42, 26, 30);
                } else if(weaponAngle > -Math.PI/8)
                {
                    bounds = new Rectangle(0, 6 * 42, 26, 30);
                } else
                {
                    bounds = new Rectangle(0, 7 * 42, 26, 30);
                }
                DrawWeapon(spriteBatch, lightColor);
            }
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                bounds, lightColor, r,
                origin, 1, effects, 0);
        }

        protected Vector2 UnitVectorFromWeaponAngle()
        {
            if(projectile.spriteDirection == 1)
            {
                return new Vector2((float)Math.Cos(-weaponAngle), (float)Math.Sin(-weaponAngle));
            } else
            {
                var reflectedAngle = Math.PI - weaponAngle;
                return new Vector2((float)Math.Cos(-reflectedAngle), (float)Math.Sin(-reflectedAngle));
            }
        }

        protected float SpriteRotationFromWeaponAngle()
        {
            if(projectile.spriteDirection == 1)
            {
                return (float)Math.PI/4-weaponAngle;
            } else
            {
                return -((float)Math.PI/4-weaponAngle);
            }
        }
        private void DrawWeapon(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[ProjectileType<MushroomSquireSword>()];
            Rectangle bounds = new Rectangle(0, 0, 26, 28);
            Vector2 origin = new Vector2(bounds.Width/2, bounds.Height/2); // origin should hopefully be more or less center of squire
            Vector2 center = UnitVectorFromWeaponAngle() * 20;
            float r = SpriteRotationFromWeaponAngle();
            Vector2 pos = projectile.Center + center;
            SpriteEffects effects =  projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, pos - Main.screenPosition,
                bounds, lightColor, r,
                origin, 1, effects, 0);
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
