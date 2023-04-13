using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SoulboundBow;
using AmuletOfManyMinions.Projectiles.Squires.SoulboundSword;
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

namespace AmuletOfManyMinions.Projectiles.Squires.SoulboundArsenal
{
	public class SoulboundArsenalMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SoulboundArsenalMinion>() };
	}

	public class SoulboundArsenalMinionItem : SquireMinionItem<SoulboundArsenalMinionBuff, SoulboundArsenalMinion>
	{

		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundArsenal/SoulboundArsenalItem";

		protected override string SpecialName => "Soulbound Coalescence";
		
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
			Item.damage = 75;
			Item.value = Item.sellPrice(0, 8, 0, 0);
			Item.rare = ItemRarityID.Lime;
			Item.noUseGraphic = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.BrokenHeroSword, 1).AddIngredient(ItemType<SoulboundSwordMinionItem>(), 1).AddIngredient(ItemType<SoulboundBowMinionItem>(), 1).AddTile(TileID.MythrilAnvil).Register();
		}

		public override void UseAnimation(Player player)
		{
			base.UseAnimation(player);
			Item.noUseGraphic = true;
		}
	}

	/// <summary>
	/// Uses ai[0] and 1 for amplitude and phase
	/// </summary>
	public abstract class SoulboundArsenalProjectile : ModProjectile
	{
		public static int TimeLeft = 180;
		protected Vector2 expectedPosition = Vector2.Zero;

		protected virtual Color LightColor => Color.White;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = 180;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
			Projectile.width = 8;
			Projectile.height = 8;
		}

		public abstract float GetRotation();

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				texture.Bounds, translucentColor, Projectile.rotation,
				texture.Bounds.Center.ToVector2(), 1, SpriteEffects.None, 0);
			return false;
		}
		public override void AI()
		{
			if (expectedPosition == Vector2.Zero)
			{
				expectedPosition = Projectile.position;
				Projectile.rotation = GetRotation();
			}
			expectedPosition += Projectile.velocity;
			Vector2 velocityTangent = new Vector2(Projectile.velocity.Y, -Projectile.velocity.X);
			velocityTangent.Normalize();
			velocityTangent *= Projectile.ai[0] * (float)Math.Sin(Projectile.ai[1] + 16 * Math.PI * (Projectile.timeLeft) / TimeLeft);
			Projectile.position = expectedPosition + velocityTangent;
			Lighting.AddLight(Projectile.Center, LightColor.ToVector3() * 0.5f);
			int dustId  = Dust.NewDust(Projectile.Center, 1, 1, 255, newColor: LightColor, Scale: 1.2f);
			Main.dust[dustId].noGravity = true;
			Main.dust[dustId].velocity *= 0.8f;
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
			for (float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 12)
			{
				Vector2 velocity = 1.5f * new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
				int dustId = Dust.NewDust(Projectile.Center, 1, 1, 255, velocity.X, velocity.Y, newColor: LightColor, Scale: 1.2f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity *= 0.8f;
			}
		}

	}
	public class SoulboundArsenalArrow : SoulboundArsenalProjectile
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundBow/SoulboundArrow";

		protected override Color LightColor => new Color(1f, 0.5f, 1f, 1f);
		public override void SetDefaults()
		{
			base.SetDefaults();
			//Projectile.minion = true; //Bandaid fix? //TODO 1.4
			Projectile.DamageType = DamageClass.Summon;
		}
		public override float GetRotation()
		{
			return (float)Math.PI / 2 + Projectile.velocity.ToRotation();
		}
	}
	public class SoulboundArsenalSwordProjectile : SoulboundArsenalProjectile
	{

		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundSword/SoulboundSword";
		protected override Color LightColor => new Color(0.75f, 0f, 1f, 1f);
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.penetrate = 3;
		}
		public override float GetRotation()
		{
			return (float)Math.PI / 4 + Projectile.velocity.ToRotation();
		}
	}

	public abstract class SoulboundArsenalBaseMinion : CoordinatedWeaponHoldingSquire
	{
		public override int BuffId => BuffType<SoulboundArsenalMinionBuff>();
		protected override int ItemType => ItemType<SoulboundArsenalMinionItem>();
		protected override int AttackFrames => 20;
		protected override string WingTexturePath => null;

		protected override float IdleDistanceMulitplier => 3;

		public override string Texture => "Terraria/Images/Item_0";

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		public override int AttackSequenceLength => 4;

		protected override bool travelRangeCanBeModified => false;

		protected override float projectileVelocity => 18;

		public override float ComputeIdleSpeed() => 13;

		public override float ComputeTargetedSpeed() => 13;

		public override float MaxDistanceFromPlayer() => 48;

		protected override bool IsMyTurn() => usingSpecial || base.IsMyTurn();

		protected override int SpecialDuration => 4 * 60;
		protected override int SpecialCooldown => 10 * 60;

		public override bool PreDraw(ref Color lightColor)
		{
			if (!IsAttacking())
			{
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

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
		}

	}

	public class SoulboundArsenalBowMinion : SoulboundArsenalBaseMinion
	{
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/SoulboundArsenal/SoulboundBow";

		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;


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
			if (!IsAttacking())
			{
				weaponAngle = Projectile.velocity.X * -Projectile.spriteDirection * 0.05f;
			}
			return base.PreDraw(ref lightColor);
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				SoundEngine.PlaySound(SoundID.Item102, Projectile.position);
				if (Main.myPlayer == Player.whoAmI)
				{
					int type = ProjectileType<SoulboundArsenalArrow>();
					Vector2 angleVector = UnitVectorFromWeaponAngle();
					angleVector *= ModifiedProjectileVelocity();
					float[] amplitudes = { 16, 16, 0 };
					float[] phases = { (float)(3 * Math.PI / 2), (float)(Math.PI / 2), 0 };
					for (int i = 0; i < 2; i++)
					{
						Projectile.NewProjectile(
							Projectile.GetSource_FromThis(),
							Projectile.Center,
							angleVector,
							type,
							Projectile.damage,
							Projectile.knockBack,
							Main.myPlayer,
							amplitudes[i],
							phases[i]);
					}
				}
			}
		}

		protected override float WeaponDistanceFromCenter() => 1;
	}

	public class SoulboundArsenalMinion : SoulboundArsenalBaseMinion
	{
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/SoulboundArsenal/SoulboundSword";

		protected override WeaponAimMode aimMode => usingSpecial ? WeaponAimMode.TOWARDS_MOUSE : WeaponAimMode.FIXED;

		public override bool IsBoss => true;


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
		}

		public override Vector2 IdleBehavior()
		{
			if (Main.myPlayer == Player.whoAmI && Player.ownedProjectileCounts[ProjectileType<SoulboundArsenalBowMinion>()] == 0 && IsPrimaryFrame)
			{
				Projectile p = Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.position,
					Projectile.velocity,
					ProjectileType<SoulboundArsenalBowMinion>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer,
					Projectile.identity);
				p.originalDamage = Projectile.originalDamage;
			}
			Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.5f);
			return base.IdleBehavior();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!IsAttacking())
			{
				weaponAngle = -9 * (float)Math.PI / 16 +
					Projectile.velocity.X * -Projectile.spriteDirection * 0.01f;
			}
			return base.PreDraw(ref lightColor);
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				SoundEngine.PlaySound(SoundID.Item71, Projectile.position);
				if (Main.myPlayer == Player.whoAmI)
				{
					Vector2 vector2Mouse = Main.MouseWorld - Projectile.Center;
					vector2Mouse.Normalize();
					vector2Mouse *= ModifiedProjectileVelocity();
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(), 
						Projectile.Center,
						vector2Mouse,
						ProjectileType<SoulboundArsenalSwordProjectile>(),
						Projectile.damage,
						Projectile.knockBack,
						Main.myPlayer,
						8);
				}
			}
		}
		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.SpecialTargetedMovement(vectorToTargetPosition);
			int projType = ProjectileType<SoulboundArsenalLaser>();
			Vector2 offset = UnitVectorFromWeaponAngle();
			if(specialFrame % 10 == 0)
			{
				SoundEngine.PlaySound(SoundID.Item13, Projectile.Center);
			}
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == Player.whoAmI && p.type == projType)
				{
					p.ai[0] = offset.ToRotation();
					p.Center = Projectile.Center + offset * 48;
					p.velocity = Vector2.Zero;
					break;
				}
			}
		}

		public override void OnStopUsingSpecial()
		{
			int projType = ProjectileType<SoulboundArsenalLaser>();
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == Player.whoAmI && p.type == projType)
				{
					p.Kill();
					break;
				}
			}
		}

		public override void OnStartUsingSpecial()
		{
			if(Player.whoAmI == Main.myPlayer)
			{
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center, 
					Vector2.Zero, 
					ProjectileType<SoulboundArsenalLaser>(), 
					3 * Projectile.damage, 
					Projectile.knockBack, 
					Player.whoAmI,
					ai0: UnitVectorFromWeaponAngle().ToRotation());
			}
		}

		protected override float WeaponDistanceFromCenter() => usingSpecial ? 1 : 30;

		protected override int WeaponHitboxEnd() => 45;
	}
}
