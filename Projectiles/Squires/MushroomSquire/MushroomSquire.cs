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
		internal override int[] ProjectileTypes => new int[] { ProjectileType<MushroomSquireMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
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
			Item.knockBack = 2f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 9;
			Item.value = Item.sellPrice(0, 0, 0, 75);
			Item.rare = ItemRarityID.White;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddRecipeGroup(RecipeGroupID.Wood, 18).AddIngredient(ItemID.Mushroom, 8).AddTile(TileID.Anvils).Register();
		}
	}

	public class MushroomSquireMushroomProjectile : ModProjectile
	{
		const int TimeToLive = 180;
		const int TimeLeftToStartFalling = TimeToLive - 15;

		public override string Texture => "Terraria/Images/Item_" + ItemID.Mushroom;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.penetrate = 1;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.timeLeft = TimeToLive;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			//Projectile.minion = true; //TODO 1.4
			Projectile.DamageType = DamageClass.Summon;
		}

		public override void AI()
		{
			base.AI();
			if(Projectile.timeLeft < TimeLeftToStartFalling && Projectile.velocity.Y < 16)
			{
				Projectile.velocity.Y += 0.5f;
				Projectile.velocity.X *= 0.99f;
			}
			Projectile.rotation += MathHelper.Pi / 16 * Math.Sign(Projectile.velocity.X);
		}

		public override void Kill(int timeLeft)
		{
			base.Kill(timeLeft);
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.Center - Vector2.One * 16, 32, 32, DustID.Copper);
			}
			if(Projectile.owner == Main.myPlayer && Main.rand.Next(3) > 0)
			{
				Vector2 launcVel = new Vector2(0.25f * Projectile.velocity.X, -Main.rand.Next(5, 8));
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					launcVel,
					ProjectileType<ForagerMushroom>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner);

			}
		}
	}


	public class MushroomSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<MushroomSquireMinionBuff>();
		protected override int ItemType => ItemType<MushroomSquireMinionItem>();
		protected override int AttackFrames => 20;

		protected override int SpecialDuration => 30;
		protected override float projectileVelocity => 9;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/LeafWings";
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/MushroomSquire/MushroomSquireSword";

		protected override LegacySoundStyle SpecialStartSound => null;
		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Mushroom Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 30;
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.SpecialTargetedMovement(vectorToTargetPosition);
			if(specialFrame % 10 == 0 && player.whoAmI == Main.myPlayer)
			{
				Vector2 vector2Mouse = Vector2.DistanceSquared(Projectile.Center, Main.MouseWorld) < 48 * 48 ?
					Main.MouseWorld - player.Center : Main.MouseWorld - Projectile.Center;
				vector2Mouse.SafeNormalize();
				vector2Mouse *= ModifiedProjectileVelocity();
				vector2Mouse = vector2Mouse.RotatedBy(Main.rand.NextFloat(MathHelper.Pi / 8) - MathHelper.Pi/16);
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					vector2Mouse,
					ProjectileType<MushroomSquireMushroomProjectile>(),
					5 * Projectile.damage / 4,
					Projectile.knockBack,
					Projectile.owner);
				SoundEngine.PlaySound(new LegacySoundStyle(2, 5), Projectile.Center);
			}
		}

		public override float MaxDistanceFromPlayer() => 120;
		protected override float WeaponDistanceFromCenter() => 30;

	}
}
