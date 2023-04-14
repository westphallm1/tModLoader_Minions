using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static AmuletOfManyMinions.CrossModClient.SummonersShine.CrossModSetup;

namespace AmuletOfManyMinions.Projectiles.Squires.SoulboundBow
{
	public class SoulboundBowMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SoulboundBowMinion>() };
	}

	public class SoulboundBowMinionItem : SquireMinionItem<SoulboundBowMinionBuff, SoulboundBowMinion>
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundBow/SoulboundBow";

		public override void ApplyCrossModChanges()
		{
			WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, SummonersShineDefaultSpecialWhitelistType.RANGEDNOINSTASTRIKE);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 36;
			Item.value = Item.sellPrice(0, 0, 50, 0);
			Item.rare = ItemRarityID.LightRed;
			Item.noUseGraphic = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.PearlwoodBow, 1).AddIngredient(ItemID.SoulofLight, 10).AddTile(TileID.Anvils).Register();
		}

		public override void UseAnimation(Player player)
		{
			base.UseAnimation(player);
			Item.noUseGraphic = true;
		}
	}

	public class SoulboundSpecialSword : ModProjectile
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundSword/SoulboundSword";
		protected virtual Color LightColor => new Color(1f, 0f, 0.8f, 1f);

		internal static int TimeToLive = 15;
		private bool hasSpawned;

		public override void SetStaticDefaults()
		{
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = TimeToLive;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.tileCollide = false;
			Projectile.penetrate = 2;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}
		public override void AI()
		{
			// travel in a straight line along velocity
			if(!hasSpawned)
			{
				SpawnDust();
				hasSpawned = true;
			}
			Color lightColor = new Color(0.75f, 0f, 1f, 1f);
			Projectile.rotation = (float)Math.PI / 4 + Projectile.velocity.ToRotation();
			Lighting.AddLight(Projectile.Center, lightColor.ToVector3());
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			float colorStep = (float)Math.Sin(MathHelper.Pi * Projectile.timeLeft / TimeToLive);
			float colorIntensity = MathHelper.Lerp(0.75f, 1, colorStep);
			lightColor = Color.White * colorIntensity;
			lightColor.A /= 2;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				texture.Bounds, lightColor, Projectile.rotation,
				texture.Bounds.Center.ToVector2(), 1, 0, 0);
			return false;
		}
		public void SpawnDust()
		{
			for (float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 12)
			{
				Vector2 velocity = 1.5f * new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
				int dustCreated = Dust.NewDust(Projectile.position, 1, 1, 255, velocity.X, velocity.Y, 50, default, Scale: 1.4f);
				Main.dust[dustCreated].color = new Color(0.75f, 0f, 1f, 1f);
				Main.dust[dustCreated].noGravity = true;
				Main.dust[dustCreated].velocity *= 0.8f;
			}
		}
	}

	public class SoulboundArrow : ModProjectile
	{

		protected virtual Color LightColor => new Color(1f, 0f, 0.8f, 1f);
		public override void SetStaticDefaults()
		{
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			// projectile.ranged = false; //Bandaid fix
			//Projectile.minion = true; //TODO 1.4
			Projectile.DamageType = DamageClass.Summon;
		}

		public override void AI()
		{
			base.AI();
			Lighting.AddLight(Projectile.Center, Color.LightPink.ToVector3() * 0.5f);
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
			// don't spawn an arrow on kill
			for (float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 12)
			{
				Vector2 velocity = 1.5f * new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
				int dustCreated = Dust.NewDust(Projectile.position, 1, 1, 255, velocity.X, velocity.Y, 50, default(Color), Scale: 1.4f);
				Main.dust[dustCreated].noGravity = true;
				Main.dust[dustCreated].velocity *= 0.8f;
			}
		}
	}

	public class SoulboundBowMinion : WeaponHoldingSquire
	{
		public override int BuffId => BuffType<SoulboundBowMinionBuff>();
		protected override int ItemType => ItemType<SoulboundBowMinionItem>();
		protected override int AttackFrames => 25;
		protected override string WingTexturePath => null;
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/SoulboundBow/SoulboundBow";

		protected override float IdleDistanceMulitplier => 3;
		public override string Texture => "Terraria/Images/Item_0";

		protected override SoundStyle? attackSound => SoundID.Item5;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

		protected override Vector2 WingOffset => new Vector2(-4, 0);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override bool travelRangeCanBeModified => false;

		protected override float projectileVelocity => 18;

		protected override int SpecialDuration => 2 * 60;
		protected override int SpecialCooldown => 8 * 60;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 22;
			Projectile.height = 32;
			Projectile.tileCollide = false;
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(Projectile.Center, Color.LightPink.ToVector3() * 0.5f);
			return base.IdleBehavior();
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!usingWeapon && attackFrame == 0)
			{
				weaponAngle = Projectile.velocity.X * -Projectile.spriteDirection * 0.05f;
				Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
				DrawWeapon(translucentColor);
			}
			return false;
		}

		public override void PostDraw(Color lightColor)
		{

			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
			base.PostDraw(translucentColor);
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				if (Main.myPlayer == Player.whoAmI)
				{
					Vector2 angleVector = UnitVectorFromWeaponAngle();
					angleVector *= ModifiedProjectileVelocity();
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center,
						angleVector,
						ProjectileType<SoulboundArrow>(),
						Projectile.damage,
						Projectile.knockBack,
						Main.myPlayer);
				}
				SoundEngine.PlaySound(SoundID.Item39, Projectile.Center);
			}
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.IdleMovement(VectorToIdle);
			if(Player.whoAmI == Main.myPlayer && specialFrame % 8 == 1)
			{
				Vector2 center = Main.MouseWorld; // only run on main player safe to use
				// spawn two whole circles of swords over the course of the special
				Vector2 offset = (4 * MathHelper.Pi * specialFrame / SpecialDuration).ToRotationVector2();
				float spawnRadius = 96;
				float travelSpeed = 1.25f * spawnRadius / SoulboundSpecialSword.TimeToLive;
				Vector2 spawnPos = center + offset * spawnRadius;
				Vector2 spawnVelocity = -offset * travelSpeed;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					spawnPos,
					spawnVelocity,
					ProjectileType<SoulboundSpecialSword>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer);
				SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
			}
		}

		protected override float WeaponDistanceFromCenter() => 1;

		public override float ComputeIdleSpeed() => 9;

		public override float ComputeTargetedSpeed() => 9;

		public override float MaxDistanceFromPlayer() => 48;
	}
}
