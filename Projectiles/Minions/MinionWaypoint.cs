using AmuletOfManyMinions.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions
{
	class MinionWaypoint : ModProjectile
	{
		public const int duration = 180000; // a long time
		private int rotationFrame = 0;
		public const int rotationFrames = 60;

		public static int Type => ProjectileType<MinionWaypoint>();

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.damage = 0;
			projectile.width = 1;
			projectile.height = 1;
			projectile.tileCollide = false;
			projectile.timeLeft = duration;
			projectile.friendly = false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return false;
		}

		public override void AI()
		{
			rotationFrame = (rotationFrame + 1) % rotationFrames;
			float startAngle = -2f * (float)Math.PI * rotationFrame / rotationFrames;
			for (int i = 0; i < 3; i++)
			{
				float angle = startAngle + i * 2 * (float)Math.PI / 3;
				Vector2 pos = projectile.Center + 12 * new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));
				Dust.NewDust(pos, 1, 1, DustType<MinionWaypointDust>(), newColor: new Color(0.5f, 1, 0.5f), Scale: 1.2f);
			}
		}

		// doesn't matter, never drawn
		public override string Texture => "Terraria/NPC_0";
	}
}
