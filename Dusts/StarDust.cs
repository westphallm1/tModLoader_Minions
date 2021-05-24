using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Dusts
{
	class StarDust : ModDust
	{
		public override void OnSpawn(Dust dust)
		{
			dust.noLight = true;
			dust.alpha = 128;
			dust.scale = Main.rand.NextFloat() / 2f + 0.25f;
			dust.rotation = Main.rand.NextFloat() * (float)Math.PI;
			dust.frame = new Rectangle(0, Main.rand.Next(3) * 18, 14, 18);
		}

		public override bool Update(Dust dust)
		{
			dust.alpha -= 2;
			dust.position += dust.velocity;
			dust.scale *= 0.95f;
			if(!dust.noGravity && dust.velocity.Y < 16)
			{
				dust.velocity.Y += 0.5f;
			}
			//dust.rotation += 0.3f;
			if (dust.alpha < 96 || dust.scale < 0.25f)
			{
				dust.active = false;
			}
			return false;
		}
	}
}
