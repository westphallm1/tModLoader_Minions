using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using static Terraria.ModLoader.ModContent;
using static AmuletOfManyMinions.CrossModClient.SummonersShine.CrossModSetup;
using AmuletOfManyMinions.Core;

namespace AmuletOfManyMinions.Projectiles.Minions.EclipseHerald
{
	public class EclipseHeraldMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<EclipseHeraldCounterMinion>() };
	}

	public class EclipseHeraldMinionItem : MinionItem<EclipseHeraldMinionBuff, EclipseHeraldCounterMinion>
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
			Item.damage = 55;
			Item.value = Item.buyPrice(0, 20, 0, 0);
			Item.rare = ItemRarityID.Red;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.FragmentSolar, 18).AddTile(TileID.LunarCraftingStation).Register();
		}
	}

	public class EclipseHeraldCounterMinion : CounterMinion
	{

		public override int BuffId => BuffType<EclipseHeraldMinionBuff>();
		protected override int MinionType => ProjectileType<EclipseHeraldMinion>();
	}
	/// <summary>
	/// Uses ai[1] to track animation frames
	/// </summary>
	public class EclipseHeraldMinion : EmpoweredMinion
	{
		public override int BuffId => BuffType<EclipseHeraldMinionBuff>();

		private int framesSinceLastHit;
		private const int AnimationFrames = 120;
		protected override int dustType => DustID.GoldFlame;
		public override int CounterType => ProjectileType<EclipseHeraldCounterMinion>();
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 9;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 66;
			Projectile.height = 60;
			Projectile.tileCollide = false;
			framesSinceLastHit = 0;
			DealsContactDamage = false;
			AttackThroughWalls = true;
			UseBeacon = false;
			FrameSpeed = 5;
		}

		private Color ShadowColor(Color original)
		{
			return new Color(original.R / 2, original.G / 2, original.B / 2);
		}

		private void DrawSuns(Color lightColor)
		{
			Vector2 pos = Projectile.Center;
			pos.Y -= 24;
			pos.X -= 8 * Projectile.spriteDirection;
			float r = (float)(2 * Math.PI * Projectile.ai[1]) / AnimationFrames;
			int index = Math.Min(5, (int)EmpowerCount - 1);
			Rectangle bounds = new Rectangle(0, 64 * index, 64, 64);
			Texture2D texture = TextureAssets.Projectile[ProjectileType<EclipseSphere>()].Value;
			// main
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				bounds.GetOrigin(), 1, 0, 0);
		}


		private void DrawShadows(Color lightColor)
		{
			Vector2 pos = Projectile.Center;
			pos.Y -= 4; // don't know why this offset needs to exist
			Rectangle bounds = new Rectangle(0, 52 * Projectile.frame, 66, 52);
			Color shadowColor = ShadowColor(lightColor);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			// echo 1
			float offset = 2f * (float)Math.Sin(Math.PI * (Projectile.ai[1] % 60) / 30);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition + Vector2.One * offset,
				bounds, shadowColor, Projectile.rotation, bounds.GetOrigin(), 1, effects, 0);
			// echo 2
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition - Vector2.One * offset,
				bounds, shadowColor, Projectile.rotation, bounds.GetOrigin(), 1, effects, 0);

		}

		public override bool PreDraw(ref Color lightColor)
		{
			Projectile.ai[1] = (Projectile.ai[1] + 1) % AnimationFrames;
			DrawSuns(lightColor);
			DrawShadows(lightColor);
			return true;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = Player.Top;
			idlePosition.X += -Player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, Projectile);
			idlePosition.Y += -32;
			if (!Collision.CanHitLine(idlePosition, 1, 1, Player.Center, 1, 1))
			{
				idlePosition.X = Player.Top.X;
				idlePosition.Y = Player.Top.Y - 16;
			}
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			Lighting.AddLight(Projectile.position, Color.White.ToVector3() * 0.5f);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// stay floating behind the player at all times
			IdleMovement(VectorToIdle);
			framesSinceLastHit++;
			if (framesSinceLastHit++ > 60 && TargetNPCIndex is int npcIndex)
			{
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= 8;
				Vector2 pos = Projectile.Center;
				pos.Y -= 24;
				if (Main.myPlayer == Player.whoAmI)
				{
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						pos,
						vectorToTargetPosition,
						ProjectileType<EclipseSphere>(),
						Projectile.damage,
						Projectile.knockBack,
						Main.myPlayer,
						EmpowerCount - 1,
						npcIndex);
				}
				framesSinceLastHit = 0;
			}
		}

		protected override int ComputeDamage()
		{
			return (int)(baseDamage / 2 + (baseDamage / 2) * EmpowerCountWithFalloff());
		}

		private Vector2? GetTargetVector()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, Player.Center, searchDistance / 2) is Vector2 target)
			{
				return target - Projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance, searchDistance / 2) is Vector2 target2)
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
			return 700 + 100 * EmpowerCount;
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
			if (VectorToTarget != null)
			{
				minFrame = 5;
				maxFrame = 9;
			}
			else if (Projectile.velocity.Y < 3)
			{
				minFrame = 0;
				maxFrame = 4;
			}
			else
			{
				minFrame = 4;
				maxFrame = 4;
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if (Math.Abs(Projectile.velocity.X) > 2)
			{
				Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
			}
			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}
	}
}
