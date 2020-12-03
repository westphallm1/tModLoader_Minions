using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.GoldenRogueSquire
{
	public class GoldenRogueSquireMinionBuff : MinionBuff
	{
		public GoldenRogueSquireMinionBuff() : base(ProjectileType<GoldenRogueSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Golden Rogue Squire");
			Description.SetDefault("An golden rogue squire will follow your orders!");
		}
	}

	public class GoldenRogueSquireMinionItem : SquireMinionItem<GoldenRogueSquireMinionBuff, GoldenRogueSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Golden Rogue Crest");
			Tooltip.SetDefault("Summons a squire\nAn adamantite squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 5.5f;
			item.width = 24;
			item.height = 38;
			item.damage = 25;
			item.value = Item.buyPrice(0, 2, 0, 0);
			item.rare = ItemRarityID.Orange;
		}
	}

	public class GoldenRogueSquireMinion : WeaponHoldingSquire<GoldenRogueSquireMinionBuff>
	{
		protected override int AttackFrames => 25;

		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/GoldenWings";

		protected override string WeaponTexturePath => "Terraria/Item_"+ItemID.MagicDagger;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override float knockbackSelf => 5;
		protected override Vector2 WeaponCenterOfRotation => new Vector2(-4, 4);

		private int projectileVelocity = 8;
		public GoldenRogueSquireMinion() : base(ItemType<GoldenRogueSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Golden Rogue Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 32;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// glowy golden wings
			return base.PreDraw(spriteBatch, Color.White);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				if (Main.myPlayer == player.whoAmI)
				{
					Vector2 angleVector = UnitVectorFromWeaponAngle();
					angleVector.Normalize();
					angleVector*= projectileVelocity;
					angleVector += projectile.velocity;
					Projectile.NewProjectile(projectile.Center,
						angleVector,
						ProjectileID.MagicDagger,
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
					
				}
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D glow = GetTexture(Texture + "_Glow");
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Rectangle bounds = glow.Bounds;
			Vector2 origin = bounds.Center.ToVector2();
			spriteBatch.Draw(glow, pos - Main.screenPosition,
				bounds, Color.White, r,
				origin, 1, effects, 0);
			base.PostDraw(spriteBatch, lightColor);
		}

		public override float MaxDistanceFromPlayer() => 300;

		public override float ComputeTargetedSpeed() => 12;

		protected override float WeaponDistanceFromCenter() => 12;

		public override float ComputeIdleSpeed() => 12;
	}
}
