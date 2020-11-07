using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.Acorn
{
	public class AcornMinionBuff : MinionBuff
	{
		public AcornMinionBuff() : base(ProjectileType<AcornMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Acorn");
			Description.SetDefault("A winged acorn will fight for you!");
		}
	}

	public class AcornMinionItem : MinionItem<AcornMinionBuff, AcornMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Acorn Staff");
			Tooltip.SetDefault("Summons a winged acorn to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 8;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 2, 0);
			item.rare = ItemRarityID.White;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Acorn, 3);
			recipe.AddIngredient(ItemID.Wood, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class AcornBomb : ModProjectile
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
			projectile.friendly = true;
			projectile.tileCollide = true;
			projectile.penetrate = 1;
		}

		public override void AI()
		{
			projectile.velocity.Y += 0.65f;
			projectile.rotation += 0.2f * Math.Sign(projectile.velocity.X);
		}

		public override void Kill(int timeLeft)
		{
			Vector2 direction = -projectile.velocity;
			direction.Normalize();
			for (int i = 0; i < 2; i++)
			{
				Dust.NewDust(projectile.position, 1, 1, DustType<AcornDust>(), -direction.X, -direction.Y, Alpha: 255, Scale: 2);
			}
		}
	}
	public class AcornMinion : SurferMinion<AcornMinionBuff>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Acorn");
			Main.projFrames[projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			drawOffsetX = (projectile.width - 44) / 2;
			attackFrames = 60;
			projectile.type = ProjectileType<AcornMinion>();
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= Main.projFrames[projectile.type])
				{
					projectile.frame = 0;
				}
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int targetAbove = 80;
			Vector2 vectorAbove = vectorToTargetPosition;
			projectile.friendly = false;
			for (int i = 16; i < targetAbove; i++)
			{
				vectorAbove = new Vector2(vectorToTargetPosition.X, vectorToTargetPosition.Y - i);
				if (!Collision.CanHit(projectile.Center, 1, 1, projectile.Center + vectorAbove, 1, 1))
				{
					break;
				}
			}
			if (Main.myPlayer == player.whoAmI && IsMyTurn() && Math.Abs(vectorAbove.X) <= 32)
			{
				Projectile.NewProjectile(
					projectile.Center,
					new Vector2(vectorAbove.X / 8, 2),
					ProjectileType<AcornBomb>(),
					projectile.damage,
					projectile.knockBack,
					player.whoAmI);
			}
			DistanceFromGroup(ref vectorAbove);
			vectorAbove.SafeNormalize();
			vectorAbove *= 8;
			int inertia = 18;
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorAbove) / inertia;
		}
	}
}
