using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace AmuletOfManyMinions.Dusts
{
	class PlusDust : ModDust
	{
		public override void OnSpawn(Dust dust)
		{
			dust.noLight = true;
			dust.scale = 1f;
			dust.rotation = 0;
			dust.frame = new Rectangle(0, 0, 10, 10);
			dust.noGravity = true;
			dust.alpha = 196;
		}

		public override bool Update(Dust dust)
		{
			dust.rotation = 0;
			// stay above the head of the squire
			Projectile myProj = Main.projectile[(int)dust.customData];
			dust.velocity.X = myProj.velocity.X;
			dust.velocity.Y = myProj.velocity.Y;
			if(dust.alpha < 128)
			{
				dust.velocity.Y -= 0.1f;
			} else
			{
				dust.velocity.Y -= 0.5f;
				dust.velocity.Y -= (dust.dustIndex % 3) / 10f;
			}
			if(dust.alpha > 64)
			{
				dust.scale += 0.01f;
			} else
			{
				dust.scale -= 0.05f;
			}
			dust.alpha -= 4;
			if(dust.alpha <= 0)
			{
				dust.active = false;
			}
			dust.position += dust.velocity;
			return false;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			Color glowMask = Color.White;
			glowMask.A = (byte)dust.alpha;
			return glowMask;
		}
	}
	

	class MinusDust: PlusDust
	{

	}
}
