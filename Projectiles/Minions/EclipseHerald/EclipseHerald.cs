using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.EclipseHerald
{
	public class EclipseHeraldMinionBuff : MinionBuff
	{
		public EclipseHeraldMinionBuff() : base(ProjectileType<EclipseHeraldCounterMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Eclipse Herald");
			Description.SetDefault("A herald of the eclipse will fight for you!");
		}
	}

	public class EclipseHeraldMinionItem : MinionItem<EclipseHeraldMinionBuff, EclipseHeraldCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Eclipse Herald Staff");
			Tooltip.SetDefault("Can't come to grips \nWith the total eclipse \nJust a slip of your lips \nand you're gone...");
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

		internal override int BuffId => BuffType<EclipseHeraldMinionBuff>();
		protected override int MinionType => ProjectileType<EclipseHeraldMinion>();
	}
	/// <summary>
	/// Uses ai[1] to track animation frames
	/// </summary>
	public class EclipseHeraldMinion : EmpoweredMinion
	{
		internal override int BuffId => BuffType<EclipseHeraldMinionBuff>();

		private int framesSinceLastHit;
		private const int AnimationFrames = 120;
		protected override int dustType => DustID.GoldFlame;
		protected override int CounterType => ProjectileType<EclipseHeraldCounterMinion>();
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Eclipse Herald");
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
			Projectile.friendly = true;
			attackThroughWalls = true;
			useBeacon = false;
			frameSpeed = 5;
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
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Texture2D texture = TextureAssets.Projectile[ProjectileType<EclipseSphere>()].Value;
			// main
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, 0, 0);
		}


		private void DrawShadows(Color lightColor)
		{
			Vector2 pos = Projectile.Center;
			pos.Y -= 4; // don't know why this offset needs to exist
			Rectangle bounds = new Rectangle(0, 52 * Projectile.frame, 66, 52);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Color shadowColor = ShadowColor(lightColor);
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			// echo 1
			float offset = 2f * (float)Math.Sin(Math.PI * (Projectile.ai[1] % 60) / 30);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition + Vector2.One * offset,
				bounds, shadowColor, Projectile.rotation, origin, 1, effects, 0);
			// echo 2
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition - Vector2.One * offset,
				bounds, shadowColor, Projectile.rotation, origin, 1, effects, 0);

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
			Vector2 idlePosition = player.Top;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, Projectile);
			idlePosition.Y += -32;
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition.X = player.Top.X;
				idlePosition.Y = player.Top.Y - 16;
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
			IdleMovement(vectorToIdle);
			framesSinceLastHit++;
			if (framesSinceLastHit++ > 60 && targetNPCIndex is int npcIndex)
			{
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= 8;
				Vector2 pos = Projectile.Center;
				pos.Y -= 24;
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						Projectile.GetProjectileSource_FromThis(),
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
			return baseDamage / 2 + (baseDamage / 2) * (int)EmpowerCount;
		}

		private Vector2? GetTargetVector()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center, searchDistance / 2) is Vector2 target)
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
			if (vectorToTarget != null)
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
