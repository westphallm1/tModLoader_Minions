using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.PricklyPear
{
	public class PricklyPearMinionBuff : MinionBuff
	{
		public PricklyPearMinionBuff() : base(ProjectileType<PricklyPearMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
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
			item.damage = 11;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 5, 0);
			item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.PinkPricklyPear, 1);
			recipe.AddIngredient(ItemID.Cactus, 25);
			recipe.AddIngredient(ItemID.Amber, 3);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
	public class PricklyPearSeedProjectile : ModProjectile
	{
		const int TIME_TO_LIVE = 90;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.timeLeft = TIME_TO_LIVE;
			projectile.tileCollide = true;
			projectile.penetrate = 1;
			projectile.friendly = true;
			projectile.usesLocalNPCImmunity = true;
		}

		public override void AI()
		{
			if (TIME_TO_LIVE - projectile.timeLeft > 6) {
				projectile.velocity.Y += 0.5f;
			}
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
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
			if(projectile.owner == Main.myPlayer && Main.rand.Next(3) == 0)
			{
				Projectile.NewProjectile(
					projectile.position, 
					Vector2.Zero, 
					ProjectileType<PricklyPearCactusProjectile>(), 
					projectile.damage, 
					projectile.knockBack, 
					projectile.owner);

			}
		}
	}

	public class PricklyPearCactusProjectile : ModProjectile
	{
		const int TIME_TO_LIVE = 180;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
			Main.projFrames[projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 32;
			projectile.timeLeft = TIME_TO_LIVE;
			projectile.tileCollide = true;
			projectile.penetrate = -1;
			projectile.friendly = true;
			projectile.usesLocalNPCImmunity = true;
			drawOriginOffsetY = 2;
		}

		public override void AI()
		{
			projectile.velocity.Y += 0.5f;
			if(projectile.timeLeft > 20 && projectile.frame < 3 && projectile.frameCounter++ >= 5)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
			} else if (projectile.timeLeft <= 20 && projectile.frame > 0 && projectile.frameCounter++ >= 5)
			{
				projectile.frameCounter = 0;
				projectile.frame--;
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
		protected override int BuffId => BuffType<PricklyPearMinionBuff>();
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
			Main.projFrames[projectile.type] = 10;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 26;
			drawOffsetX = -2;
			drawOriginOffsetY = -6;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{

			if(vector.Y < - 3 * projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = 7;
			if(vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			// only change speed if the target is a decent distance away
			if (Math.Abs(vector.X) < 4 && targetNPCIndex is int idx && Math.Abs(Main.npc[idx].velocity.X) < 7)
			{
				projectile.velocity.X = Main.npc[idx].velocity.X;
			}
			else
			{
				projectile.velocity.X = (projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
		}

		private void FireSeeds()
		{
			int seedVelocity = 7;
			lastFiredFrame = animationFrame;
			if(player.whoAmI == Main.myPlayer)
			{
				foreach(float seedAngle in seedAngles)
				{
					Vector2 velocity = seedVelocity * seedAngle.ToRotationVector2();
					velocity.Y *= -1;
					velocity.X += projectile.velocity.X;
					Projectile.NewProjectile(
						projectile.Center,
						velocity,
						ProjectileType<PricklyPearSeedProjectile>(),
						projectile.damage,
						projectile.knockBack,
						player.whoAmI);
				}
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{

			if(Math.Abs(vectorToTargetPosition.X) < 1.5f * preferredDistanceFromTarget &&
				Math.Abs(vectorToTargetPosition.Y) < 2 * preferredDistanceFromTarget &&
				animationFrame - lastFiredFrame >= fireRate)
			{
				FireSeeds();
			}

			if(Math.Abs(vectorToTargetPosition.X) < preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X -= preferredDistanceFromTarget * Math.Sign(vectorToTargetPosition.X);
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override void AfterMoving()
		{
			projectile.friendly = false;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			if (vectorToTarget is Vector2 target && Math.Abs(target.X) < 1.5 * preferredDistanceFromTarget)
			{
				projectile.spriteDirection = Math.Sign(target.X);
			}
		}
	}
}
