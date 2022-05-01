using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.StardustSquire
{
	public class StardustSquireMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<StardustSquireMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Stardust Squire");
			Description.SetDefault("A stardust squire will follow your orders!");
		}
	}

	public class StardustSquireMinionItem : SquireMinionItem<StardustSquireMinionBuff, StardustSquireMinion>
	{
		protected override string SpecialName => "Stardust Constellation";
		protected override string SpecialDescription => 
			"Summons a lingering constellation\n" +
			"to shoot stars at enemies.";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Stardust");
			Tooltip.SetDefault("Summons a squire\nA stardust squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 92;
			Item.value = Item.sellPrice(0, 10, 0, 0);
			Item.rare = ItemRarityID.Red;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.FragmentStardust, 18).AddTile(TileID.LunarCraftingStation).Register();
		}
	}


	public abstract class StardustSquireSubProjectile : TransientMinion
	{
		private Vector2 initialVelocity = Vector2.Zero;
		private float maxSpeed = default;
		protected float inertia = 16;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = false; // hack to undo TransientMinion.SetStaticDefaults
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 100);
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

			Rectangle bounds = new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], Projectile.width, Projectile.height);
			Vector2 origin = new Vector2(Projectile.width / 2, Projectile.height / 2);

			SpriteEffects effects = 0;
			if (Projectile.velocity.X < 0)
			{
				effects = SpriteEffects.FlipVertically;
			}
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				bounds, translucentColor, Projectile.rotation,
				origin, 1, effects, 0);
			return false;
		}

		private void Move(Vector2 vector2Target)
		{
			vector2Target.SafeNormalize();
			vector2Target *= maxSpeed;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vector2Target) / inertia;
			base.TargetedMovement(vector2Target);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			Move(vectorToTargetPosition);
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			Move(vectorToIdlePosition);
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(Projectile.Center, Color.DeepSkyBlue.ToVector3());
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (initialVelocity == Vector2.Zero)
			{
				initialVelocity = Projectile.velocity;
				maxSpeed = Projectile.velocity.Length();
			}
			return initialVelocity;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return vectorToTarget == null;
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
			for (float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 12)
			{
				Vector2 velocity = 1.5f * new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
				Dust.NewDust(Projectile.Center, 1, 1, DustType<MovingWaypointDust>(), velocity.X, velocity.Y, newColor: Color.DeepSkyBlue, Scale: 1f);
				velocity *= 2;
				Dust.NewDust(Projectile.Center, 1, 1, DustType<MovingWaypointDust>(), velocity.X, velocity.Y, newColor: Color.DeepSkyBlue, Scale: 1f);
			}
		}
	}

	public class StardustBeastProjectile : StardustSquireSubProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			frameSpeed = 5;
			Projectile.timeLeft = 180;
			Projectile.penetrate = 3;
			Projectile.friendly = true;
			Projectile.width = 48;
			Projectile.height = 24;
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 offset = Projectile.velocity;
			offset.Normalize();
			offset *= 24;
			for (int i = 0; i < 5; i++)
			{
				Vector2 velocity = -Projectile.velocity / 8 + 4 * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat());
				Dust.NewDust(Projectile.Center + offset, 1, 1, DustType<MovingWaypointDust>(), velocity.X, velocity.Y, newColor: Color.DeepSkyBlue, Scale: 0.9f);
			}
			return base.IdleBehavior();
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override Vector2? FindTarget()
		{
			if (SelectedEnemyInRange(300f, maxRangeFromPlayer: false) is Vector2 closest)
			{
				return closest - Projectile.position;
			}
			return null;
		}
	}

	public class StarFistProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = 3;
			Projectile.penetrate = 3;
			Projectile.friendly = true;
			Projectile.width = 22;
			Projectile.height = 32;
		}
	}

	public class StardustGuardianProjectile : StardustSquireSubProjectile
	{
		private bool hasFoundTarget = false;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 3;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = 60;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.width = 22;
			Projectile.height = 32;
			inertia = 16;
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 vector2Idle = base.IdleBehavior();
			Projectile.rotation = Projectile.velocity.X >= 0 ? 0 : (float)Math.PI;
			Projectile.spriteDirection = Projectile.velocity.X >= 0 ? 1 : -1;
			return vector2Idle;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return base.OnTileCollide(oldVelocity) && (!hasFoundTarget && Projectile.timeLeft < 30) 
				|| framesSinceHadTarget > 60;
		}

		public override void PostDraw(Color lightColor)
		{
			if (!(vectorToTarget is Vector2 target) || target.Length() > 48)
			{
				return;
			}
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 100);
			Texture2D texture = TextureAssets.Projectile[ProjectileType<StarFistProjectile>()].Value;
			for (int i = 0; i < 3; i++)
			{
				Vector2 offset = Projectile.velocity;
				offset.Normalize();
				offset *= Main.rand.Next(12, 18);
				float rotation = Projectile.rotation + (float)(Math.PI / 6 - Main.rand.NextDouble() * Math.PI / 3);
				Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition,
					texture.Bounds, translucentColor, rotation,
					texture.Bounds.Center.ToVector2(), 1, 0, 0);
			}
			base.PostDraw(lightColor);
		}

		public override Vector2? FindTarget()
		{
			if (SelectedEnemyInRange(450f, maxRangeFromPlayer: false) is Vector2 closest)
			{
				if (!hasFoundTarget)
				{
					hasFoundTarget = true;
					Projectile.timeLeft += 180; // give a bit of extra attack time if we actually find anything
				}
				return closest - Projectile.position;
			}
			return null;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, 1);
		}
	}

	public class StardustSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<StardustSquireMinionBuff>();
		protected override int ItemType => ItemType<StardustSquireMinionItem>();
		protected override int AttackFrames => 30;
		protected override string WingTexturePath => null;
		protected override string WeaponTexturePath => "Terraria/Images/Item_" + ItemID.StardustDragonStaff;

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.DIAGONAL;

		protected override Vector2 WingOffset => new Vector2(-4, 0);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);
		protected override LegacySoundStyle attackSound => new LegacySoundStyle(2, 43);

		protected override float projectileVelocity => 14;
		private int attackSequence = 0; // kinda replicate CoordinatedWeaponHoldingSquire but not quire
		protected override bool travelRangeCanBeModified => false;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ancient Cobalt Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 32;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Lighting.AddLight(Projectile.Center, Color.DeepSkyBlue.ToVector3());
			int projType = ProjectileType<StardustGuardianProjectile>();
			if (player.ownedProjectileCounts[projType] == 0)
			{
				Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 100);
				Texture2D texture = TextureAssets.Projectile[projType].Value;

				Rectangle bounds = new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[projType], 22, 36);
				Vector2 origin = new Vector2(11, 18);

				Vector2 guardianOffset = new Vector2(-16, -8);
				guardianOffset.X *= Projectile.spriteDirection;
				SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
				Main.EntitySpriteDraw(texture, Projectile.Center + guardianOffset - Main.screenPosition,
					bounds, translucentColor, Projectile.rotation,
					origin, 1, effects, 0);
			}
			return base.PreDraw(ref lightColor);
		}


		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}


		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				Vector2 angleVector = UnitVectorFromWeaponAngle();
				if ((attackSequence++ * ModifiedAttackFrames) % 180 < ModifiedAttackFrames &&
					player.ownedProjectileCounts[ProjectileType<StardustGuardianProjectile>()] == 0)
				{
					if (Main.myPlayer == player.whoAmI)
					{
						angleVector *= ModifiedProjectileVelocity() * 0.75f;
						Projectile.NewProjectile(
							Projectile.GetSource_FromThis(),
							Projectile.Center,
							angleVector,
							ProjectileType<StardustGuardianProjectile>(),
							Projectile.damage,
							Projectile.knockBack,
							Main.myPlayer);
					}
				}
				else
				{
					if (Main.myPlayer == player.whoAmI)
					{
						angleVector *= ModifiedProjectileVelocity();
						Projectile.NewProjectile(
							Projectile.GetSource_FromThis(),
							Projectile.Center,
							angleVector,
							ProjectileType<StardustBeastProjectile>(),
							Projectile.damage,
							Projectile.knockBack,
							Main.myPlayer);
					}
				}
			}
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
		}

		protected override float WeaponDistanceFromCenter() => 12;

		public override float ComputeIdleSpeed() => 16;

		public override float ComputeTargetedSpeed() => 16;

		public override float MaxDistanceFromPlayer() => 60;

		public override void OnStartUsingSpecial()
		{
			if(player.whoAmI == Main.myPlayer)
			{
				Vector2 target = Main.MouseWorld - Projectile.Center;
				target.SafeNormalize();
				target *= 2 * projectileVelocity;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					target,
					ProjectileType<ConstellationSeed>(),
					Projectile.damage / 2,
					Projectile.knockBack / 2,
					player.whoAmI,
					ai0: Main.MouseWorld.X,
					ai1: Main.MouseWorld.Y);
			}
		}
	}
}
