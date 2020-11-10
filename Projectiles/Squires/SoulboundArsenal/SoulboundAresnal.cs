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

namespace AmuletOfManyMinions.Projectiles.Squires.SoulboundArsenal
{
	public class SoulboundArsenalMinionBuff : MinionBuff
	{
		public SoulboundArsenalMinionBuff() : base(ProjectileType<SoulboundArsenalMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Soulbound Arsenal");
			Description.SetDefault("A soulbound bow and sword will follow your orders!");
		}
	}

	public class SoulboundArsenalMinionItem : SquireMinionItem<SoulboundArsenalMinionBuff, SoulboundArsenalMinion>
	{

		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundArsenal/SoulboundArsenalItem";

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Soulbound Arsenal");
			Tooltip.SetDefault("Summons a squire\nA soulbound bow and sword will fight for you!\nClick and hold to guide their attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
			item.damage = 70;
			item.value = Item.sellPrice(0, 8, 0, 0);
			item.rare = ItemRarityID.Lime;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.BrokenHeroSword, 1);
			recipe.AddIngredient(ItemType<SoulboundSwordMinionItem>(), 1);
			recipe.AddIngredient(ItemType<SoulboundBowMinionItem>(), 1);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
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
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.timeLeft = 180;
			projectile.penetrate = 1;
			projectile.friendly = true;
			projectile.width = 8;
			projectile.height = 8;
		}

		public abstract float GetRotation();

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = Main.projectileTexture[projectile.type];

			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
				texture.Bounds, translucentColor, projectile.rotation,
				texture.Bounds.Center.ToVector2(), 1, SpriteEffects.None, 0);
			return false;
		}
		public override void AI()
		{
			if (expectedPosition == Vector2.Zero)
			{
				expectedPosition = projectile.position;
				projectile.rotation = GetRotation();
			}
			expectedPosition += projectile.velocity;
			Vector2 velocityTangent = new Vector2(projectile.velocity.Y, -projectile.velocity.X);
			velocityTangent.Normalize();
			velocityTangent *= projectile.ai[0] * (float)Math.Sin(projectile.ai[1] + 16 * Math.PI * (projectile.timeLeft) / TimeLeft);
			projectile.position = expectedPosition + velocityTangent;
			Lighting.AddLight(projectile.Center, LightColor.ToVector3() * 0.5f);
			Dust.NewDust(projectile.Center, 1, 1, DustType<MinionWaypointDust>(), newColor: LightColor, Scale: 1.2f);
		}

		public override void Kill(int timeLeft)
		{
			for (float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 12)
			{
				Vector2 velocity = 1.5f * new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
				Dust.NewDust(projectile.Center, 1, 1, DustType<MovingWaypointDust>(), velocity.X, velocity.Y, newColor: LightColor, Scale: 1f);
			}
		}

	}
	public class SoulboundArsenalArrow : SoulboundArsenalProjectile
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundBow/SoulboundArrow";

		protected override Color LightColor => Color.LightPink;

		public override float GetRotation()
		{
			return (float)Math.PI / 2 + projectile.velocity.ToRotation();
		}
	}
	public class SoulboundArsenalSwordProjectile : SoulboundArsenalProjectile
	{

		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundSword/SoulboundSword";
		protected override Color LightColor => Color.Lavender;
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.penetrate = 3;
		}
		public override float GetRotation()
		{
			return (float)Math.PI / 4 + projectile.velocity.ToRotation();
		}
	}

	public abstract class SoulboundArsenalBaseMinion : CoordinatedWeaponHoldingSquire<SoulboundArsenalMinionBuff>
	{
		protected SoulboundArsenalBaseMinion(int itemID) : base(itemID) { }
		protected override int AttackFrames => 20;
		protected override string WingTexturePath => null;

		protected override float IdleDistanceMulitplier => 3;

		public override string Texture => "Terraria/Item_0";

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		public override int AttackSequenceLength => 4;

		protected int projectileVelocity = 18;

		public override float ComputeIdleSpeed() => 13;

		public override float ComputeTargetedSpeed() => 13;

		public override float MaxDistanceFromPlayer() => 48;
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (!IsAttacking())
			{
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

	}

	public class SoulboundArsenalBowMinion : SoulboundArsenalBaseMinion
	{
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/SoulboundArsenal/SoulboundBow";

		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

		public SoulboundArsenalBowMinion() : base(ItemType<SoulboundArsenalMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 22;
			projectile.height = 32;
			projectile.tileCollide = false;
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(projectile.Center, Color.LightPink.ToVector3() * 0.5f);
			return base.IdleBehavior();
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (!IsAttacking())
			{
				weaponAngle = projectile.velocity.X * -projectile.spriteDirection * 0.05f;
			}
			return base.PreDraw(spriteBatch, lightColor);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				Main.PlaySound(new LegacySoundStyle(2, 102), projectile.position);
				if (Main.myPlayer == player.whoAmI)
				{
					int type = ProjectileType<SoulboundArsenalArrow>();
					Vector2 angleVector = UnitVectorFromWeaponAngle();
					angleVector *= projectileVelocity;
					float[] amplitudes = { 16, 16, 0 };
					float[] phases = { (float)(3 * Math.PI / 2), (float)(Math.PI / 2), 0 };
					for (int i = 0; i < 2; i++)
					{
						Projectile.NewProjectile(
							projectile.Center,
							angleVector,
							type,
							projectile.damage,
							projectile.knockBack,
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

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		public override bool IsBoss => true;

		public SoulboundArsenalMinion() : base(ItemType<SoulboundArsenalMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Soulbound Arsenal");
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
			if (Main.myPlayer == player.whoAmI && player.ownedProjectileCounts[ProjectileType<SoulboundArsenalBowMinion>()] == 0)
			{
				Projectile.NewProjectile(
					projectile.position,
					projectile.velocity,
					ProjectileType<SoulboundArsenalBowMinion>(),
					projectile.damage,
					projectile.knockBack,
					Main.myPlayer,
					projectile.identity);
			}
			Lighting.AddLight(projectile.Center, Color.Purple.ToVector3() * 0.5f);
			return base.IdleBehavior();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (!IsAttacking())
			{
				weaponAngle = -9 * (float)Math.PI / 16 +
					projectile.velocity.X * -projectile.spriteDirection * 0.01f;
			}
			return base.PreDraw(spriteBatch, lightColor);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				Main.PlaySound(new LegacySoundStyle(2, 71), projectile.position);
				if (Main.myPlayer == player.whoAmI)
				{
					Vector2 vector2Mouse = Main.MouseWorld - projectile.Center;
					vector2Mouse.Normalize();
					vector2Mouse *= projectileVelocity;
					Projectile.NewProjectile(projectile.Center,
						vector2Mouse,
						ProjectileType<SoulboundArsenalSwordProjectile>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer,
						8);
				}
			}
		}

		protected override float WeaponDistanceFromCenter() => 30;

		protected override int WeaponHitboxEnd() => 45;
	}
}
