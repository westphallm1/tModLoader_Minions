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
	public class LandChunkProjectile : SimpleMinion
	{
		public override string Texture => "Terraria/Item_0";

		internal override int BuffId => BuffType<TerrarianEntMinionBuff>();

		internal SpriteCompositionHelper scHelper;

		internal CompositeSpriteBatchDrawer[] drawers;
		internal SpriteCycleDrawer[] drawFuncs;


		internal int spawnFrames = 30;

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
			projectile.timeLeft = 240;
			projectile.localNPCHitCooldown = 30;
			projectile.usesLocalNPCImmunity = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.minionSlots = 0;
			projectile.rotation = 0;
			attackThroughWalls = true;
			usesTactics = false;
			useBeacon = false;
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
			int treeIdx = Math.Max(0,(int)projectile.ai[0] - 1);
			drawers = LandChunkConfigs.templates[treeIdx % LandChunkConfigs.templates.Length]();
			drawFuncs = new SpriteCycleDrawer[drawers.Length];
			for(int i = 0; i < drawers.Length; i++)
			{
				drawFuncs[i] = drawers[i].Draw;
			}
		}


		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			lightColor = Color.White * 0.75f;
			scHelper.Process(spriteBatch, lightColor, false, drawFuncs);
			return false;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// TODO real behavior
			int ai0 = (int)projectile.ai[0];
			int baseXoffset = ai0 % 2 == 0 ? 64 : -96;
			int xOffset = Math.Sign(baseXoffset) * 48 * (ai0 / 2) + baseXoffset;
			int yOffset = 48 - 64 * (ai0 / 2);
			projectile.position = player.Center + new Vector2(xOffset, yOffset + 8 * (float)Math.Sin(MathHelper.TwoPi * animationFrame / 60));
			projectile.velocity = Vector2.Zero;
		}

		public override Vector2 IdleBehavior()
		{
			Array.ForEach(drawers, d => d.Update(projectile, animationFrame, spawnFrames));
			return Vector2.Zero;
		}

		public override Vector2? FindTarget()
		{
			// No-op
			if(animationFrame < 90)
			{
				return null;
			}
			return null;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// No-op
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
