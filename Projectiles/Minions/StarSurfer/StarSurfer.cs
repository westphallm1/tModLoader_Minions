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
		internal override int[] ProjectileTypes => new int[] { ProjectileType<StarSurferMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
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
			Item.damage = 32;
			Item.knockBack = 3f;
			Item.mana = 10;
			Item.width = 46;
			Item.height = 46;
			Item.value = Item.buyPrice(0, 6, 0, 0);
			Item.rare = ItemRarityID.LightRed;
		}

	}

	/// <summary>
	/// Uses ai[0] for its frame given on spawn
	/// </summary>
	public class StarSurferProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 3;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.penetrate = 1;
			Projectile.maxPenetrate = 1;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 120;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			base.AI();
			if (Projectile.timeLeft < 90) // start falling after so many frames
			{
				Projectile.velocity.Y += 0.5f;
			}
			Projectile.rotation += (float)Math.PI / 9;
			Projectile.frame = ((int)Projectile.ai[0]) % 3;
			//Dust.NewDust(projectile.position, projectile.width / 2, projectile.height / 2, DustID.Gold, -projectile.velocity.X, -projectile.velocity.Y);
		}
	}
	public class StarSurferMinion : SurferMinion
	{

		internal override int BuffId => BuffType<StarSurferMinionBuff>();

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
			Projectile.width = 26;
			Projectile.height = 32;
			projectileType = ProjectileType<StarSurferProjectile>();
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(Projectile.position, Color.Yellow.ToVector3());
			return base.IdleBehavior();
		}

		public override Vector2? FindTarget()
		{
			if (FindTargetInTurnOrder(900f, Projectile.Center) is Vector2 target)
			{
				return target;
			}
			else
			{
				return null;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if (Main.rand.NextBool(5))
			{
				Dust.NewDust(Projectile.Center,
					8,
					8, DustType<StarDust>(),
					-Projectile.velocity.X,
					-Projectile.velocity.Y);
			}
			if (projectileFrameCount++ > projectileFireRate)
			{
				projectileFrameCount = 0;
				if (Main.myPlayer == player.whoAmI)
				{
					vectorToTargetPosition.SafeNormalize();
					vectorToTargetPosition *= projectileVelocity;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, VaryLaunchVelocity(vectorToTargetPosition), projectileType, projectileDamage, 5, Main.myPlayer, ai0: Projectile.minionPos);
				}
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
		}
	}
}
