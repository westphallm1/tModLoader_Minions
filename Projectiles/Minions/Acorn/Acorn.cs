using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.CrossModClient.SummonersShine;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static AmuletOfManyMinions.CrossModClient.SummonersShine.CrossModSetup;

namespace AmuletOfManyMinions.Projectiles.Minions.Acorn
{
	public class AcornMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<AcornMinion>() };
	}

	public class AcornMinionItem : MinionItem<AcornMinionBuff, AcornMinion>
	{
		public override void ApplyCrossModChanges()
		{
			WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, SummonersShineDefaultSpecialWhitelistType.RANGEDNOMULTISHOT);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 8;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.buyPrice(0, 0, 2, 0);
			Item.rare = ItemRarityID.White;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Acorn, 3).AddRecipeGroup(RecipeGroupID.Wood, 10).AddTile(TileID.WorkBenches).Register();
		}
	}

	public class AcornBomb : ModProjectile
	{
		const int TIME_TO_LIVE = 90;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = TIME_TO_LIVE;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.65f;
			Projectile.rotation += 0.2f * Math.Sign(Projectile.velocity.X);
		}

		public override void Kill(int timeLeft)
		{
			Vector2 direction = -Projectile.velocity;
			direction.Normalize();
			for (int i = 0; i < 2; i++)
			{
				Dust.NewDust(Projectile.position, 1, 1, DustType<AcornDust>(), -direction.X, -direction.Y, Alpha: 255, Scale: 2);
			}
		}
	}
	public class AcornMinion : HeadCirclingGroupAwareMinion
	{
		int lastFiredFrame = 0;
		public override int BuffId => BuffType<AcornMinionBuff>();
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 4;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			DrawOffsetX = (Projectile.width - 44) / 2;
			attackFrames = 60;
			DealsContactDamage = false;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type])
				{
					Projectile.frame = 0;
				}
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int targetAbove = 80;
			Vector2 vectorAbove = vectorToTargetPosition;
			// only check for exact position once close to target
			if (vectorToTargetPosition.LengthSquared() < 256 * 256)
			{
				for (int i = 16; i < targetAbove; i++)
				{
					vectorAbove = new Vector2(vectorToTargetPosition.X, vectorToTargetPosition.Y - i);
					if (!Collision.CanHit(Projectile.Center, 1, 1, Projectile.Center + vectorAbove, 1, 1))
					{
						break;
					}
				}
			}
			if (Main.myPlayer == Player.whoAmI && IsMyTurn() && AnimationFrame - lastFiredFrame >= attackFrames && Math.Abs(vectorAbove.X) <= 32)
			{
				lastFiredFrame = AnimationFrame;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					new Vector2(vectorAbove.X / 8 + Projectile.velocity.X, 2),
					ProjectileType<AcornBomb>(),
					Projectile.damage,
					Projectile.knockBack,
					Player.whoAmI);
			}
			DistanceFromGroup(ref vectorAbove);
			vectorAbove.SafeNormalize();
			vectorAbove *= 8;
			int inertia = 18;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorAbove) / inertia;
		}
	}
}
