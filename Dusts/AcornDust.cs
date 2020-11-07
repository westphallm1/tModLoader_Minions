using Microsoft.Xna.Framework;
using Terraria;

namespace AmuletOfManyMinions.Dusts
{
	class AcornDust : PumpkinDust
	{
		public override void OnSpawn(Dust dust)
		{
			base.OnSpawn(dust);
			dust.frame = new Rectangle(0, Main.rand.Next(2) * 14, 12, 14);
		}
	}
}
