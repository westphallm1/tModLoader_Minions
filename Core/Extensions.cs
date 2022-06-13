using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core
{
	public static class Vector2Extensions
	{
		public static void SafeNormalize(this ref Vector2 vec)
		{
			if (vec != Vector2.Zero)
			{
				vec.Normalize();
			}
		}
	}

	public static class ModProjectileExtensions
	{
		public static void ClientSideNPCHitCheck(this ModProjectile modProjectile)
		{
			if (modProjectile.Projectile.owner == Main.myPlayer ||
				Minion.GetClosestEnemyToPosition(modProjectile.Projectile.Center, 128, requireLOS: false) is not NPC npc)
			{
				return;
			}
			if (modProjectile.Projectile.Hitbox.Intersects(npc.Hitbox))
			{
				modProjectile.OnHitNPC(npc, 0, 0, false);
			}
		}

		public static bool BounceOnCollision(this Projectile proj, Vector2 oldVelocity)
		{
			// bounce off walls while coasting after hitting enemies
			if (Math.Abs(proj.velocity.Y) < Math.Abs(oldVelocity.Y))
			{
				proj.velocity.Y = -oldVelocity.Y;
			} else if (Math.Abs(proj.velocity.X) < Math.Abs(oldVelocity.X))
			{
				proj.velocity.X = -oldVelocity.X;
			} else
			{
				// don't really understand what's going on in this case but that's ok
				return false; 
			}
			return false;
		}
	}

	public static class RectangleExtensions
	{
		public static Vector2 GetOrigin(this Rectangle rect)
		{
			return new(rect.Width / 2, rect.Height / 2);
		}
	}
}

