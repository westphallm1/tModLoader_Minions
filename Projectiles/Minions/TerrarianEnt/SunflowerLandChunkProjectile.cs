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

namespace AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt
{
	public class LandChunkProjectile : TransientMinion
	{
		public override string Texture => "Terraria/Item_0";
		internal SpriteCompositionHelper scHelper;

		internal CompositeSpriteBatchDrawer[] drawers;


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
			projectile.rotation = 0;
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
			drawers = LandChunkConfigs.templates[1]();
		}

		public override void AI()
		{
			if(animationFrame == 0)
			{
				OnSpawn();
			}
			animationFrame++;
			Player player = Main.player[projectile.owner];

			// TODO replace with real behavior
			if (animationFrame < 90)
			{
				int xOffset = projectile.ai[0] == 0 ? 64 : -96;
				projectile.position = player.Center + new Vector2(xOffset, 8 * (float)Math.Sin(MathHelper.TwoPi * projectile.timeLeft / 60));
				projectile.velocity = Vector2.Zero;
			} else
			{
				// "pretend" to fly away for now
				int maxSpeed = 8;
				int currentSpeed = Math.Min(maxSpeed, animationFrame - 90);
				projectile.velocity = new Vector2(currentSpeed, 0);
				projectile.rotation += (currentSpeed / (float)maxSpeed) * MathHelper.Pi / 8;
			}
			Array.ForEach(drawers, d => d.Update(projectile, animationFrame, spawnFrames));
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			lightColor = Color.White * 0.75f;
			scHelper.Process(spriteBatch, lightColor, false, drawers.Select<CompositeSpriteBatchDrawer, SpriteCycleDrawer>(d=>d.Draw).ToArray());
			return false;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// No-op
		}
	}

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

		public MonumentDrawer(Texture2D texture, Rectangle bounds, int growthRate = 4)
		{
			monumentTexture = texture;
			monumentBounds = bounds;
			this.growthRate = growthRate;
		}
		internal override void Draw(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			if (animationFrame < spawnFrames)
			{
				return;
			}
			(byte, byte)?[,,] tiles = new (byte, byte)?[1, monumentBounds.Height, monumentBounds.Width];
			int heightToDraw = Math.Min(monumentBounds.Height, (animationFrame - spawnFrames - growthRate) / growthRate);
			int heightOffset = Math.Min(16 + 8 * monumentBounds.Height, growthRate * (animationFrame - spawnFrames) - 32);
			// programmatic build here feels a bit iffy
			for (int i = 0; i < heightToDraw; i++)
			{
				for(int j = 0; j < monumentBounds.Width; j++)
				{
					tiles[0, i, j] = ((byte)(monumentBounds.X + j), (byte)(monumentBounds.Y + i));
				}
			}
			helper.AddTileSpritesToBatch(monumentTexture, 0, tiles, Vector2.UnitY * -heightOffset, 0);
		}
	}


	public class LandChunkConfigs
	{

		public static Func<CompositeSpriteBatchDrawer[]>[] templates;
		public static void Load()
		{
			// TODO some assembly stuff to autoload these
			templates = new Func<CompositeSpriteBatchDrawer[]>[] { Sunflowers, Statue };
		}

		public static void Unload()
		{
			templates = null;
		}

		public static CompositeSpriteBatchDrawer[] Sunflowers()
		{
			return new CompositeSpriteBatchDrawer[]
			{
				new MonumentDrawer(ModContent.GetTexture("Terraria/Tiles_27"), new Rectangle(2 * Main.rand.Next(3), 0, 2, 4)),
				new ClutterDrawer(
					ModContent.GetTexture("Terraria/Tiles_3"), 
					Enumerable.Repeat(0, 4).Select(_ => Main.rand.Next(10)).ToArray(), 
					height: 20),
				new TileDrawer(ModContent.GetTexture("Terraria/Tiles_2"),  2)
			};
		}
		public static CompositeSpriteBatchDrawer[] Statue()
		{
			int tileTexture = Main.rand.NextBool() ? 179 : 1;
			return new CompositeSpriteBatchDrawer[]
			{
				new MonumentDrawer(ModContent.GetTexture("Terraria/Tiles_105"), new Rectangle(2* Main.rand.Next(20), 0, 2, 3)),
				// no clutter
				new TileDrawer(ModContent.GetTexture("Terraria/Tiles_" + tileTexture), 1)
			};
		}
	}
}
