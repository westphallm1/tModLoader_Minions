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
		private SpriteCompositionHelper scHelper;

		private Texture2D groundTexture;
		private Texture2D plantsTexture;
		private Texture2D sunflowerTexture;

		private int spawnFrames = 30;

		private (byte, byte)?[,,] tiles =
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

		private int[] plantFrames;

		private int sunflowerVariant;

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
			groundTexture = ModContent.GetTexture("Terraria/Tiles_2");
			plantsTexture = ModContent.GetTexture("Terraria/Tiles_3");
			sunflowerTexture = ModContent.GetTexture("Terraria/Tiles_27");
			scHelper = new SpriteCompositionHelper(this)
			{
				idleCycleFrames = 160,
				frameResolution = 1,
				posResolution = 1
			};
		}

		public override void AI()
		{
			if(plantFrames == null)
			{
				plantFrames = Enumerable.Repeat(0, 4).Select(_ => Main.rand.Next(10)).ToArray();
				sunflowerVariant = 2 * Main.rand.Next(3);

			}
			animationFrame++;
			if (animationFrame <= spawnFrames && animationFrame % (spawnFrames /4) == 0)
			{
				for(int i = 0; i < 3; i++)
				{
					Dust.NewDust(projectile.TopLeft, 24, 24, 2, 0, 0);
				}
			}
			Player player = Main.player[projectile.owner];
			if(animationFrame < 90)
			{
				int xOffset = projectile.ai[0] == 0 ? 64 : -96;
				projectile.position = player.Center + new Vector2(xOffset, 8 * (float) Math.Sin(MathHelper.TwoPi * projectile.timeLeft / 60));
				projectile.velocity = Vector2.Zero;
			} else
			{
				// "pretend" to fly away for now
				int maxSpeed = 8;
				int currentSpeed = Math.Min(maxSpeed, animationFrame - 90);
				projectile.velocity = new Vector2(currentSpeed, 0);
				projectile.rotation += (currentSpeed / (float)maxSpeed) * MathHelper.Pi / 8;
			}
		}

		private void DrawBase(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			int tileFrame = animationFrame > spawnFrames ? 4 : animationFrame / (spawnFrames / 4); 
			helper.AddTileSpritesToBatch(groundTexture, tileFrame, tiles,  Vector2.Zero, 0);
		}

		private void DrawPlants(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			if(animationFrame < spawnFrames)
			{
				return;
			}
			for(int i = 0; i < plantFrames.Length; i++)
			{
				int yOffset = Math.Min(30, 2 * (animationFrame - spawnFrames - 4 * i));
				if(yOffset < 0)
				{
					continue;
				}
				Vector2 offset = new Vector2(-24 + 16 * i, -yOffset);
				int idx = plantFrames[i];
				Rectangle bounds = new Rectangle(18 * idx, 0, 16, 20);
				helper.AddSpriteToBatch(plantsTexture, bounds, offset, 0, 1);
			}
		}

		private void DrawSunflower(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			if(animationFrame < spawnFrames)
			{
				return;
			}
			(byte, byte)?[,,] tiles = new (byte, byte)?[1, 4, 2];
			int heightToDraw = Math.Min(4, (animationFrame - spawnFrames - 4) / 4);
			int heightOffset = Math.Min(48, 4 * (animationFrame - spawnFrames) - 32);
			// programmatic build here feels a bit iffy
			for(int i = 0; i < heightToDraw; i++)
			{
				tiles[0, i, 0] = ((byte) sunflowerVariant, (byte)i);
				tiles[0, i, 1] = ((byte) (sunflowerVariant + 1), (byte)i);
			}
			helper.AddTileSpritesToBatch(sunflowerTexture, 0, tiles,  Vector2.UnitY * -heightOffset, 0);
			
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			lightColor = Color.White * 0.75f;
			scHelper.Process(spriteBatch, lightColor, false, DrawSunflower, DrawPlants, DrawBase);
			return false;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// No-op
		}
	}
}
