using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.PricklyPear
{
	public class PricklyPearMinionBuff : MinionBuff
	{
		public PricklyPearMinionBuff() : base(ProjectileType<PricklyPearMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Prickly Pear Hedgehog");
			Description.SetDefault("A prickly pear pal will fight for you!");
		}
	}

	public class PricklyPearMinionItem : MinionItem<PricklyPearMinionBuff, PricklyPearMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Prickly Pear Staff");
			Tooltip.SetDefault("Summons a prickly pear hedgehog to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 10;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.buyPrice(0, 0, 5, 0);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.PinkPricklyPear, 1).AddIngredient(ItemID.Cactus, 25).AddIngredient(ItemID.Amber, 3).AddTile(TileID.Anvils).Register();
		}
	}
	public class PricklyPearSeedProjectile : ModProjectile
	{
		const int TIME_TO_LIVE = 90;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = TIME_TO_LIVE;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			if (TIME_TO_LIVE - Projectile.timeLeft > 6)
			{
				Projectile.velocity.Y += 0.5f;
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = false;
			return true;
		}

		public override void Kill(int timeLeft)
		{
			// only spawn 1ish cactus per volley
			// this can spawn cacti upon hitting walls/ceilings/enemies , but that's ok
			if (Projectile.owner == Main.myPlayer && Main.rand.Next(3) == 0)
			{
				Projectile.NewProjectile(
					Projectile.GetProjectileSource_FromThis(),
					Projectile.position,
					Vector2.Zero,
					ProjectileType<PricklyPearCactusProjectile>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner);

			}
		}
	}

	public class PricklyPearCactusProjectile : ModProjectile
	{
		const int TIME_TO_LIVE = 180;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 32;
			Projectile.timeLeft = TIME_TO_LIVE;
			Projectile.tileCollide = true;
			Projectile.penetrate = 9;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
			// don't instakill an an enemy that falls onto a cactus
			Projectile.localNPCHitCooldown = 30;
			DrawOriginOffsetY = 2;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.5f;
			// hack to play the despawn animation after running out of penetrates
			if (Projectile.penetrate == 1 && Projectile.friendly)
			{
				Projectile.friendly = false;
				Projectile.timeLeft = 20;
			}
			if (Projectile.timeLeft > 20 && Projectile.frame < 3 && Projectile.frameCounter++ >= 5)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
			}
			else if (Projectile.timeLeft <= 20 && Projectile.frame > 0 && Projectile.frameCounter++ >= 5)
			{
				Projectile.frameCounter = 0;
				Projectile.frame--;
			}
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = false;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}
	}


	public class PricklyPearMinion : SimpleGroundBasedMinion
	{
		internal override int BuffId => BuffType<PricklyPearMinionBuff>();
		int lastFiredFrame = 0;
		int fireRate = 90;
		// don't get too close
		int preferredDistanceFromTarget = 96;
		float[] seedAngles = { MathHelper.Pi / 6, MathHelper.PiOver2, 5 * MathHelper.Pi / 6 };
		private Dictionary<GroundAnimationState, (int, int?)> frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
		{
			[GroundAnimationState.FLYING] = (6, 10),
			[GroundAnimationState.JUMPING] = (0, 0),
			[GroundAnimationState.STANDING] = (1, 1),
			[GroundAnimationState.WALKING] = (0, 6),
		};

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("PricklyPear");
			Main.projFrames[Projectile.type] = 10;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 26;
			DrawOffsetX = -2;
			DrawOriginOffsetY = -6;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -3 * Projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = 7;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			// only change speed if the target is a decent distance away
			if (Math.Abs(vector.X) < 4 && targetNPCIndex is int idx && Math.Abs(Main.npc[idx].velocity.X) < 7)
			{
				Projectile.velocity.X = Main.npc[idx].velocity.X;
			}
			else
			{
				Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
		}

		private void FireSeeds()
		{
			int seedVelocity = 7;
			lastFiredFrame = animationFrame;
			SoundEngine.PlaySound(new LegacySoundStyle(6, 1), Projectile.position);
			if (player.whoAmI == Main.myPlayer)
			{
				foreach (float seedAngle in seedAngles)
				{
					Vector2 velocity = seedVelocity * seedAngle.ToRotationVector2();
					velocity.Y *= -1;
					velocity.X += Projectile.velocity.X;
					Projectile.NewProjectile(
						Projectile.GetProjectileSource_FromThis(),
						Projectile.Center,
						VaryLaunchVelocity(velocity),
						ProjectileType<PricklyPearSeedProjectile>(),
						Projectile.damage,
						Projectile.knockBack,
						player.whoAmI);
				}
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{

			if (Math.Abs(vectorToTargetPosition.X) < 1.5f * preferredDistanceFromTarget &&
				Math.Abs(vectorToTargetPosition.Y) < 2 * preferredDistanceFromTarget &&
				animationFrame - lastFiredFrame >= fireRate)
			{
				FireSeeds();
			}

			if (Math.Abs(vectorToTargetPosition.X) < preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X -= preferredDistanceFromTarget * Math.Sign(vectorToTargetPosition.X);
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override void AfterMoving()
		{
			Projectile.friendly = false;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			if (vectorToTarget is Vector2 target && Math.Abs(target.X) < 1.5 * preferredDistanceFromTarget)
			{
				Projectile.spriteDirection = Math.Sign(target.X);
			}
		}
	}
}
