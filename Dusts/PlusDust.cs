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
			dust.scale = 1.8f + Main.rand.NextFloat() / 5f;
			dust.rotation = 0;
			dust.frame = new Rectangle(0, 0, 10, 10);
			dust.noGravity = true;
			dust.alpha = 64;
		}

		public override bool Update(Dust dust)
		{
			dust.rotation = 0;
			return true;
		}
	}

	class MinusDust: PlusDust
	{

	}
}
