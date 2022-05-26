using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire
{
	public class PumpkinSquireMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<PumpkinSquireMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Pumpkin Squire");
			Description.SetDefault("An pumpkin squire will follow your orders!");
		}
	}

	public class PumpkinSquireMinionItem : SquireMinionItem<PumpkinSquireMinionBuff, PumpkinSquireMinion>
	{
		protected override string SpecialName => "Giant Pumpkin";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Pumpkin Crest");
			Tooltip.SetDefault("Summons a squire\nA pumpkin squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3.5f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 17;
			Item.value = Item.sellPrice(0, 0, 1, 0);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Pumpkin, 15).AddRecipeGroup("AmuletOfManyMinions:EvilBars", 12).AddTile(TileID.Anvils).Register();
		}
	}

	public abstract class BasePumpkinBomb : ModProjectile
	{
		protected abstract int TimeToLive { get;  }
		protected abstract int FallAfterFrames { get;  }
		protected int bounces;
		protected bool startFalling;
		protected int dustCount;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = TimeToLive;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			startFalling = false;
		}

		public override void AI()
		{
			Projectile.rotation += MathHelper.Pi / 16 * Math.Sign(Projectile.velocity.X);
			if (Projectile.timeLeft < TimeToLive - FallAfterFrames)
			{
				startFalling = true;
			}
			if (startFalling)
			{
				if(Projectile.velocity.Y < 16)
				{
					Projectile.velocity.Y += 0.5f;
				}
			}
		}

		protected abstract void OnFloorBounce(int bouncesLeft, Vector2 oldVelocity);
		protected abstract void OnWallBounce(int bouncesLeft, Vector2 oldVelocity);

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (oldVelocity.Y > 0 && Projectile.velocity.Y == 0)
			{
				OnFloorBounce(bounces, oldVelocity);
				bounces--;
				SoundEngine.PlaySound(SoundID.Dig with { Pitch = Main.rand.Next(1) }, Projectile.Center);
			}
			if (oldVelocity.Y < 0)
			{
				startFalling = true;
			}
			if (oldVelocity.X != 0 && Projectile.velocity.X == 0)
			{
				OnWallBounce(bounces, oldVelocity);
			}
			return bounces == 0;
		}

		public override void Kill(int timeLeft)
		{
			// don't explode
			SoundEngine.PlaySound(SoundID.NPCDeath1 with { PitchVariance = 0.5f }, Projectile.position);
			Vector2 direction = -Projectile.velocity;
			direction.Normalize();
			for (int i = 0; i < dustCount; i++)
			{
				Dust.NewDust(Projectile.position, 1, 1, DustType<PumpkinDust>(), -direction.X, -direction.Y, Alpha: 255, Scale: 2);
			}
		}

	}


	public abstract class WeakPumpkinBomb : BasePumpkinBomb
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			bounces = 3;
			Projectile.penetrate = 3;
			dustCount = 3;
		}
		protected override int TimeToLive => 120;

		protected override int FallAfterFrames => 15;

		protected override void OnFloorBounce(int bouncesLeft, Vector2 oldVelocity)
		{
			Projectile.velocity.Y = -3 * bouncesLeft;
			// make sure not to collide right away again
			Projectile.position.Y -= 8;
			Projectile.velocity.X *= 0.67f;
		}

		protected override void OnWallBounce(int bouncesLeft, Vector2 oldVelocity)
		{
			Projectile.velocity.X = -Math.Sign(oldVelocity.X) * 1.5f * bouncesLeft;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Projectile.damage = (int)(Projectile.damage * 0.9f);
		}
	}

	public class PumpkinBomb : WeakPumpkinBomb
	{
		// no op
	}

	public class BigPumpkinBomb : BasePumpkinBomb
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			bounces = 12;
			Projectile.penetrate = 20;
			dustCount = 6;
		}
		int spawnFrames = 30;
		protected override int TimeToLive => 360;

		protected override int FallAfterFrames => spawnFrames + 15;

		public override void AI()
		{
			if(Projectile.timeLeft < TimeToLive - spawnFrames)
			{
				Projectile.friendly = true;
				Projectile.tileCollide = true;
				Projectile.ai[0] = -1;
				base.AI();
			} else
			{
				Projectile.friendly = false;
				Projectile.tileCollide = false;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float spawnFrame = Math.Min(spawnFrames, TimeToLive - Projectile.timeLeft);
			float scale = MathHelper.Lerp(0.25f, 1, spawnFrame / spawnFrames);
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				texture.Bounds, lightColor, Projectile.rotation,
				texture.Bounds.Center.ToVector2(), scale, 0, 0);
			return false;
		}

		protected override void OnFloorBounce(int bouncesLeft, Vector2 oldVelocity)
		{
			Projectile.velocity.Y = -Math.Max(bouncesLeft / 2f, 2f);
			// make sure not to collide right away again
			Projectile.position.Y -= 2;
		}

		protected override void OnWallBounce(int bouncesLeft, Vector2 oldVelocity)
		{
			Projectile.velocity.X = -Math.Sign(oldVelocity.X) * Math.Max(1.5f, bouncesLeft / 4f);
		}
	}

	public class PumpkinSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<PumpkinSquireMinionBuff>();
		protected override int ItemType => ItemType<PumpkinSquireMinionItem>();
		protected override int AttackFrames => 30;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/SpookyWings";
		protected override string WeaponTexturePath => null;

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override SoundStyle? attackSound => SoundID.Item19;
		protected override float projectileVelocity => 8;

		protected override bool travelRangeCanBeModified => false;

		protected override int SpecialDuration => 30;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Pumpkin Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 22;
			Projectile.height = 32;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0 && Main.myPlayer == player.whoAmI)
			{
				Vector2 vector2Mouse = UnitVectorFromWeaponAngle();
				vector2Mouse *= 1.5f *  ModifiedProjectileVelocity();
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(), 
					Projectile.Center,
					vector2Mouse,
					ProjectileType<PumpkinBomb>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer);
			}
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			int bigPumpkinType = ProjectileType<BigPumpkinBomb>();
			Projectile bigPumpkin = Main.projectile.Where(p =>
				p.active && p.owner == player.whoAmI && p.type == bigPumpkinType && p.ai[0] == Projectile.whoAmI).FirstOrDefault();
			Vector2 vector2Mouse = UnitVectorFromWeaponAngle();
			if (bigPumpkin == default && Main.myPlayer == player.whoAmI)
			{
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(), 
					Projectile.Center,
					Vector2.Zero,
					bigPumpkinType,
					3 * Projectile.damage / 2,
					Projectile.knockBack,
					Main.myPlayer,
					ai0: Projectile.whoAmI);
			} else if (bigPumpkin != default && specialFrame == SpecialDuration - 1)
			{
				vector2Mouse *= 1.5f * ModifiedProjectileVelocity();
				bigPumpkin.velocity = vector2Mouse;
			} else if(bigPumpkin != default)
			{
				vector2Mouse *= 32;
				bigPumpkin.Center = Projectile.Center + vector2Mouse;
			}
		}

		protected override float WeaponDistanceFromCenter() => 12;

		public override float ComputeIdleSpeed() => 9;

		public override float ComputeTargetedSpeed() => 9;

		public override float MaxDistanceFromPlayer() => 50;
	}
}
