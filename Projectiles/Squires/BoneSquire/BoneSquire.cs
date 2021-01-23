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
			item.knockBack = 4.5f;
			item.width = 24;
			item.height = 38;
			item.damage = 29;
			item.value = Item.sellPrice(0, 0, 50, 0);
			item.rare = ItemRarityID.Orange;
		}
	}


	public class BoneSquireMinion : WeaponHoldingSquire
	{
		protected override int BuffId => BuffType<BoneSquireMinionBuff>();
		protected override int AttackFrames => 25;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/BoneWings";
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/BoneSquire/BoneSquireFlailBall";

		// swing weapon in a full circle
		protected override float SwingAngle1 => SwingAngle0 - 2 * (float)Math.PI;

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override float knockbackSelf => 5f;
		protected override Vector2 WingOffset => new Vector2(-4, 2);
		public BoneSquireMinion() : base(ItemType<BoneSquireMinionItem>()) { }

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 22;
			projectile.height = 30;
			projectile.localNPCHitCooldown = AttackFrames / 2;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bone Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}


		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (usingWeapon)
			{
				Texture2D chainTexture = GetTexture("AmuletOfManyMinions/Projectiles/Squires/BoneSquire/BoneSquireFlailChain");
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

		protected override float WeaponDistanceFromCenter() => 45;

		protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() - 10;
		protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 10;

		public override float ComputeIdleSpeed() => 9;

		public override float ComputeTargetedSpeed() => 9;

		public override float MaxDistanceFromPlayer() => 224;
	}
}
