using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.StarSurfer
{
	public class StarSurferMinionBuff : MinionBuff
	{
		public StarSurferMinionBuff() : base(ProjectileType<StarSurferMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Star Surfer");
			Description.SetDefault("A star surfer will fight for you!");
		}
	}

	public class StarSurferMinionItem : MinionItem<StarSurferMinionBuff, StarSurferMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Star Surfer Staff");
			Tooltip.SetDefault("Summons a star surfer to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 32;
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 46;
			item.height = 46;
			item.value = Item.buyPrice(0, 6, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}

	}

	/// <summary>
	/// Uses ai[0] for its frame given on spawn
	/// </summary>
	public class StarSurferProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			Main.projFrames[projectile.type] = 3;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.penetrate = 1;
			projectile.maxPenetrate = 1;
			projectile.tileCollide = true;
			projectile.timeLeft = 120;
			projectile.width = 8;
			projectile.height = 8;
			projectile.friendly = true;
			projectile.ignoreWater = true;
		}

		public override void AI()
		{
			base.AI();
			if (projectile.timeLeft < 90) // start falling after so many frames
			{
				projectile.velocity.Y += 0.5f;
			}
			projectile.rotation += (float)Math.PI / 9;
			projectile.frame = ((int)projectile.ai[0]) % 3;
			//Dust.NewDust(projectile.position, projectile.width / 2, projectile.height / 2, DustID.Gold, -projectile.velocity.X, -projectile.velocity.Y);
		}
	}
	public class StarSurferMinion : SurferMinion
	{

		protected override int BuffId => BuffType<StarSurferMinionBuff>();

		protected int projectileFireRate = 120;
		protected int projectileDamage = 30;
		protected int projectileFrameCount = 0;
		protected int projectileVelocity = 18;
		protected int projectileType;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Star Surfer");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			diveBombFrameRateLimit = 30;
			diveBombSpeed = 20;
			diveBombInertia = 10;
			approachSpeed = 15;
			approachInertia = 20;
			bumbleSpriteDirection = -1;
			projectile.width = 26;
			projectile.height = 32;
			projectileType = ProjectileType<StarSurferProjectile>();
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(projectile.position, Color.Yellow.ToVector3());
			return base.IdleBehavior();
		}

		public override Vector2? FindTarget()
		{
			if (FindTargetInTurnOrder(900f, projectile.Center) is Vector2 target)
			{
				projectile.friendly = true;
				return target;
			}
			else
			{
				projectile.friendly = false;
				return null;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if (Main.rand.Next(5) == 0)
			{
				Dust.NewDust(projectile.Center,
					8,
					8, DustType<StarDust>(),
					-projectile.velocity.X,
					-projectile.velocity.Y);
			}
			if (projectileFrameCount++ > projectileFireRate)
			{
				projectileFrameCount = 0;
				if (Main.myPlayer == player.whoAmI)
				{
					vectorToTargetPosition.SafeNormalize();
					vectorToTargetPosition *= projectileVelocity;
					Projectile.NewProjectile(projectile.position, vectorToTargetPosition, projectileType, projectileDamage, 5, Main.myPlayer, ai0: projectile.minionPos);
				}
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
		}
	}
}
