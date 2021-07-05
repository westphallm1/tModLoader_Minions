using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Dusts
{
	class SnowDust : ModDust
	{

		public override bool Autoload(ref string name, ref string texture)
		{
			texture = "Terraria/Projectile_" + ProjectileID.NorthPoleSnowflake;
			return base.Autoload(ref name, ref texture);
		}
		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, Main.rand.Next(2) * 26, 26, 26);
			dust.noGravity = true;
			dust.noLight = true;
			dust.color = Color.White;
		}

		public override bool Update(Dust dust)
		{
			dust.scale *= 0.99f;
			if(dust.scale < 0.2f)
			{
				dust.active = false;
			}
			return true;
		}
	}
}
