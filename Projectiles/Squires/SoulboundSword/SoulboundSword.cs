using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.SoulboundSword
{
	public class SoulboundSwordMinionBuff : MinionBuff
	{
		public SoulboundSwordMinionBuff() : base(ProjectileType<SoulboundSwordMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Soulbound Sword");
			Description.SetDefault("A soulbound bow will follow your orders!");
		}
	}

	public class SoulboundSwordMinionItem : SquireMinionItem<SoulboundSwordMinionBuff, SoulboundSwordMinion>
	{

		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundSword/SoulboundSword";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Soulbound Sword");
			Tooltip.SetDefault("Summons a squire\nA soulbound sword will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
			item.damage = 32;
			item.value = Item.buyPrice(0, 0, 50, 0);
			item.rare = ItemRarityID.LightRed;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.ShadewoodSword, 1);
			recipe.AddIngredient(ItemID.SoulofNight, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
			ModRecipe recipe2 = new ModRecipe(mod);
			recipe2.AddIngredient(ItemID.EbonwoodSword, 1);
			recipe2.AddIngredient(ItemID.SoulofNight, 10);
			recipe2.AddTile(TileID.Anvils);
			recipe2.SetResult(this);
			recipe2.AddRecipe();
		}
	}

	public class SoulboundSwordMinion : WeaponHoldingSquire<SoulboundSwordMinionBuff>
	{
		protected override int AttackFrames => 15;
		protected override string WingTexturePath => null;
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/SoulboundSword/SoulboundSword";

		public override string Texture => "Terraria/Item_0";

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override float knockbackSelf => 4;

		public SoulboundSwordMinion() : base(ItemType<SoulboundSwordMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Soulbound Sword");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 22;
			projectile.height = 32;
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(projectile.Center, Color.LightPink.ToVector3() * 0.5f);
			return base.IdleBehavior();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (!usingWeapon && attackFrame == 0)
			{
				weaponAngle = -9 * (float)Math.PI / 16 +
					projectile.velocity.X * -projectile.spriteDirection * 0.01f;
				Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
				DrawWeapon(spriteBatch, translucentColor);
			}
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{

			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
			base.PostDraw(spriteBatch, translucentColor);
		}

		protected override float WeaponDistanceFromCenter() => 30;

		protected override int WeaponHitboxEnd() => 45;

		public override float ComputeIdleSpeed() => 13;

		public override float ComputeTargetedSpeed() => 13;

		public override float MaxDistanceFromPlayer() => 250;
	}
}
