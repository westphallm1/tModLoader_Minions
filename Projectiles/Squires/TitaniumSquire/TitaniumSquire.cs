using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.TitaniumSquire
{
	public class TitaniumSquireMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<TitaniumSquireMinion>() };
	}

	public class TitaniumSquireMinionItem : SquireMinionItem<TitaniumSquireMinionBuff, TitaniumSquireMinion>
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 8f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 54;
			Item.value = Item.buyPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.TitaniumBar, 14).AddTile(TileID.MythrilAnvil).Register();
		}
	}

	public class TitaniumDroneDamageHitbox : ModProjectile
	{
		public override string Texture => "Terraria/Images/Item_0";
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
		}
	}

	public class TitaniumSquireDrone : SquireAccessoryMinion
	{
		protected override bool IsEquipped(SquireModPlayer player) => player.HasSquire() && 
			player.GetSquire().type == ProjectileType<TitaniumSquireMinion>();
		private static int AnimationFrames = 80;

		private int attackRate => (int)Math.Max(15f, 30f * Player.GetModPlayer<SquireModPlayer>().FullSquireAttackSpeedModifier);
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 18;
			Projectile.height = 18;
			FrameSpeed = 10;
		}

		public override Vector2 IdleBehavior()
		{
			int angleFrame = animationFrame % AnimationFrames;
			float angle = 2 * (float)(Math.PI * angleFrame) / AnimationFrames;
			float radius = 36;
			Vector2 angleVector = radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			SquireModPlayer modPlayer = Player.GetModPlayer<SquireModPlayer>();
			if(modPlayer.HasSquire())
			{
				Projectile.spriteDirection = modPlayer.GetSquire().spriteDirection;
			}
			// offset downward vertically a bit
			// the scale messes with the positioning in some way
			return base.IdleBehavior() + angleVector + new Vector2(0, 8);
		}
		public override Vector2? FindTarget()
		{
			if (animationFrame % attackRate == 0 && SquireAttacking() &&
				SelectedEnemyInRange(180, maxRangeFromPlayer: false) is Vector2 target)
			{
				return target - Projectile.Center;
			}
			return null;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			for(int i = 0; i < 3; i++)
			{
				int dustId = Dust.NewDust(Projectile.position, 20, 20, 160);
				ColorDust(dustId);
			}
		}

		public override void Kill(int timeLeft)
		{
			for(int i = 0; i < 3; i++)
			{
				int dustId = Dust.NewDust(Projectile.position, 20, 20, 160);
				ColorDust(dustId);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.IdleMovement(VectorToIdle);
			if (animationFrame % attackRate == 0 )
			{
				if(Player.whoAmI == Main.myPlayer)
				{
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center + vectorToTargetPosition,
						Vector2.Zero,
						ProjectileType<TitaniumDroneDamageHitbox>(),
						Projectile.damage,
						0,
						Player.whoAmI);
				}
				Vector2 targetVector = vectorToTargetPosition;
				Vector2 stepVector = targetVector;
				stepVector.Normalize();

				for(int i = 12; i < targetVector.Length(); i++)
				{
					Vector2 posVector = Projectile.Center + stepVector * i;
					int dustId = Dust.NewDust(posVector, 1, 1, 160);
					ColorDust(dustId);
					Main.dust[dustId].scale = Main.rand.NextFloat(0.9f, 1.3f);
					Main.dust[dustId].velocity *= 0.2f;
				}
				SoundEngine.PlaySound(SoundID.Item92, Projectile.Center);
			}
		}

		private void ColorDust(int dustId)
		{
			int dustColorIdx = Main.rand.Next(4);
			if (dustColorIdx == 0)
			{
				Main.dust[dustId].color = Color.LimeGreen;
			}
			else if (dustColorIdx == 1)
			{
				Main.dust[dustId].color = Color.Purple;
			} else
			{
				Main.dust[dustId].color = Color.LightSteelBlue;
			}
		}
	}

	public class TitaniumSquireMinion : WeaponHoldingSquire
	{
		public override int BuffId => BuffType<TitaniumSquireMinionBuff>();
		protected override int ItemType => ItemType<TitaniumSquireMinionItem>();
		protected override int AttackFrames => 38;

		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";

		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/TitaniumSquire/TitaniumSquireSpear";

		protected override Vector2 WingOffset => new Vector2(-6, 6);

		protected override float knockbackSelf => 5;
		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 6);

		protected override int SpecialDuration => 4 * 60;
		protected override int SpecialCooldown => 10 * 60;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 32;
		}

		protected override float WeaponDistanceFromCenter()
		{
			//All of this is based on the weapon sprite and AttackFrames above.
			int reachFrames = AttackFrames / 2; //A spear should spend half the AttackFrames extending, and half retracting by default.
			int spearLength = WeaponTexture.Width(); //A decent aproximation of how long the spear is.
			int spearStart = (spearLength / 3); //Two thirds of the spear starts behind by default.
			float spearSpeed = spearLength / reachFrames; //A calculation of how quick the spear should be moving.
			if (attackFrame <= reachFrames)
			{
				return spearSpeed * attackFrame - spearStart;
			}
			else
			{
				return (spearSpeed * reachFrames - spearStart) - spearSpeed * (attackFrame - reachFrames);
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
					ProjectileType<TitaniumSquireDrone>(),
					Projectile.damage,
					0,
					Player.whoAmI);
			}
		}

		public override void OnStopUsingSpecial()
		{
			int projType = ProjectileType<TitaniumSquireDrone>();
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.owner == Player.whoAmI && p.type == projType)
				{
					p.Kill();
					break;
				}
			}
		}

		protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() + 35;

		protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 45;

		public override float MaxDistanceFromPlayer() => 290;

		public override float ComputeTargetedSpeed() => 11;

		public override float ComputeIdleSpeed() => 11;
	}
}
