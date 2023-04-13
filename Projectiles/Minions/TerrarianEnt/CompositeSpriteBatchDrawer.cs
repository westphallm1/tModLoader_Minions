using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt
{
	public abstract class CompositeSpriteBatchDrawer
	{
		// these need to be injected internally each frame, a bit annoying
		internal int animationFrame;
		internal int spawnFrames;

		// This is technically specific to TileDrawer, but referenced by enough
		// other subclasses
		// Pixels from center of projectile to top of "the ground"
		internal readonly int TileTop = 16;


		internal virtual void Update(Projectile proj, int animationFrame, int spawnFrames)
		{
			this.animationFrame = animationFrame;
			this.spawnFrames = spawnFrames;
		}

		internal abstract void Draw(SpriteCompositionHelper helper, int frame, float cycleAngle);
	}

	public class TileDrawer : CompositeSpriteBatchDrawer
	{
		internal Texture2D groundTexture;
		internal int dustId = -1;

		public TileDrawer(Texture2D groundTexture, int dustId = -1)
		{
			this.groundTexture = groundTexture;
			this.dustId = dustId;
		}
		
		private (byte, byte)?[,,] groundTiles =
		{
			{
				{ null, null, null, null},
				{ null, (9,0), (12, 0), null},
				{ null, null, null, null},
			},

			{
				{ (7, 0), null, null, null},
				{ (7, 16), (7,4), (12, 0), null},
				{ null, null, null, null},
			},

			{
				{ (0, 3), (1, 0), (1, 3), null},
				{ (0, 4), (1,2), (1, 4), null},
				{ null, null, null, null},
			},
			{
				{ (0, 3), (1, 0), (2, 0), (1, 3) },
				{ (0, 4), (1, 2), (2, 2), (1, 4) },
				{ null, null, null, null},
			},
			{
				{ (0, 3), (1, 0), (2, 0), (1, 3) },
				{ (0, 4), (1, 1), (2, 1), (1, 4) },
				{ null, (0, 4), (1, 4), null},
			},
		};

		internal override void Update(Projectile proj, int animationFrame, int spawnFrames)
		{
			base.Update(proj, animationFrame, spawnFrames);
			if (dustId > - 1 && animationFrame <= spawnFrames && animationFrame > 0 && animationFrame % (spawnFrames / 4) == 0)
			{
				for (int i = 0; i < 3; i++)
				{
					Dust.NewDust(proj.TopLeft, 24, 24, dustId, 0, 0);
				}
			}
		}
		internal override void Draw(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			int tileFrame = animationFrame > spawnFrames ? 4 : animationFrame / (spawnFrames / 4);
			helper.AddTileSpritesToBatch(groundTexture, tileFrame, groundTiles, Vector2.Zero, 0);
		}


	}
	public class ClutterDrawer : CompositeSpriteBatchDrawer
	{
		internal int[] clutterFrames;
		internal Texture2D clutterTexture;
		internal int clutterWidth = 16;
		internal int clutterHeight = 16;

		public ClutterDrawer(Texture2D texture, int[] clutterFrames, int width = 16, int height = 16)
		{
			this.clutterTexture = texture;
			this.clutterFrames = clutterFrames;
			this.clutterWidth = width;
			this.clutterHeight = height;
		}
		internal override void Draw(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			if (animationFrame < spawnFrames)
			{
				return;
			}
			for (int i = 0; i < clutterFrames.Length; i++)
			{
				// TODO figure this out programatically
				int maxHeight = 30;
				int yOffset = Math.Min(maxHeight, 2 * (animationFrame - spawnFrames - 4 * i));
				if (yOffset < 0 || clutterFrames[i] == -1)
				{
					continue;
				}
				Vector2 offset = new Vector2(-24 + 16 * i, -yOffset);
				int idx = clutterFrames[i];
				Rectangle bounds = new Rectangle((clutterWidth + 2) * idx, 0, clutterWidth, clutterHeight);
				helper.AddSpriteToBatch(clutterTexture, bounds, offset, 0, 1);
			}
		}
	}

	public class MonumentDrawer : CompositeSpriteBatchDrawer
	{
		internal Rectangle monumentBounds;
		internal Texture2D monumentTexture;
		internal int growthRate = 4;
		internal (byte, byte)?[,,] tiles;

		public MonumentDrawer(Texture2D texture, Rectangle bounds, int growthRate = 4)
		{
			monumentTexture = texture;
			monumentBounds = bounds;
			this.growthRate = growthRate;
			tiles = new (byte, byte)?[1, monumentBounds.Height, monumentBounds.Width];
		}
		internal override void Draw(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			if (animationFrame < spawnFrames)
			{
				return;
			}
			
			int heightToDraw = (int)Math.Min(monumentBounds.Height - 1, (animationFrame - spawnFrames - growthRate) * growthRate / 16f);
			int maxHeight = TileTop + 8 * monumentBounds.Height;
			int heightOffset = Math.Min(maxHeight, growthRate * (animationFrame - spawnFrames) - 32);
			// programmatic build here feels a bit iffy
			for(int j = 0; j < monumentBounds.Width; j++)
			{
				tiles[0, heightToDraw, j] = ((byte)(monumentBounds.X + j), (byte)(monumentBounds.Y + heightToDraw));
			}
			helper.AddTileSpritesToBatch(monumentTexture, 0, tiles, Vector2.UnitY * -heightOffset, 0);
		}
	}

	public class TreeDrawer : CompositeSpriteBatchDrawer
	{
		internal Rectangle folliageBounds;
		internal Texture2D folliageTexture;
		internal Texture2D woodTexture;
		internal Texture2D branchesTexture;
		// in tiles, not pixels
		internal int trunkHeight = 5;
		internal int folliageSpawnFrames = 20;
		// config for roots and branches orientations
		internal int rootsConfig = 0;
		internal int branchesConfig = 0;
		int treeTileSize = 20;
		int branchFrames = 3;
		internal (byte, byte)?[,,] tiles;

		// Used to draw trunk itself, branches, and roots
		private int trunkAnimFrame = -1;
		private int trunkTileRowToDraw;
		private int trunkHeightOffset;
		internal int trunkHeadstartFrames;

		internal virtual (byte, byte) TrunkTileForLocation(int row) => (0, (byte)(row % 3));

		internal override void Update(Projectile proj, int animationFrame, int spawnFrames)
		{
			base.Update(proj, animationFrame, spawnFrames);
			// the positioning info for the tree roots is shared across multiple methods, so only
			// compute it once
			int startFrame = spawnFrames + folliageSpawnFrames - trunkHeadstartFrames;
			// extremely hacky, smaller trunks lag behind a bit
			if(trunkHeight < 4)
			{
				startFrame -= 2;
			}
			if (animationFrame < startFrame)
			{
				return;
			}
			trunkAnimFrame = animationFrame - startFrame;
			int heightPerFrame = folliageBounds.Height / folliageSpawnFrames;
			trunkTileRowToDraw = (int)Math.Min(trunkHeight-1, trunkAnimFrame * heightPerFrame / (float)treeTileSize);
			int maxHeight = TileTop + treeTileSize / 2 * trunkHeight;
			trunkHeightOffset = Math.Min(maxHeight, heightPerFrame * trunkAnimFrame - treeTileSize);

			// bottom of tree configs
			if (trunkTileRowToDraw == trunkHeight - 1 && rootsConfig != 1)
			{
				// tile where roots connect
				tiles[0, trunkTileRowToDraw, 1] = ((byte)(rootsConfig == 0 ? 0 : 3), (byte)(6 + trunkTileRowToDraw % 3));
			}
			else
			{
				// rest of trunk
				tiles[0, trunkTileRowToDraw, 1] = TrunkTileForLocation(trunkTileRowToDraw);
			}
		}

		public TreeDrawer(
			Texture2D folliageTexture, 
			Texture2D woodTexture, 
			Texture2D branchesTexture, 
			Rectangle folliageBounds, 
			int trunkHeight = 4, 
			int decorationConfig = -1,
			int branchFrames = 3,
			int trunkHeadstartFrames = 0)
		{
			this.folliageTexture = folliageTexture;
			this.woodTexture = woodTexture;
			this.branchesTexture = branchesTexture;
			this.folliageBounds = folliageBounds;
			this.trunkHeight = trunkHeight;
			this.trunkHeadstartFrames = trunkHeadstartFrames;
			this.branchFrames = branchFrames;
			decorationConfig = decorationConfig > -1 ? decorationConfig : Main.rand.Next(12);
			rootsConfig = decorationConfig % 3;
			branchesConfig = decorationConfig % 4;

			tiles = new (byte, byte)?[1, trunkHeight, 3];
		}

		internal void DrawTreeTop(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			int heightPerFrame = folliageBounds.Height / folliageSpawnFrames;
			int animFrame = animationFrame - spawnFrames;
			int heightToDraw = Math.Min(folliageBounds.Height, animFrame * 2 * heightPerFrame);

			float scale = Math.Max(0.1f, Math.Min(1, animFrame / (float) folliageSpawnFrames));
			int maxHeight = TileTop + treeTileSize * trunkHeight + folliageBounds.Height/2;
			int heightOffset = Math.Min(maxHeight, heightPerFrame * animFrame - treeTileSize);
			Rectangle bounds = new Rectangle(folliageBounds.X, folliageBounds.Y, folliageBounds.Width, heightToDraw);
			helper.AddSpriteToBatch(folliageTexture, bounds, new Vector2(1, 2 - heightOffset) * scale, 0, scale);
		}

		internal void DrawTrunk(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			if(trunkAnimFrame < 0)
			{
				return;
			}
			helper.AddTileSpritesToBatch(woodTexture, 0, tiles, Vector2.UnitY * -trunkHeightOffset, tileSize: treeTileSize);
		}
		internal void DrawDecorations(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			int rootsTile = trunkHeight - 1;
			int branchesTile = (trunkHeight -1) / 2;
			Vector2 trunkOffset = Vector2.UnitY * -trunkHeightOffset;
			// roots
			int xMinusPadding = treeTileSize - 4;
			if (trunkTileRowToDraw >= rootsTile && rootsConfig != 1)
			{
				int xOffset = (xMinusPadding) * (rootsConfig == 0 ? 1 : -1);
				float yOffset = (treeTileSize * (trunkHeight -1)) / 2f;
				Vector2 rootOffset = new Vector2(xOffset, yOffset);
				(byte, byte)?[, , ] rootTile = {{{((byte)(rootsConfig == 0 ?  1 : 2), (byte)(6 + trunkTileRowToDraw%3))}}};
				helper.AddTileSpritesToBatch(woodTexture, 0, rootTile,  trunkOffset + rootOffset, tileSize: treeTileSize);
			}
			// branches
			if (trunkTileRowToDraw >= branchesTile && branchesConfig < 2)
			{
				int branchWidth = branchesTexture.Width / 2;
				int branchHeight = branchesTexture.Height / branchFrames;
				int branchPadding = branchesConfig == 0 ? -2 : 4;
				int xOffset = branchPadding + (xMinusPadding + branchWidth) / 2 * (branchesConfig == 0 ? 1 : -1);
				float yOffset = treeTileSize * (branchesTile) / 2f - branchHeight/2f;
				Vector2 branchOffset = new Vector2(xOffset, yOffset);
				Rectangle bounds = new Rectangle((1-branchesConfig) * branchWidth, branchHeight * (branchesTile % branchFrames), branchWidth, branchHeight);
				helper.AddSpriteToBatch(branchesTexture, bounds, trunkOffset + branchOffset, 0, 1);
			}

		}

		internal override void Draw(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			if (animationFrame < spawnFrames)
			{
				return;
			}

			// first, draw folliage
			DrawTreeTop(helper, frame, cycleAngle);
			// then, draw trunk
			DrawTrunk(helper, frame, cycleAngle);
			// then, draw branches/roots
			DrawDecorations(helper, frame, cycleAngle);
		}
	}

	public class PalmTreeDrawer : TreeDrawer
	{
		public PalmTreeDrawer(
			Texture2D folliageTexture, 
			Texture2D woodTexture, 
			Texture2D branchesTexture, 
			Rectangle folliageBounds, 
			int trunkHeight = 4, 
			int decorationConfig = -1, 
			int branchFrames = 3, 
			int trunkHeadstartFrames = 0) : 
			base(folliageTexture, woodTexture, branchesTexture, folliageBounds, trunkHeight, decorationConfig, branchFrames, trunkHeadstartFrames)
		{
			// no roots or branches
			rootsConfig = 1;
			branchesConfig = 2;
		}

		internal override (byte, byte) TrunkTileForLocation(int row)
		{
			return ((byte)(row % 3), 0);
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
			Main.instance.LoadTiles(27);
			Main.instance.LoadTiles(3);
			Main.instance.LoadTiles(2);
			return new CompositeSpriteBatchDrawer[]
			{
				new MonumentDrawer(TextureAssets.Tile[27].Value, new Rectangle(2 * Main.rand.Next(3), 0, 2, 4)),
				new ClutterDrawer(TextureAssets.Tile[3].Value, 
					Enumerable.Repeat(0, 4).Select(_ => Main.rand.Next(10)).ToArray(), 
					height: 20),
				new TileDrawer(TextureAssets.Tile[2].Value,  2)
			};
		}
		public static CompositeSpriteBatchDrawer[] Statue()
		{
			int tileTexture = Main.rand.NextBool() ? 179 : 1;
			Main.instance.LoadTiles(tileTexture);
			Main.instance.LoadTiles(105);
			return new CompositeSpriteBatchDrawer[]
			{
				new MonumentDrawer(TextureAssets.Tile[105].Value, new Rectangle(2* Main.rand.Next(20), 0, 2, 3)),
				// no clutter
				new TileDrawer(TextureAssets.Tile[tileTexture].Value, 1)
			};
		}
		private static TreeDrawer MakeTreeDrawer(int[] tileSets, int trunkIdx = -1, int minHeight = 3, int maxHeight = 6, int topFrames =3, int branchFrames = 3)
		{
			int tileSet = tileSets[Main.rand.Next(tileSets.Length)];
			Texture2D folliageTexture = TextureAssets.TreeTop[tileSet].Value;
			Texture2D branchTexture = TextureAssets.TreeBranch[tileSet].Value;
			Rectangle folliageBounds = new Rectangle(
				folliageTexture.Width / topFrames * Main.rand.Next(topFrames), 0, 
				folliageTexture.Width / topFrames, folliageTexture.Height);
			Asset<Texture2D> trunk;
			if (trunkIdx > -1 && trunkIdx < TextureAssets.Wood.Length)
			{
				trunk = TextureAssets.Wood[trunkIdx];
			}
			else
			{
				trunk = TextureAssets.Tile[5];
			}
			return new TreeDrawer(
				folliageTexture,
				trunk.Value,
				branchTexture,
				folliageBounds,
				trunkHeight: Main.rand.Next(minHeight, maxHeight),
				branchFrames: branchFrames);
		}

		public static CompositeSpriteBatchDrawer[] ForestTree()
		{
			int[] tileSets = { 0, 6, 7, 8, 9, 10 };
			Main.instance.LoadTiles(3);
			Main.instance.LoadTiles(2);
			return new CompositeSpriteBatchDrawer[]
			{
				MakeTreeDrawer(tileSets),
				new ClutterDrawer(TextureAssets.Tile[3].Value,
					new int[] { Main.rand.Next(10), -1, -1, Main.rand.Next(10)},
					height: 20),
				new TileDrawer(TextureAssets.Tile[2].Value,  2)
			};
		}

		public static CompositeSpriteBatchDrawer[] CorruptTree()
		{
			int[] tileSets = { 1 };
			Main.instance.LoadTiles(24);
			Main.instance.LoadTiles(23);
			return new CompositeSpriteBatchDrawer[]
			{
				MakeTreeDrawer(tileSets, 0),
				new ClutterDrawer(TextureAssets.Tile[24].Value,
					new int[] { Main.rand.Next(10), -1, -1, Main.rand.Next(20)},
					height: 20),
				new TileDrawer(TextureAssets.Tile[23].Value,  14)
			};
		}

		public static CompositeSpriteBatchDrawer[] CrimsonTree()
		{
			int[] tileSets = { 5 };
			Main.instance.LoadTiles(199);
			return new CompositeSpriteBatchDrawer[]
			{
				MakeTreeDrawer(tileSets, 4),
				new TileDrawer(TextureAssets.Tile[199].Value,  125)
			};
		}

		public static CompositeSpriteBatchDrawer[] SnowyTree()
		{
			int[] tileSets = { 4, 12, 16, 17, 18 };
			Main.instance.LoadTiles(147);
			return new CompositeSpriteBatchDrawer[]
			{
				MakeTreeDrawer(tileSets, 3),
				new TileDrawer(TextureAssets.Tile[147].Value,  51)
			};
		}

		public static CompositeSpriteBatchDrawer[] JungleTree()
		{
			int[] tileSets = { 2, 11, 13 };
			Main.instance.LoadTiles(61);
			Main.instance.LoadTiles(60);
			return new CompositeSpriteBatchDrawer[]
			{
				MakeTreeDrawer(tileSets, Main.rand.NextBool() ? 1 : 5),
				new ClutterDrawer(TextureAssets.Tile[61].Value,
					new int[] { Main.rand.Next(20), -1, -1, Main.rand.Next(20)},
					height: 20),
				new TileDrawer(TextureAssets.Tile[60].Value,  39)
			};
		}
		public static CompositeSpriteBatchDrawer[] HallowedTree()
		{
			int[] tileSets = { 3 };
			Main.instance.LoadTiles(110);
			Main.instance.LoadTiles(109);
			TreeDrawer drawer = MakeTreeDrawer(tileSets, 2, topFrames: 9, branchFrames: 9, minHeight: 2, maxHeight: 4);
			drawer.trunkHeadstartFrames = 3;
			return new CompositeSpriteBatchDrawer[]
			{
				drawer,
				new ClutterDrawer(TextureAssets.Tile[110].Value,
					new int[] { Main.rand.Next(20), -1, -1, Main.rand.Next(20)},
					height: 20),
				new TileDrawer(TextureAssets.Tile[109].Value,  39)
			};
		}

		public static CompositeSpriteBatchDrawer[] PalmTree()
		{
			Texture2D folliageTexture = TextureAssets.TreeTop[15].Value;
			Rectangle folliageBounds = new Rectangle(
				folliageTexture.Width / 3* Main.rand.Next(3), 0, 
				folliageTexture.Width / 3, folliageTexture.Height / 4);
			Main.instance.LoadTiles(323);
			Main.instance.LoadTiles(53);
			return new CompositeSpriteBatchDrawer[]
			{
				new PalmTreeDrawer(
					folliageTexture,
					TextureAssets.Tile[323].Value,
					null,
					folliageBounds,
					trunkHeight: Main.rand.Next(3, 6)),
				// TODO add starfish
				new TileDrawer(TextureAssets.Tile[53].Value,  39)
			};
		}
	}

}
