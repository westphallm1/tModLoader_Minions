using AmuletOfManyMinions.Core.Minions.Effects;
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
using static AmuletOfManyMinions.CrossModClient.SummonersShine.CrossModSetup;

namespace AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire
{
	public class PumpkinSquireMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<PumpkinSquireMinion>() };
	}

	public class PumpkinSquireMinionItem : SquireMinionItem<PumpkinSquireMinionBuff, PumpkinSquireMinion>
	{
		public override void ApplyCrossModChanges()
		{
			WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, SummonersShineDefaultSpecialWhitelistType.RANGEDNOINSTASTRIKE);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3.5f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 14;
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

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.damage = (int)(Projectile.damage * 0.9f);
		}
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
		public override int BuffId => BuffType<PumpkinSquireMinionBuff>();
		protected override int ItemType => ItemType<PumpkinSquireMinionItem>();
		protected override int AttackFrames => 40;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/SpookyWings";
		protected override string WeaponTexturePath => null;

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override SoundStyle? attackSound => SoundID.Item153 with { Volume = 0.5f };
		protected override float projectileVelocity => 8;

		protected override bool travelRangeCanBeModified => false;

		protected override int SpecialDuration => 30;

		private int WhipFrames => ModifiedAttackFrames / 2;


		private readonly int whipLength = 128;
		Vector2 whipVector;
		private bool whipTipCrit;

		private bool IsUsingWhip => whipVector != default && attackFrame < WhipFrames;


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

		public override void LoadAssets()
		{
			base.LoadAssets();
			AddTexture(Texture + "Whip");
		}

		public override Vector2 IdleBehavior()
		{
			whipTipCrit = false;
			return base.IdleBehavior();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(!IsUsingWhip || 
				Vector2.DistanceSquared(projHitbox.Center.ToVector2(), targetHitbox.Center.ToVector2()) > 10 * whipLength * whipLength ||
				!Collision.CanHitLine(projHitbox.Center.ToVector2(), 1, 1, targetHitbox.Center.ToVector2(), 1, 1))
			{
				return false;
			}
			targetHitbox.Inflate(16, 16);
			bool anyHits = false;
			Vector2 baseOffset = whipVector;
			baseOffset.Normalize();
			baseOffset *= WeaponDistanceFromCenter();
			baseOffset += Projectile.Center + WeaponCenterOfRotation;
			new WhipDrawer(GetVineFrame, WhipFrames).ApplyWhipSegments(
				baseOffset, baseOffset + whipVector, attackFrame,
				// TODO short circuit somehow
				(midPoint, rotation, bounds) => { 
					anyHits |= targetHitbox.Contains(midPoint.ToPoint()); 
					whipTipCrit |= bounds.X == 40 && targetHitbox.Contains(midPoint.ToPoint()); 
				});
			return anyHits;
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0 && Main.myPlayer == Player.whoAmI)
			{
				whipVector = UnitVectorFromWeaponAngle();
				whipVector *= whipLength;
			}
		}

		protected override void DrawWeapon(Color lightColor)
		{
			if(IsUsingWhip)
			{
				Vector2 baseOffset = whipVector;
				baseOffset.Normalize();
				baseOffset *= WeaponDistanceFromCenter();
				baseOffset += Projectile.Center + WeaponCenterOfRotation;
				new WhipDrawer(GetVineFrame, WhipFrames).DrawWhip(
					ExtraTextures[2].Value, baseOffset, baseOffset + whipVector, attackFrame);
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			// 50% chance to crit when hitting enemy with tip
			if (whipTipCrit && Main.rand.NextBool(5))
			{
				modifiers.SetCrit();
			}
		}

		private Rectangle GetVineFrame(int frameIdx, bool isLast)
		{
			if(isLast)
			{
				return new(40, 0, 20, 20);
			} else
			{
				return new(20 * (frameIdx % 2), 0, 20, 20);
			}
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			int bigPumpkinType = ProjectileType<BigPumpkinBomb>();
			Projectile bigPumpkin = Main.projectile.Where(p =>
				p.active && p.owner == Player.whoAmI && p.type == bigPumpkinType && p.ai[0] == Projectile.whoAmI).FirstOrDefault();
			Vector2 vector2Mouse = UnitVectorFromWeaponAngle();
			if (bigPumpkin == default && Main.myPlayer == Player.whoAmI)
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

		protected override float WeaponDistanceFromCenter() => 8;

		public override float ComputeIdleSpeed() => 9;

		public override float ComputeTargetedSpeed() => 9;

		public override float MaxDistanceFromPlayer() => 50;
	}
}
