﻿using AmuletOfManyMinions.Core.Minions.Pathfinding;
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
			Projectile.damage = 0;
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = duration;
			Projectile.friendly = false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		internal Color GetWaypointColor(MinionPathfindingPlayer player)
		{
			bool isMyPlayer = Main.myPlayer == player.Player.whoAmI;
			bool suceeded = isMyPlayer && player.GetPathfinder((int)Projectile.ai[0]).searchSucceeded && player.InWaypointRange(Projectile.Center);
			if(isMyPlayer)
			{
				if(!suceeded)
				{
					return Color.Gray;
				}
				bool isActive = player.CurrentTacticGroup == Projectile.ai[0] || player.CurrentTacticGroup == 2;
				Color color = MinionPathfindingPlayer.WaypointColors[(int)Projectile.ai[0]];
				if(isActive)
				{
					return color;
				}
				else
				{
					return Color.Multiply(color, 0.5f);
				}

			} else
			{
				return (Projectile.ai[0] == 0 ? Color.Aquamarine : Color.Lavender) * 0.5f;
			}
		}

		public override void AI()
		{
			rotationFrame = (rotationFrame + 1) % rotationFrames;
			float startAngle = -2f * (float)Math.PI * rotationFrame / rotationFrames;
			MinionPathfindingPlayer player = Main.player[Projectile.owner].GetModPlayer<MinionPathfindingPlayer>();
			bool isMyPlayer = Main.myPlayer == player.Player.whoAmI;
			BlockAwarePathfinder pathfinder = player.GetPathfinder((int)Projectile.ai[0]);
			if(pathfinder.searchSucceeded || !pathfinder.searchFailed)
			{

				int radius = isMyPlayer ? pathfinder.searchSucceeded ? 12 : 6 : 8;
				Color color = GetWaypointColor(player);
				float scale = isMyPlayer ?  pathfinder.searchSucceeded ? 1.2f : 0.8f : 1f; 
				for (int i = 0; i < 3; i++)
				{
					float angle = startAngle + i * 2 * (float)Math.PI / 3;
					Vector2 pos = Projectile.Center + radius * angle.ToRotationVector2();
					Dust.NewDust(pos, 1, 1, DustType<MinionWaypointDust>(), newColor: color, Scale: scale);
				}
			} else if (pathfinder.searchFailed)
			{
				for(int i = 0; i < 2; i++)
				{
					float offset = 12 * (i == 0 ? (float)Math.Sin(startAngle) : (float)Math.Cos(startAngle));
					Vector2 pos = Projectile.Center + new Vector2(i == 1 ? offset : -offset, offset);
					Dust.NewDust(pos, 1, 1, DustType<MinionWaypointDust>(), newColor: Color.Red, Scale: 1.2f);
				}
			}
		}

		// doesn't matter, never drawn
		public override string Texture => "Terraria/Images/NPC_0";
	}
}
