using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt
{
	/// <summary>
	/// Uses ai[1] for positioning/sprite selection purposes
	/// </summary>
	public class LandChunkProjectile : GroupAwareMinion
	{
		public override string Texture => "Terraria/Item_0";

		internal override int BuffId => BuffType<TerrarianEntMinionBuff>();

		internal SpriteCompositionHelper scHelper;

		internal CompositeSpriteBatchDrawer[] drawers;
		internal SpriteCycleDrawer[] drawFuncs;
		internal Vector2 travelDir;
		internal int travelStartFrame;


		internal int spawnFrames = 30;
		internal int attackDelayFrames = 20;
		internal int framesToLiveAfterAttack = 120;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
			Main.projFrames[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			projectile.localNPCHitCooldown = 6;
			projectile.minionSlots = 0;
			attackThroughWalls = true;
			useBeacon = false;
			attackFrames = 60;
			scHelper = new SpriteCompositionHelper(this)
			{
				idleCycleFrames = 160,
				frameResolution = 1,
				posResolution = 1
			};
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			int treeIdx = Math.Max(0,(int)projectile.ai[1] - 1);
			drawers = LandChunkConfigs.templates[treeIdx % LandChunkConfigs.templates.Length]();
			drawFuncs = new SpriteCycleDrawer[drawers.Length];
			for(int i = 0; i < drawers.Length; i++)
			{
				drawFuncs[i] = drawers[i].Draw;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(travelDir == default)
			{
				return false;
			}
			projHitbox.Inflate(64, 64);
			return projHitbox.Intersects(targetHitbox);
		}

		// for layering purposes, this needs to be done manually
		// Called from TerrarianEnt.PreDraw
		public void SubPreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			scHelper.Process(spriteBatch, lightColor, false, drawFuncs);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return false;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			float[] angleOffsets = { 0, MathHelper.PiOver4, -MathHelper.PiOver4 };
			int ai1 = (int)projectile.ai[1];
			bool isEven = ai1 % 2 == 0;
			int sideOffset = ai1 / 2;
			Vector2 center = new Vector2(-16, -64);
			float baseAngle = isEven ? angleOffsets[sideOffset] : MathHelper.Pi + angleOffsets[sideOffset];
			baseAngle += MathHelper.Pi / 16 * (float) Math.Sin(MathHelper.TwoPi * groupAnimationFrame / groupAnimationFrames);
			Vector2 offset = 164 * baseAngle.ToRotationVector2();
			offset.Y *= 0.5f;
			projectile.rotation = MathHelper.Pi/48 * (float) Math.Sin(MathHelper.TwoPi * animationFrame / 120);
			projectile.position = player.Center + center + offset;
			projectile.velocity = Vector2.Zero;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Array.ForEach(drawers, d => d.Update(projectile, animationFrame, spawnFrames));
			return Vector2.Zero;
		}

		public override Vector2? FindTarget()
		{
			// TODO lift some EmpoweredMinion stuff from here
			if(animationFrame < attackDelayFrames)
			{
				return null;
			} else if (travelDir != default)
			{
				return travelDir;
			} else if (IsMyTurn() && SelectedEnemyInRange(1000, player.Center, 1000) is Vector2 target)
			{
				travelDir = target - projectile.Center;
				travelDir.SafeNormalize();
				travelDir *= 14;
				if(targetNPCIndex is int idx)
				{
					travelDir += Main.npc[idx].velocity;
				}
				travelStartFrame = animationFrame;
				return travelDir;
			}
			return null;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// No-op
			projectile.rotation += MathHelper.Pi / 8;
			// just
			float travelFraction = Math.Max(1, (animationFrame - travelStartFrame) / 14f);
			projectile.velocity = travelFraction * vectorToTargetPosition;
		}

		public override void AfterMoving()
		{
			if(travelStartFrame != default && (animationFrame - travelStartFrame > framesToLiveAfterAttack 
				|| Vector2.DistanceSquared(player.Center, projectile.Center) > 1300f * 1300f))
			{
				projectile.Kill();
			}
		}
	}

	public class LandChunkConfigs
	{

		public static Func<CompositeSpriteBatchDrawer[]>[] templates;
		public static void Load()
		{
			// TODO some assembly stuff to autoload these
			templates = new Func<CompositeSpriteBatchDrawer[]>[] { ForestTree, PalmTree, SnowyTree, JungleTree, HallowedTree, CorruptTree, CrimsonTree };
		}

		public static void Unload()
		{
			templates = null;
		}

		public static CompositeSpriteBatchDrawer[] Sunflowers()
		{
			return new CompositeSpriteBatchDrawer[]
			{
				new MonumentDrawer(GetTexture("Terraria/Tiles_27"), new Rectangle(2 * Main.rand.Next(3), 0, 2, 4)),
				new ClutterDrawer(GetTexture("Terraria/Tiles_3"), 
					Enumerable.Repeat(0, 4).Select(_ => Main.rand.Next(10)).ToArray(), 
					height: 20),
				new TileDrawer(GetTexture("Terraria/Tiles_2"),  2)
			};
		}
		public static CompositeSpriteBatchDrawer[] Statue()
		{
			int tileTexture = Main.rand.NextBool() ? 179 : 1;
			return new CompositeSpriteBatchDrawer[]
			{
				new MonumentDrawer(GetTexture("Terraria/Tiles_105"), new Rectangle(2* Main.rand.Next(20), 0, 2, 3)),
				// no clutter
				new TileDrawer(GetTexture("Terraria/Tiles_" + tileTexture), 1)
			};
		}
		private static TreeDrawer MakeTreeDrawer(int[] tileSets, string trunkIdx, int minHeight = 3, int maxHeight = 6, int topFrames =3, int branchFrames = 3)
		{
			int tileSet = tileSets[Main.rand.Next(tileSets.Length)];
			Texture2D folliageTexture = GetTexture("Terraria/Tree_Tops_" + tileSet);
			Texture2D branchTexture = GetTexture("Terraria/Tree_Branches_" + tileSet);
			Rectangle folliageBounds = new Rectangle(
				folliageTexture.Width / topFrames * Main.rand.Next(topFrames), 0, 
				folliageTexture.Width / topFrames, folliageTexture.Height);
			return new TreeDrawer(
				folliageTexture,
				GetTexture("Terraria/Tiles_"+trunkIdx),
				branchTexture,
				folliageBounds,
				trunkHeight: Main.rand.Next(minHeight, maxHeight),
				branchFrames: branchFrames);
		}

		public static CompositeSpriteBatchDrawer[] ForestTree()
		{
			int[] tileSets = { 0, 6, 7, 8, 9, 10 };
			return new CompositeSpriteBatchDrawer[]
			{
				MakeTreeDrawer(tileSets, "5"),
				new ClutterDrawer(GetTexture("Terraria/Tiles_3"),
					new int[] { Main.rand.Next(10), -1, -1, Main.rand.Next(10)},
					height: 20),
				new TileDrawer(GetTexture("Terraria/Tiles_2"),  2)
			};
		}

		public static CompositeSpriteBatchDrawer[] CorruptTree()
		{
			int[] tileSets = { 1 };
			return new CompositeSpriteBatchDrawer[]
			{
				MakeTreeDrawer(tileSets, "5_0"),
				new ClutterDrawer(GetTexture("Terraria/Tiles_24"),
					new int[] { Main.rand.Next(10), -1, -1, Main.rand.Next(20)},
					height: 20),
				new TileDrawer(GetTexture("Terraria/Tiles_23"),  14)
			};
		}

		public static CompositeSpriteBatchDrawer[] CrimsonTree()
		{
			int[] tileSets = { 5 };
			return new CompositeSpriteBatchDrawer[]
			{
				MakeTreeDrawer(tileSets, "5_4"),
				new TileDrawer(GetTexture("Terraria/Tiles_199"),  125)
			};
		}

		public static CompositeSpriteBatchDrawer[] SnowyTree()
		{
			int[] tileSets = { 4, 12, 16, 17, 18 };
			return new CompositeSpriteBatchDrawer[]
			{
				MakeTreeDrawer(tileSets, "5_3"),
				new TileDrawer(GetTexture("Terraria/Tiles_147"),  51)
			};
		}

		public static CompositeSpriteBatchDrawer[] JungleTree()
		{
			int[] tileSets = { 2, 11, 13 };
			return new CompositeSpriteBatchDrawer[]
			{
				MakeTreeDrawer(tileSets, Main.rand.NextBool() ? "5_1" : "5_5"),
				new ClutterDrawer(GetTexture("Terraria/Tiles_61"),
					new int[] { Main.rand.Next(20), -1, -1, Main.rand.Next(20)},
					height: 20),
				new TileDrawer(GetTexture("Terraria/Tiles_60"),  39)
			};
		}
		public static CompositeSpriteBatchDrawer[] HallowedTree()
		{
			int[] tileSets = { 3 };
			TreeDrawer drawer = MakeTreeDrawer(tileSets, "5_2", topFrames: 9, branchFrames: 9, minHeight: 2, maxHeight: 4);
			drawer.trunkHeadstartFrames = 3;
			return new CompositeSpriteBatchDrawer[]
			{
				drawer,
				new ClutterDrawer(GetTexture("Terraria/Tiles_110"),
					new int[] { Main.rand.Next(20), -1, -1, Main.rand.Next(20)},
					height: 20),
				new TileDrawer(GetTexture("Terraria/Tiles_109"),  39)
			};
		}

		public static CompositeSpriteBatchDrawer[] PalmTree()
		{
			Texture2D folliageTexture = GetTexture("Terraria/Tree_Tops_15");
			Rectangle folliageBounds = new Rectangle(
				folliageTexture.Width / 3* Main.rand.Next(3), 0, 
				folliageTexture.Width / 3, folliageTexture.Height / 4);
			return new CompositeSpriteBatchDrawer[]
			{
				new PalmTreeDrawer(
					folliageTexture,
					GetTexture("Terraria/Tiles_323"),
					null,
					folliageBounds,
					trunkHeight: Main.rand.Next(3, 6)),
				// TODO add starfish
				new TileDrawer(GetTexture("Terraria/Tiles_53"),  39)
			};
		}
	}
}
