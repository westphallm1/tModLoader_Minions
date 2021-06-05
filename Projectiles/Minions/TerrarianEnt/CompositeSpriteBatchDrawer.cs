using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt
{
	public abstract class CompositeSpriteBatchDrawer
	{
		// these need to be injected internally each frame, a bit annoying
		internal int animationFrame;
		internal int spawnFrames;


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
			if (dustId > - 1 && animationFrame <= spawnFrames && animationFrame % (spawnFrames / 4) == 0)
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
				int yOffset = Math.Min(30, 2 * (animationFrame - spawnFrames - 4 * i));
				if (yOffset < 0)
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
			int heightOffset = Math.Min(16 + 8 * monumentBounds.Height, growthRate * (animationFrame - spawnFrames) - 32);
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
		// in tiles, not pixels
		internal int trunkHeight = 5;
		internal int folliageSpawnFrames = 20;
		int treeTileSize = 20;
		internal (byte, byte)?[,,] tiles;

		public TreeDrawer(Texture2D folliageTexture, Texture2D woodTexture, Rectangle folliageBounds, int trunkHeight = 5)
		{
			this.folliageTexture = folliageTexture;
			this.woodTexture = woodTexture;
			this.folliageBounds = folliageBounds;
			this.trunkHeight = trunkHeight;
			tiles = new (byte, byte)?[1, trunkHeight, 3];
		}

		internal void DrawTreeTop(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			int heightPerFrame = folliageBounds.Height / folliageSpawnFrames;
			int animFrame = animationFrame - spawnFrames;
			int heightToDraw = Math.Min(folliageBounds.Height, animFrame * 2 * heightPerFrame);

			float scale = Math.Max(0.5f, heightToDraw / (float)folliageBounds.Height);
			int maxHeight = treeTileSize * trunkHeight + folliageBounds.Height/2;
			int heightOffset = Math.Min(maxHeight, heightPerFrame * animFrame);
			Rectangle bounds = new Rectangle(folliageBounds.X, folliageBounds.Y, folliageBounds.Width, heightToDraw);
			helper.AddSpriteToBatch(folliageTexture, bounds, new Vector2(1, 2 - heightOffset) * scale, 0, scale);
		}

		internal void DrawTrunk(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			if (animationFrame < spawnFrames + folliageSpawnFrames)
			{
				return;
			}
			int animFrame = animationFrame - spawnFrames - folliageSpawnFrames;
			int heightPerFrame = folliageBounds.Height / folliageSpawnFrames;
			int heightToDraw = (int)Math.Min(trunkHeight-1, animFrame * heightPerFrame / (float)treeTileSize);
			int heightOffset = Math.Min(treeTileSize/2 * trunkHeight, heightPerFrame * animFrame);
			tiles[0, heightToDraw, 1] = (0, (byte)(heightToDraw%3));
			helper.AddTileSpritesToBatch(woodTexture, 0, tiles, Vector2.UnitY * -heightOffset, tileSize: treeTileSize);
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
			// TODO
		}

	}

}
