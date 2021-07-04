using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.BoneSquire
{
	public class BoneSquireMinionBuff : MinionBuff
	{
		public BoneSquireMinionBuff() : base(ProjectileType<BoneSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Bone Squire");
			Description.SetDefault("A bone squire will follow your orders!");
		}
	}

	public class BoneSquireMinionItem : SquireMinionItem<BoneSquireMinionBuff, BoneSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Bones");
			Tooltip.SetDefault("Summons a squire\nA bone squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 7.0f;
			item.width = 24;
			item.height = 38;
			item.damage = 40;
			item.value = Item.sellPrice(0, 0, 50, 0);
			item.rare = ItemRarityID.Orange;
		}
	}


	public class BoneSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<BoneSquireMinionBuff>();
		protected override int AttackFrames => usingSpecial ? 20 : 35;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/BoneWings";

		string BaseWeaponTexture = "AmuletOfManyMinions/Projectiles/Squires/BoneSquire/BoneSquireFlailBall";
		protected override string WeaponTexturePath => usingSpecial ? BaseWeaponTexture + "_Flaming" : BaseWeaponTexture;

		string BaseChainPath = "AmuletOfManyMinions/Projectiles/Squires/BoneSquire/BoneSquireFlailChain";
		protected string ChainTexturePath => usingSpecial ? BaseChainPath + "_Flaming" : BaseChainPath;
		// swing weapon in a full circle
		protected override float SwingAngle1 => SwingAngle0 - 2 * (float)Math.PI;

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override float knockbackSelf => 5f;
		protected override Vector2 WingOffset => new Vector2(-4, 2);

		protected override int SpecialDuration => 300;
		protected override int SpecialCooldown => 1200;
		public BoneSquireMinion() : base(ItemType<BoneSquireMinionItem>()) { }

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 22;
			projectile.height = 30;
			projectile.localNPCHitCooldown = AttackFrames / 3;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bone Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}


		protected override Rectangle GetWeaponTextureBounds(Texture2D texture)
		{
			if(usingSpecial)
			{
				int frame = (specialFrame / 5) % 4;
				int frameHeight = texture.Height / 4;
				return new Rectangle(0, frameHeight * frame, texture.Width, frameHeight);
			} else
			{
				return base.GetWeaponTextureBounds(texture);
			}
		}

		protected override void DrawWeapon(SpriteBatch spriteBatch, Color lightColor)
		{
			base.DrawWeapon(spriteBatch, usingSpecial ? Color.White : lightColor);
		}
		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (usingWeapon)
			{
				lightColor = usingSpecial ? Color.White : lightColor;
				Texture2D chainTexture = GetTexture(ChainTexturePath);
				Vector2 chainVector = UnitVectorFromWeaponAngle();
				float r = (float)Math.PI / 2 + chainVector.ToRotation();
				Vector2 center = projectile.Center + WeaponCenterOfRotation;
				Rectangle bounds = chainTexture.Bounds;
				Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
				int i;
				for (i = bounds.Height / 2; i < WeaponDistanceFromCenter(); i += bounds.Height)
				{
					Vector2 pos = center + chainVector * i;
					spriteBatch.Draw(chainTexture, pos - Main.screenPosition,
						bounds, lightColor, r,
						origin, 1, SpriteEffects.None, 0);
				}
			}
			base.PostDraw(spriteBatch, lightColor);
		}


		protected override float WeaponDistanceFromCenter() => 60;

		protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() - 10;
		protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 10;

		public override float ComputeIdleSpeed() => usingSpecial ? 11 : 9;

		public override float ComputeTargetedSpeed() => usingSpecial ? 11 : 9;

		public override float MaxDistanceFromPlayer() => usingSpecial ? 260 : 224;
	}
}
