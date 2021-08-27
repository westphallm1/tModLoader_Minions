using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CrimsonAltar
{
	public class CrimsonAltarMinionBuff : MinionBuff
	{
		public CrimsonAltarMinionBuff() : base(ProjectileType<CrimsonAltarCounterMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crimson Cell");
			Description.SetDefault("A crimson cell will fight for you!");
		}
	}

	public class CrimsonAltarMinionItem : MinionItem<CrimsonAltarMinionBuff, CrimsonAltarCounterMinion>
	{

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crimson Cell Staff");
			Tooltip.SetDefault("Summons a crimson cell to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3f;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 15;
			Item.value = Item.sellPrice(0, 0, 70, 0);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.CrimtaneBar, 12).AddIngredient(ItemID.TissueSample, 6).AddTile(TileID.Anvils).Register();
		}
	}

	public abstract class CrimsonAltarBaseCrimera : BumblingTransientMinion
	{
		protected override float inertia => 20;
		protected override float idleSpeed => 10;

		protected override int timeToLive => 120;

		protected override float distanceToBumbleBack => 2000f; // don't bumble back

		protected override float searchDistance => 220f;

		protected virtual int dustType => DustID.Blood;
		protected virtual int dustFrequency => 5;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 2;
		}

		protected override void Move(Vector2 vector2Target, bool isIdle = false)
		{
			base.Move(vector2Target, isIdle);
			Projectile.rotation = Projectile.velocity.ToRotation() + 3 * (float)Math.PI / 2;
			if (Main.rand.Next(dustFrequency) == 0)
			{
				Dust.NewDust(Projectile.Center, 1, 1, dustType, -Projectile.velocity.X / 2, -Projectile.velocity.Y / 2);
			}
		}

		public override void Kill(int timeLeft)
		{
			base.Kill(timeLeft);
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, dustType, Projectile.velocity.X / 2, Projectile.velocity.Y / 2);
			}
		}
	}
	public class CrimsonAltarBigCrimera : CrimsonAltarBaseCrimera
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Minions/CrimsonAltar/CrimsonAltarCrimera";

		protected override int dustType => 87;


		public override void SetDefaults()
		{
			base.SetDefaults();
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Color maskColor = new Color(254, 202, 80);
			Rectangle bounds = new Rectangle(0, 0,
				texture.Bounds.Width, texture.Bounds.Height / 2);
			Vector2 origin = bounds.Center.ToVector2();
			Vector2 pos = Projectile.Center;
			float r = Projectile.rotation;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, maskColor, r,
				origin, 1.5f, 0, 0);
			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Ichor, 90);
		}
	}

	public class CrimsonAltarCrimera : CrimsonAltarBaseCrimera
	{
	}

	public class CrimsonAltarCounterMinion : CounterMinion
	{
		internal override int BuffId => BuffType<CrimsonAltarMinionBuff>();
		protected override int MinionType => ProjectileType<CrimsonAltarMinion>();
	}
	public class CrimsonAltarMinion : EmpoweredMinion
	{
		internal override int BuffId => BuffType<CrimsonAltarMinionBuff>();
		private int framesSinceLastHit;
		public override int CounterType => ProjectileType<CrimsonAltarCounterMinion>();
		protected override int dustType => DustID.Blood;

		public override string GlowTexture => null; // have to manually choose when to draw glow

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crimson Cell");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 4;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}

		public override void LoadAssets()
		{
			AddTexture(Texture + "_Glow");
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.tileCollide = false;
			framesSinceLastHit = 0;
			Projectile.friendly = true;
			attackThroughWalls = true;
			useBeacon = false;
		}



		public override void PostDraw(Color lightColor)
		{
			if (EmpowerCount < 4)
			{
				return;
			}
			Texture2D texture = ExtraTextures[0].Value;
			Rectangle bounds = texture.Bounds;
			Vector2 origin = bounds.Center.ToVector2();
			Vector2 pos = Projectile.Center;
			float r = Projectile.rotation;
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, Color.White, r,
				origin, 1, 0, 0);
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, Projectile);
			idlePosition.Y += -8;
			animationFrame += 1;
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition.X = player.Top.X;
				idlePosition.Y = player.Top.Y - 16;
			}
			idlePosition.Y += 4 * (float)Math.Sin(animationFrame / 32f);
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.25f);
			return vectorToIdlePosition;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// stay floating behind the player at all times
			IdleMovement(vectorToIdle);
			framesSinceLastHit++;
			int rateOfFire = 120;
			if (framesSinceLastHit++ > rateOfFire)
			{
				framesSinceLastHit = 0;
				for (int i = 0; i < EmpowerCount; i++)
				{
					bool summonBig = EmpowerCount >= 4 && Main.rand.Next(4) == 0;
					int projType = summonBig ? ProjectileType<CrimsonAltarBigCrimera>() : ProjectileType<CrimsonAltarCrimera>();
					float rangeSquare = Math.Min(120, vectorToTargetPosition.Length() / 2);
					vectorToTargetPosition.X += Main.rand.NextFloat() * rangeSquare - rangeSquare / 2;
					vectorToTargetPosition.Y += Main.rand.NextFloat() * rangeSquare - rangeSquare / 2;
					int projectileVelocity = summonBig ? 8 : 12;
					vectorToTargetPosition.SafeNormalize();
					vectorToTargetPosition *= projectileVelocity;
					Vector2 pos = Projectile.Center;
					framesSinceLastHit = 0;
					if (Main.myPlayer == player.whoAmI)
					{
						Projectile.NewProjectile(
							Projectile.GetProjectileSource_FromThis(),
							pos,
							VaryLaunchVelocity(vectorToTargetPosition),
							projType,
							Projectile.damage,
							Projectile.knockBack,
							Main.myPlayer);
					}
				}
			}
		}

		protected override int ComputeDamage()
		{
			return baseDamage + (baseDamage / 8) * (int)EmpowerCount; // only scale up damage a little bit
		}

		private Vector2? GetTargetVector()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center) is Vector2 target)
			{
				return target - Projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance) is Vector2 target2)
			{
				return target2 - Projectile.Center;
			}
			else
			{
				return null;
			}
		}
		public override Vector2? FindTarget()
		{
			Vector2? target = GetTargetVector();
			return target;
		}

		protected override float ComputeSearchDistance()
		{
			return 500 + 30 * EmpowerCount;
		}

		protected override float ComputeInertia()
		{
			return 5;
		}

		protected override float ComputeTargetedSpeed()
		{
			return 16;
		}

		protected override float ComputeIdleSpeed()
		{
			return 16;
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 0;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			Projectile.spriteDirection = 1;
			Projectile.frame = Math.Min(4, (int)EmpowerCount) - 1;
			Projectile.rotation += player.direction / 32f;
			if (Main.rand.Next(120) == 0)
			{
				for (int i = 0; i < 3; i++)
				{
					Dust.NewDust(Projectile.Center, 16, 16, DustID.Blood, Main.rand.Next(6) - 3, Main.rand.Next(6) - 3);
				}
			}
		}
	}
}
