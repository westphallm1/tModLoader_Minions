using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.CrossModClient.SummonersShine;
using AmuletOfManyMinions.Projectiles.Minions.CrimsonAltar;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static AmuletOfManyMinions.CrossModClient.SummonersShine.CrossModSetup;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CorruptionAltar
{
	public class CorruptionAltarMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<CorruptionAltarCounterMinion>() };
	}

	public class CorruptionAltarMinionItem : MinionItem<CorruptionAltarMinionBuff, CorruptionAltarCounterMinion>
	{
		public override void ApplyCrossModChanges()
		{
			WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, SummonersShineDefaultSpecialWhitelistType.RANGED);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3f;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 14;
			Item.value = Item.sellPrice(0, 0, 70, 0);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.DemoniteBar, 12).AddIngredient(ItemID.ShadowScale, 6).AddTile(TileID.Anvils).Register();
		}
	}

	public abstract class CorruptionAltarBaseEater : CrimsonAltarBaseCrimera
	{
		protected override float searchDistance => 200f;

		protected override int dustType => 14;
		protected override int dustFrequency => 12;

	}
	public class CorruptionAltarBigEater : CorruptionAltarBaseEater
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Minions/CorruptionAltar/CorruptionAltarEater";

		protected override int dustType => 89;
		public override void SetDefaults()
		{
			base.SetDefaults();
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Color maskColor = new Color(96, 248, 2);
			Rectangle bounds = new Rectangle(0, 0,
				texture.Bounds.Width, texture.Bounds.Height / 2);
			Vector2 pos = Projectile.Center;
			float r = Projectile.rotation;
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, maskColor, r, bounds.GetOrigin(), 1.5f, 0, 0);
			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.CursedInferno, 180);
		}
	}

	public class CorruptionAltarEater : CorruptionAltarBaseEater
	{
	}
	public class CorruptionAltarCounterMinion : CounterMinion
	{
		public override int BuffId => BuffType<CorruptionAltarMinionBuff>();
		protected override int MinionType => ProjectileType<CorruptionAltarMinion>();
	}

	public class CorruptionAltarMinion : EmpoweredMinion
	{
		public override int BuffId => BuffType<CorruptionAltarMinionBuff>();

		private int framesSinceLastHit;
		protected override int dustType => DustID.Blood;
		public override int CounterType => ProjectileType<CorruptionAltarCounterMinion>();
		public override string GlowTexture => null; // have to manually choose when to draw glow

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
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
			Projectile.height = 50;
			Projectile.tileCollide = false;
			framesSinceLastHit = 0;
			DealsContactDamage = false;
			AttackThroughWalls = true;
			UseBeacon = false;
		}



		public override void PostDraw(Color lightColor)
		{
			if (EmpowerCount < 4)
			{
				return;
			}
			Texture2D texture = ExtraTextures[0].Value;
			Rectangle bounds = texture.Bounds;
			Vector2 pos = Projectile.Center;
			float r = Projectile.rotation;
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, Color.White, r, bounds.GetOrigin(), 1, 0, 0);
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = Player.Top;
			idlePosition.X += -Player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, Projectile);
			idlePosition.Y += -8;
			AnimationFrame += 1;
			if (!Collision.CanHitLine(idlePosition, 1, 1, Player.Center, 1, 1))
			{
				idlePosition.X = Player.Top.X;
				idlePosition.Y = Player.Top.Y - 16;
			}
			idlePosition.Y += 4 * (float)Math.Sin(2 * Math.PI * AnimationFrame / 120f);
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.25f);
			return vectorToIdlePosition;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// stay floating behind the player at all times
			IdleMovement(VectorToIdle);
			framesSinceLastHit++;
			int rateOfFire = Math.Max(70, 120 - 10 * EmpowerCount);
			if (framesSinceLastHit++ > rateOfFire)
			{
				int minionsToSpawn = Math.Max(1, Main.rand.Next(1) + (int)EmpowerCount - 1);
				framesSinceLastHit = 0;
				for (int i = 0; i < minionsToSpawn; i++)
				{
					bool summonBig = EmpowerCount >= 4 && Main.rand.NextBool(4);
					int projType = summonBig ? ProjectileType<CorruptionAltarBigEater>() : ProjectileType<CorruptionAltarEater>();
					float rangeSquare = Math.Min(120, vectorToTargetPosition.Length() / 2);
					vectorToTargetPosition.X += Main.rand.NextFloat() * rangeSquare - rangeSquare / 2;
					vectorToTargetPosition.Y += Main.rand.NextFloat() * rangeSquare - rangeSquare / 2;
					float projectileVelocity = summonBig ? 9.5f : 12.5f;
					vectorToTargetPosition.SafeNormalize();
					vectorToTargetPosition *= projectileVelocity;
					Vector2 pos = Projectile.Center;
					framesSinceLastHit = 0;
					if (Main.myPlayer == Player.whoAmI)
					{
						Projectile.NewProjectile(
							Projectile.GetSource_FromThis(),
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
			return (int)(baseDamage + (baseDamage / 8) * EmpowerCountWithFalloff()); // only scale up damage a little bit
		}

		private Vector2? GetTargetVector()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, Player.Center) is Vector2 target)
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
			Projectile.rotation = (float)(Math.PI / 8 * Math.Cos(2 * Math.PI * AnimationFrame / 120f));

			if (Main.rand.NextBool(120))
			{
				for (int i = 0; i < 3; i++)
				{
					Dust.NewDust(Projectile.Center, 16, 16, 14, Main.rand.Next(6) - 3, Main.rand.Next(6) - 3);
				}
			}
		}
	}
}
