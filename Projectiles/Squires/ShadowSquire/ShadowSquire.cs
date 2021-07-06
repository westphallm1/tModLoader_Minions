using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.CrimsonSquire;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.ShadowSquire
{
	public class ShadowSquireMinionBuff : MinionBuff
	{
		public ShadowSquireMinionBuff() : base(ProjectileType<ShadowSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Shadow Squire");
			Description.SetDefault("A shadow squire will follow your orders!");
		}
	}

	public class ShadowSquireMinionItem : SquireMinionItem<ShadowSquireMinionBuff, ShadowSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Shadows");
			Tooltip.SetDefault("Summons a squire\nA shadow squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 4f;
			item.width = 20;
			item.height = 38;
			item.damage = 20;
			item.value = Item.sellPrice(0, 0, 20, 0);
			item.rare = ItemRarityID.Green;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.DemoniteBar, 12);
			recipe.AddIngredient(ItemID.ShadowScale, 6);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class CorruptFlaskProjectile : EvilSquireFlask
	{
		public override string Texture => "Terraria/Item_" + ItemID.FlaskofCursedFlames;
		protected override int DustId => 89;
		protected override int BuffId => BuffID.CursedInferno;
		protected override int BuffDuration => 420;
	}

	public class ShadowSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<ShadowSquireMinionBuff>();
		protected override int AttackFrames => 20;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/DemonWings";
		protected override string WeaponTexturePath => "Terraria/Item_" + ItemID.WarAxeoftheNight;

		protected override float projectileVelocity => 12;

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WingOffset => new Vector2(-4, 0);

		public ShadowSquireMinion() : base(ItemType<ShadowSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Shadow Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 22;
			projectile.height = 32;
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.SpecialTargetedMovement(vectorToTargetPosition);
			if(specialFrame == 1 && player.whoAmI == Main.myPlayer)
			{
				Vector2 vector2Mouse = Vector2.DistanceSquared(projectile.Center, Main.MouseWorld) < 48 * 48 ?
					Main.MouseWorld - player.Center : Main.MouseWorld - projectile.Center;
				vector2Mouse.SafeNormalize();
				vector2Mouse *= ModifiedProjectileVelocity();
				Projectile.NewProjectile(projectile.Center,
					vector2Mouse,
					ProjectileType<CorruptFlaskProjectile>(),
					3 * projectile.damage / 2,
					projectile.knockBack,
					Main.myPlayer,
					8);
			}
		}
		protected override float WeaponDistanceFromCenter() => 20;

		protected override int WeaponHitboxEnd() => 40;

		public override float ComputeIdleSpeed() => 9;

		public override float ComputeTargetedSpeed() => 9;

		public override float MaxDistanceFromPlayer() => 210;
	}
}
