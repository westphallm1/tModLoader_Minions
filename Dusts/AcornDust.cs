using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

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
