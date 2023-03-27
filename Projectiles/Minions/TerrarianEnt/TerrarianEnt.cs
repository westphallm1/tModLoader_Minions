using AmuletOfManyMinions.Projectiles.Minions.Acorn;
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
using ReLogic.Content;
using static Terraria.ModLoader.ModContent;
using AmuletOfManyMinions.Core;

namespace AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt
{
	public class TerrarianEntMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<TerrarianEntCounterMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ent of the Forest");
			Description.SetDefault("A powerful forest spirit will fight for you!");
		}
	}

	public class TerrarianEntMinionItem : MinionItem<TerrarianEntMinionBuff, TerrarianEntCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Staff of the Sacred Sapling");
			Tooltip.SetDefault("Summons a powerful forest spirit to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3f;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 185;
			Item.value = Item.sellPrice(0, 15, 0, 0);
			Item.rare = ItemRarityID.Red;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemType<AcornMinionItem>(), 1).AddIngredient(ItemID.LunarBar, 6).AddIngredient(ItemID.FragmentNebula, 6).AddIngredient(ItemID.FragmentSolar, 6).AddIngredient(ItemID.FragmentStardust, 6).AddIngredient(ItemID.FragmentVortex, 6).AddTile(TileID.LunarCraftingStation).Register();
		}
	}

	public class TerrarianEntCounterMinion : CounterMinion
	{

		public override int BuffId => BuffType<TerrarianEntMinionBuff>();
		protected override int MinionType => ProjectileType<TerrarianEntMinion>();
	}

	public class TerrarianEntMinion : EmpoweredMinion
	{
		public override int BuffId => BuffType<TerrarianEntMinionBuff>();
		public override int CounterType => ProjectileType<TerrarianEntCounterMinion>();
		
		private SpriteCompositionHelper scHelper;

		protected override int dustType => 2;

		private Asset<Texture2D> bodyTexture;

		private List<LandChunkProjectile> subProjectiles;
		private Projectile swingingProjectile;
		private int nextTreeIndex;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ent of the Forest");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 1;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}
		
		public override void ApplyCrossModChanges()
		{
			base.ApplyCrossModChanges();
			CrossModClient.SummonersShine.General.ApplyChanges_STEPPED(Type);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 44;
			Projectile.height = 44;
			Projectile.tileCollide = false;
			DealsContactDamage = false;
			AttackThroughWalls = true;
			UseBeacon = false;
			FrameSpeed = 5;

			subProjectiles = new List<LandChunkProjectile>();
		}
		public override void LoadAssets()
		{
			AddTexture(Texture + "_Foliage");
			AddTexture(Texture + "_Vines");
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// main guy doesn't hit anything
			return false;
		}


		private void DrawVines(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			float sinAngle = (float)Math.Sin(cycleAngle);
			Vector2 leftVine = new Vector2(-48, 78) + Vector2.One * 4 * sinAngle;
			Vector2 rightVine = new Vector2(64, 74) + new Vector2(1, -1) * -2 * sinAngle;
			// left vine
			Texture2D value = ExtraTextures[1].Value;
			helper.AddSpriteToBatch(value, (0, 2),  leftVine);
			helper.AddSpriteToBatch(value, (1, 2),  rightVine);
		}

		private void DrawFoliage(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			float sinAngle = (float)Math.Sin(cycleAngle);
			Vector2 leftLeaf = new Vector2(-66, -66) + Vector2.One * 2 * sinAngle;
			Vector2 middleLeaf = new Vector2(0, -100) + Vector2.UnitY * -3 * sinAngle;
			Vector2 rightLeaf = new Vector2(56, -64)  + Vector2.One * -2 * sinAngle;
			// left leaf
			Texture2D value = ExtraTextures[0].Value;
			helper.AddSpriteToBatch(value, (1, 3),  leftLeaf);
			// middle leaf
			helper.AddSpriteToBatch(value, (2, 3),  middleLeaf);
			// right leaf
			helper.AddSpriteToBatch(value, (0, 3),  rightLeaf);
		}

		private void DrawBody(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			// body
			helper.AddSpriteToBatch(bodyTexture.Value, (Projectile.frame, 5),  Vector2.Zero);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			lightColor = Color.White * 0.75f;
			int i;
			for(i = 0; i < subProjectiles.Count; i++)
			{
				if(subProjectiles[i].Projectile.position.Y > Projectile.position.Y + 96)
				{
					break;
				}
				subProjectiles[i].SubPreDraw(lightColor);
			}
			scHelper.Draw(lightColor);
			for(; i < subProjectiles.Count; i++)
			{
				subProjectiles[i].SubPreDraw(lightColor);
			}
			return false;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			scHelper = new SpriteCompositionHelper(this, new Rectangle(0, 0, 300, 300))
			{
				idleCycleFrames = 160,
				frameResolution = 1,
				posResolution = 1
			};
			if(bodyTexture == null && !Main.dedServ)
			{
				Main.instance.LoadProjectile(Projectile.type);
				bodyTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type];
			}
			scHelper.Attach();
		}
		public override void AfterMoving()
		{
			scHelper.UpdateDrawers(false, DrawVines, DrawBody, DrawFoliage);
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			// center on the player at all times
			Vector2 idlePosition = Player.Top;
			idlePosition.Y += -96 + 8 * (float)Math.Sin(MathHelper.TwoPi * GroupAnimationFrame / GroupAnimationFrames);
			if(swingingProjectile != default)
			{
				int attackStyle = (int)Projectile.ai[1] / 2;
				int swingTravelRadius = attackStyle == 2 ? 64 : 24;
				Vector2 swingOffset = swingingProjectile.Center - idlePosition;
				swingOffset.SafeNormalize();
				swingOffset *= swingTravelRadius;
				idlePosition += swingOffset;
			}
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			if (!Collision.CanHitLine(idlePosition, 1, 1, Player.Center, 1, 1))
			{
				idlePosition.X = Player.Top.X;
				idlePosition.Y = Player.Top.Y - 16;
			}
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			SpawnTrees();
			return vectorToIdlePosition;
		}

		public void SpawnTrees()
		{
			int maxCount = Math.Min(6, EmpowerCount + 1);
			int subProjType = ProjectileType<LandChunkProjectile>();
			
			// get the list of currently active sub-projectiles
			if(swingingProjectile != null && (!swingingProjectile.active || swingingProjectile.localAI[0] == 0))
			{
				swingingProjectile = null;
			} 
			subProjectiles.Clear();
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == Player.whoAmI && p.type == subProjType)
				{
					subProjectiles.Add((LandChunkProjectile)p.ModProjectile);
					if(swingingProjectile == null && p.localAI[0] != 0)
					{
						swingingProjectile = p;
					}
				}
			}
			subProjectiles.Sort((s1, s2) => (int)(s1.Projectile.position.Y - s2.Projectile.position.Y));
			List<float> idle = subProjectiles.Select(s => s.Projectile.ai[1])
				.Where(v => v> -1)
				.OrderBy(v=>v).ToList();


			if(Main.myPlayer == Player.whoAmI && idle.Count < maxCount && AnimationFrame % 30 == 0)
			{
				for(int i = 0; i < idle.Count; i++)
				{
					if(idle.Contains(nextTreeIndex))
					{
						nextTreeIndex = (nextTreeIndex + 1) % maxCount;
					} else
					{
						break;
					}
				}
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Player.Center,
					Vector2.Zero,
					subProjType,
					Projectile.damage,
					0,
					Player.whoAmI,
					ai1: nextTreeIndex);
				nextTreeIndex = (nextTreeIndex + 1) % maxCount;
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// stay floating behind the player at all times
			IdleMovement(VectorToIdle);
		}

		protected override int ComputeDamage()
		{
			return (int)(baseDamage + (baseDamage / 6) * EmpowerCountWithFalloff()); // only scale up damage a little bit
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

		protected override float ComputeSearchDistance() => 800 + 20 * EmpowerCount;

		protected override float ComputeInertia() => 5;

		protected override float ComputeTargetedSpeed() => 18;

		protected override float ComputeIdleSpeed() => 18;

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame) { /* no-op */ }

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			// frames go back and forth rather than looping
			int rawFrame = (AnimationFrame / 8) % 20;
			if(rawFrame < 7)
			{
				Projectile.frame = 0;
			} else if (rawFrame < 10)
			{
				Projectile.frame = rawFrame - 6;
			} else if (rawFrame < 17)
			{
				Projectile.frame = 4;
			} else
			{
				Projectile.frame = 20 - rawFrame;
			}
			Projectile.rotation = Projectile.velocity.X * 0.01f;
			
		}
	}
}
