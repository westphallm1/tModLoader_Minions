using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.StardustSquire
{
	public class StardustSquireMinionBuff : MinionBuff
	{
		public StardustSquireMinionBuff() : base(ProjectileType<StardustSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Stardust Squire");
			Description.SetDefault("A stardust squire will follow your orders!");
		}
	}

	public class StardustSquireMinionItem : SquireMinionItem<StardustSquireMinionBuff, StardustSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Stardust");
			Tooltip.SetDefault("Summons a squire\nA stardust squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
			item.damage = 92;
			item.value = Item.sellPrice(0, 10, 0, 0);
			item.rare = ItemRarityID.Red;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.FragmentStardust, 18);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}


	public abstract class StardustSquireSubProjectile : TransientMinion
	{
		private Vector2 initialVelocity = Vector2.Zero;
		private float maxSpeed = default;
		protected float inertia = 8;
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 100);
			Texture2D texture = Main.projectileTexture[projectile.type];

			Rectangle bounds = new Rectangle(0, projectile.frame * texture.Height / Main.projFrames[projectile.type], projectile.width, projectile.height);
			Vector2 origin = new Vector2(projectile.width / 2, projectile.height / 2);

			SpriteEffects effects = 0;
			if (projectile.velocity.X < 0)
			{
				effects = SpriteEffects.FlipVertically;
			}
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
				bounds, translucentColor, projectile.rotation,
				origin, 1, effects, 0);
			return false;
		}

		private void Move(Vector2 vector2Target)
		{
			vector2Target.SafeNormalize();
			vector2Target *= maxSpeed;
			projectile.velocity = (projectile.velocity * (inertia - 1) + vector2Target) / inertia;
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
			Lighting.AddLight(projectile.Center, Color.LightBlue.ToVector3());
			projectile.rotation = projectile.velocity.ToRotation();
			if (initialVelocity == Vector2.Zero)
			{
				initialVelocity = projectile.velocity;
				maxSpeed = projectile.velocity.Length();
			}
			return initialVelocity;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return vectorToTarget == null;
		}

		public override void Kill(int timeLeft)
		{
			for (float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 12)
			{
				Vector2 velocity = 1.5f * new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
				Dust.NewDust(projectile.Center, 1, 1, DustType<MovingWaypointDust>(), velocity.X, velocity.Y, newColor: Color.LightBlue, Scale: 1f);
				velocity *= 2;
				Dust.NewDust(projectile.Center, 1, 1, DustType<MovingWaypointDust>(), velocity.X, velocity.Y, newColor: Color.LightBlue, Scale: 1f);
			}
		}
	}

	public class StardustBeastProjectile : StardustSquireSubProjectile
	{

		private bool canTarget = true;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			frameSpeed = 5;
			projectile.timeLeft = 180;
			projectile.penetrate = 3;
			projectile.friendly = true;
			projectile.width = 48;
			projectile.height = 24;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			canTarget = false;
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 offset = projectile.velocity;
			offset.Normalize();
			offset *= 24;
			for (int i = 0; i < 5; i++)
			{
				Vector2 velocity = -projectile.velocity / 2 + 4 * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat());
				Dust.NewDust(projectile.Center + offset, 1, 1, DustType<MovingWaypointDust>(), velocity.X, velocity.Y, newColor: Color.LightBlue, Scale: 0.9f);
			}
			return base.IdleBehavior();
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (vectorToTargetPosition.Length() < 8)
			{
				canTarget = false;
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override Vector2? FindTarget()
		{
			if (canTarget && ClosestEnemyInRange(300f, projectile.position, maxRangeFromPlayer: false) is Vector2 closest)
			{
				return closest - projectile.position;
			}
			return null;
		}
	}

	public class StarFistProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.timeLeft = 3;
			projectile.penetrate = 3;
			projectile.friendly = true;
			projectile.width = 22;
			projectile.height = 32;
		}
	}

	public class StardustGuardianProjectile : StardustSquireSubProjectile
	{

		private static Random random = new Random();

		private bool hasFoundTarget = false;
		public override void SetStaticDefaults()
		{
			Main.projFrames[projectile.type] = 3;
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.timeLeft = 60;
			projectile.penetrate = -1;
			projectile.friendly = true;
			projectile.width = 22;
			projectile.height = 32;
			inertia = 16;
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 vector2Idle = base.IdleBehavior();
			projectile.rotation = projectile.velocity.X >= 0 ? 0 : (float)Math.PI;
			projectile.spriteDirection = projectile.velocity.X >= 0 ? 1 : -1;
			return vector2Idle;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return base.OnTileCollide(oldVelocity) && !hasFoundTarget || framesSinceHadTarget > 60;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (!(vectorToTarget is Vector2 target) || target.Length() > 48)
			{
				return;
			}
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 100);
			Texture2D texture = Main.projectileTexture[ProjectileType<StarFistProjectile>()];
			for (int i = 0; i < 3; i++)
			{
				Vector2 offset = projectile.velocity;
				offset.Normalize();
				offset *= random.Next(12, 18);
				float rotation = projectile.rotation + (float)(Math.PI / 6 - random.NextDouble() * Math.PI / 3);
				spriteBatch.Draw(texture, projectile.Center + offset - Main.screenPosition,
					texture.Bounds, translucentColor, rotation,
					texture.Bounds.Center.ToVector2(), 1, 0, 0);
			}
			base.PostDraw(spriteBatch, lightColor);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			base.OnHitNPC(target, damage, knockback, crit);
		}

		public override Vector2? FindTarget()
		{
			if (ClosestEnemyInRange(450f, projectile.position, maxRangeFromPlayer: false) is Vector2 closest)
			{
				if (!hasFoundTarget)
				{
					hasFoundTarget = true;
					projectile.timeLeft += 180; // give a bit of extra attack time if we actually find anything
				}
				return closest - projectile.position;
			}
			return null;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, 1);
		}
	}

	public class StardustSquireMinion : WeaponHoldingSquire<StardustSquireMinionBuff>
	{
		protected override int AttackFrames => 30;
		protected override string WingTexturePath => null;
		protected override string WeaponTexturePath => "Terraria/Item_" + ItemID.StardustDragonStaff;

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.DIAGONAL;

		protected override Vector2 WingOffset => new Vector2(-4, 0);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected float projectileVelocity = 14;
		private int attackSequence = 0; // kinda replicate CoordinatedWeaponHoldingSquire but not quire
		public StardustSquireMinion() : base(ItemType<StardustSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ancient Cobalt Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 32;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{

			int projType = ProjectileType<StardustGuardianProjectile>();
			if (player.ownedProjectileCounts[projType] == 0)
			{
				Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 100);
				Texture2D texture = Main.projectileTexture[projType];

				Rectangle bounds = new Rectangle(0, projectile.frame * texture.Height / Main.projFrames[projType], 22, 36);
				Vector2 origin = new Vector2(11, 18);

				Vector2 guardianOffset = new Vector2(-16, -8);
				guardianOffset.X *= projectile.spriteDirection;
				SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
				spriteBatch.Draw(texture, projectile.Center + guardianOffset - Main.screenPosition,
					bounds, translucentColor, projectile.rotation,
					origin, 1, effects, 0);
			}
			return base.PreDraw(spriteBatch, lightColor);
		}


		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}


		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				Vector2 angleVector = UnitVectorFromWeaponAngle();
				if ((attackSequence++ * AttackFrames) % 180 < AttackFrames &&
					player.ownedProjectileCounts[ProjectileType<StardustGuardianProjectile>()] == 0)
				{
					angleVector *= projectileVelocity * 0.75f;
					Projectile.NewProjectile(projectile.Center,
						angleVector,
						ProjectileType<StardustGuardianProjectile>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}
				else
				{
					angleVector *= projectileVelocity;
					Projectile.NewProjectile(projectile.Center,
						angleVector,
						ProjectileType<StardustBeastProjectile>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}
			}
		}

		protected override float WeaponDistanceFromCenter() => 12;

		public override float ComputeIdleSpeed() => 16;

		public override float ComputeTargetedSpeed() => 16;

		public override float MaxDistanceFromPlayer() => 60;
	}
}
