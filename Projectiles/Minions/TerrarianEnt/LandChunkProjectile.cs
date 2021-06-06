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
	public class LandChunkProjectile : TransientMinion
	{
		public override string Texture => "Terraria/Item_0";
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
			drawers = LandChunkConfigs.templates[2]();
			drawFuncs = new SpriteCycleDrawer[drawers.Length];
			for(int i = 0; i < drawers.Length; i++)
			{
				drawFuncs[i] = drawers[i].Draw;
			}
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
			scHelper.Process(spriteBatch, lightColor, false, drawFuncs);
			return false;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
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
			templates = new Func<CompositeSpriteBatchDrawer[]>[] { Sunflowers, Statue, ForestTree };
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

		public static CompositeSpriteBatchDrawer[] ForestTree()
		{
			return new CompositeSpriteBatchDrawer[]
			{
				new TreeDrawer(
					GetTexture("Terraria/Tree_Tops_0"),
					GetTexture("Terraria/Tiles_5"),
					GetTexture("Terraria/Tree_Branches_0"),
					new Rectangle(82 * Main.rand.Next(3), 0, 82, 82),
					trunkHeight: Main.rand.Next(3, 5)),
				new ClutterDrawer(GetTexture("Terraria/Tiles_3"),
					new int[] { Main.rand.Next(10), -1, -1, Main.rand.Next(10)},
					height: 20),
				new TileDrawer(GetTexture("Terraria/Tiles_2"),  2)
			};

		}
	}
}
