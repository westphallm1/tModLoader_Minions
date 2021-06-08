using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt
{
	public class TerrarianEntMinionBuff : MinionBuff
	{
		public TerrarianEntMinionBuff() : base(ProjectileType<TerrarianEntCounterMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Ent of the Forest");
			Description.SetDefault("A powerful forest spirit will fight for you!");
		}
	}

	public class TerrarianEntMinionItem : MinionItem<TerrarianEntMinionBuff, TerrarianEntCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Sacred Sapling");
			Tooltip.SetDefault("Summons a powerful forest spirit to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.damage = 165;
			item.value = Item.sellPrice(0, 3, 0, 0);
			item.rare = ItemRarityID.Red;
		}
	}

	public class TerrarianEntCounterMinion : CounterMinion
	{

		internal override int BuffId => BuffType<TerrarianEntMinionBuff>();
		protected override int MinionType => ProjectileType<TerrarianEntMinion>();
	}

	public class TerrarianEntMinion : EmpoweredMinion
	{
		internal override int BuffId => BuffType<TerrarianEntMinionBuff>();
		protected override int CounterType => ProjectileType<TerrarianEntCounterMinion>();

		private SpriteCompositionHelper scHelper;

		private int framesSinceLastHit;
		protected override int dustType => 2;

		private Texture2D bodyTexture;

		private Texture2D foliageTexture;
		private Texture2D vinesTexture;
		private List<LandChunkProjectile> subProjectiles;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ent of the Forest");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
			IdleLocationSets.trailingInAir.Add(projectile.type);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 44;
			projectile.height = 44;
			projectile.tileCollide = false;
			framesSinceLastHit = 0;
			projectile.friendly = true;
			attackThroughWalls = true;
			useBeacon = false;
			frameSpeed = 5;
			scHelper = new SpriteCompositionHelper(this)
			{
				idleCycleFrames = 160,
				frameResolution = 1,
				posResolution = 1
			};

			if(bodyTexture == null || foliageTexture == null || vinesTexture == null)
			{
				bodyTexture = GetTexture(Texture);
				foliageTexture = GetTexture(Texture + "_Foliage");
				vinesTexture = GetTexture(Texture + "_Vines");
			}

			subProjectiles = new List<LandChunkProjectile>();
		}


		private void DrawVines(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			float sinAngle = (float)Math.Sin(cycleAngle);
			Vector2 leftVine = new Vector2(-48, 78) + Vector2.One * 4 * sinAngle;
			Vector2 rightVine = new Vector2(64, 74) + new Vector2(1, -1) * -2 * sinAngle;
			// left vine
			helper.AddSpriteToBatch(vinesTexture, (0, 2),  leftVine);
			helper.AddSpriteToBatch(vinesTexture, (1, 2),  rightVine);
		}

		private void DrawFoliage(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			float sinAngle = (float)Math.Sin(cycleAngle);
			Vector2 leftLeaf = new Vector2(-66, -66) + Vector2.One * 2 * sinAngle;
			Vector2 middleLeaf = new Vector2(0, -100) + Vector2.UnitY * -3 * sinAngle;
			Vector2 rightLeaf = new Vector2(56, -64)  + Vector2.One * -2 * sinAngle;
			// left leaf
			helper.AddSpriteToBatch(foliageTexture, (1, 3),  leftLeaf);
			// middle leaf
			helper.AddSpriteToBatch(foliageTexture, (2, 3),  middleLeaf);
			// right leaf
			helper.AddSpriteToBatch(foliageTexture, (0, 3),  rightLeaf);
		}

		private void DrawBody(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			// body
			helper.AddSpriteToBatch(bodyTexture, (projectile.frame, 5),  Vector2.Zero);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			lightColor = Color.White * 0.75f;
			int i;
			for(i = 0; i < subProjectiles.Count; i++)
			{
				if(subProjectiles[i].projectile.position.Y > projectile.position.Y + 96)
				{
					break;
				}
				subProjectiles[i].SubPreDraw(spriteBatch, lightColor);
			}
			scHelper.Process(spriteBatch, lightColor, false, DrawVines, DrawBody, DrawFoliage);
			for(; i < subProjectiles.Count; i++)
			{
				subProjectiles[i].SubPreDraw(spriteBatch, lightColor);
			}
			return false;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			// center on the player at all times
			Vector2 idlePosition = player.Top;
			idlePosition.Y += -96 + 8 * (float)Math.Sin(MathHelper.TwoPi * groupAnimationFrame / groupAnimationFrames);
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition.X = player.Top.X;
				idlePosition.Y = player.Top.Y - 16;
			}
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			int subProjType = ProjectileType<LandChunkProjectile>();
			
			// get the list of currently active sub-projectiles
			subProjectiles.Clear();
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == player.whoAmI && p.type == subProjType)
				{
					subProjectiles.Add((LandChunkProjectile)p.modProjectile);
				}
			}
			subProjectiles.Sort((s1, s2) => (int)(s1.projectile.position.Y - s2.projectile.position.Y));


			if(Main.myPlayer == player.whoAmI && player.ownedProjectileCounts[subProjType] < 6 && animationFrame % 30 == 0)
			{
				int lowestUnspawnedTree;
				int firstMissingIdx = subProjectiles.Select(s => (int)s.projectile.ai[1]).OrderBy(v=>v).Where((v, idx) => v != idx).FirstOrDefault();
				if(firstMissingIdx == default)
				{
					lowestUnspawnedTree = player.ownedProjectileCounts[subProjType];
				} else
				{
					lowestUnspawnedTree = firstMissingIdx - 1;
				}
				Projectile.NewProjectile(
					player.Center,
					Vector2.Zero,
					subProjType,
					projectile.damage,
					0,
					player.whoAmI,
					ai1: lowestUnspawnedTree);
			}
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
		}

		protected override int ComputeDamage()
		{
			return baseDamage + (baseDamage / 6) * EmpowerCount; // only scale up damage a little bit
		}

		private Vector2? GetTargetVector()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance, player.Center) is Vector2 target2)
			{
				return target2 - projectile.Center;
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

		protected override float ComputeSearchDistance() => 800 + 20 * EmpowerCount;

		protected override float ComputeInertia() => 5;

		protected override float ComputeTargetedSpeed() => 18;

		protected override float ComputeIdleSpeed() => 18;

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame) { /* no-op */ }

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			// frames go back and forth rather than looping
			int rawFrame = (animationFrame / 8) % 20;
			if(rawFrame < 7)
			{
				projectile.frame = 0;
			} else if (rawFrame < 10)
			{
				projectile.frame = rawFrame - 6;
			} else if (rawFrame < 17)
			{
				projectile.frame = 4;
			} else
			{
				projectile.frame = 20 - rawFrame;
			}
			projectile.rotation = projectile.velocity.X * 0.01f;
			
		}
	}
}
