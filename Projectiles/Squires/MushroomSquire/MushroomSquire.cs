using AmuletOfManyMinions.Items.Armor;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.MushroomSquire
{
	public class MushroomSquireMinionBuff : MinionBuff
	{
		public MushroomSquireMinionBuff() : base(ProjectileType<MushroomSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Mushroom Squire");
			Description.SetDefault("A mushroom squire will follow your orders!");
		}
	}

	public class MushroomSquireMinionItem : SquireMinionItem<MushroomSquireMinionBuff, MushroomSquireMinion>
	{
		protected override string SpecialName => "Mushroom Toss";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of the Forest");
			Tooltip.SetDefault("Summons a squire\nA mushroom squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 2f;
			item.width = 24;
			item.height = 38;
			item.damage = 9;
			item.value = Item.sellPrice(0, 0, 0, 75);
			item.rare = ItemRarityID.White;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 18);
			recipe.AddIngredient(ItemID.Mushroom, 8);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class MushroomSquireMushroomProjectile : ModProjectile
	{
		const int TimeToLive = 180;
		const int TimeLeftToStartFalling = TimeToLive - 15;

		public override string Texture => "Terraria/Item_" + ItemID.Mushroom;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.penetrate = 1;
			projectile.width = 12;
			projectile.height = 12;
			projectile.timeLeft = TimeToLive;
			projectile.friendly = true;
			projectile.tileCollide = true;
			projectile.minion = true;
		}

		public override void AI()
		{
			base.AI();
			if(projectile.timeLeft < TimeLeftToStartFalling && projectile.velocity.Y < 16)
			{
				projectile.velocity.Y += 0.5f;
				projectile.velocity.X *= 0.99f;
			}
			projectile.rotation += MathHelper.Pi / 16 * Math.Sign(projectile.velocity.X);
		}

		public override void Kill(int timeLeft)
		{
			base.Kill(timeLeft);
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(projectile.Center - Vector2.One * 16, 32, 32, DustID.Copper);
			}
			if(projectile.owner == Main.myPlayer && Main.rand.Next(3) > 0)
			{
				Vector2 launcVel = new Vector2(0.25f * projectile.velocity.X, -Main.rand.Next(5, 8));
				Projectile.NewProjectile(
					projectile.Center,
					launcVel,
					ProjectileType<ForagerMushroom>(),
					projectile.damage,
					projectile.knockBack,
					projectile.owner);

			}
		}
	}


	public class MushroomSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<MushroomSquireMinionBuff>();
		protected override int AttackFrames => 20;

		protected override int SpecialDuration => 30;
		protected override float projectileVelocity => 9;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/LeafWings";
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/MushroomSquire/MushroomSquireSword";

		protected override LegacySoundStyle SpecialStartSound => null;
		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);
		public MushroomSquireMinion() : base(ItemType<MushroomSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Mushroom Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 30;
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.SpecialTargetedMovement(vectorToTargetPosition);
			if(specialFrame % 10 == 0 && player.whoAmI == Main.myPlayer)
			{
				Vector2 vector2Mouse = Vector2.DistanceSquared(projectile.Center, Main.MouseWorld) < 48 * 48 ?
					Main.MouseWorld - player.Center : Main.MouseWorld - projectile.Center;
				vector2Mouse.SafeNormalize();
				vector2Mouse *= ModifiedProjectileVelocity();
				vector2Mouse = vector2Mouse.RotatedBy(Main.rand.NextFloat(MathHelper.Pi / 8) - MathHelper.Pi/16);
				Projectile.NewProjectile(
					projectile.Center,
					vector2Mouse,
					ProjectileType<MushroomSquireMushroomProjectile>(),
					5 * projectile.damage / 4,
					projectile.knockBack,
					projectile.owner);
				Main.PlaySound(new LegacySoundStyle(2, 5), projectile.Center);
			}
		}

		public override float MaxDistanceFromPlayer() => 120;
		protected override float WeaponDistanceFromCenter() => 30;

	}
}
