using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.CrimsonSquire;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.ShadowSquire
{
	public class ShadowSquireMinionBuff : MinionBuff
	{
		public ShadowSquireMinionBuff() : base(ProjectileType<ShadowSquireMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Shadow Squire");
			Description.SetDefault("A shadow squire will follow your orders!");
		}
	}

	public class ShadowSquireMinionItem : SquireMinionItem<ShadowSquireMinionBuff, ShadowSquireMinion>
	{
		protected override string SpecialName => "Flask of Cursed Flames";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Shadows");
			Tooltip.SetDefault("Summons a squire\nA shadow squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 4f;
			Item.width = 20;
			Item.height = 38;
			Item.damage = 20;
			Item.value = Item.sellPrice(0, 0, 20, 0);
			Item.rare = ItemRarityID.Green;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.DemoniteBar, 12).AddIngredient(ItemID.ShadowScale, 6).AddTile(TileID.Anvils).Register();
		}
	}

	public class CorruptFlaskProjectile : EvilSquireFlask
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.FlaskofCursedFlames;
		protected override int DustId => 89;
		protected override int BuffId => BuffID.CursedInferno;
		protected override int BuffDuration => 420;
	}

	public class ShadowSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<ShadowSquireMinionBuff>();
		protected override int AttackFrames => 20;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/DemonWings";
		protected override string WeaponTexturePath => "Terraria/Images/Item_" + ItemID.WarAxeoftheNight;

		protected override float projectileVelocity => 12;

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WingOffset => new Vector2(-4, 0);

		protected override LegacySoundStyle SpecialStartSound => new LegacySoundStyle(2, 106);

		public ShadowSquireMinion() : base(ItemType<ShadowSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Shadow Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 22;
			Projectile.height = 32;
		}

		public override void OnStartUsingSpecial()
		{
			if(player.whoAmI == Main.myPlayer)
			{
				Vector2 vector2Mouse = Vector2.DistanceSquared(Projectile.Center, Main.MouseWorld) < 48 * 48 ?
					Main.MouseWorld - player.Center : Main.MouseWorld - Projectile.Center;
				vector2Mouse.SafeNormalize();
				vector2Mouse *= ModifiedProjectileVelocity();
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(), 
					Projectile.Center,
					vector2Mouse,
					ProjectileType<CorruptFlaskProjectile>(),
					3 * Projectile.damage / 2,
					Projectile.knockBack,
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
