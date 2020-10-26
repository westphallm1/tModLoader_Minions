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
    class PumpkinDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noLight = true;
            dust.alpha = 128;
            dust.scale = Main.rand.NextFloat()/2f + 0.25f;
            dust.rotation = Main.rand.NextFloat() * (float) Math.PI;
            dust.frame = new Rectangle(0, Main.rand.Next(3) * 16, 16, 16);
            dust.noGravity = false;
            dust.scale = 1;
            dust.alpha = 64;
        }
    }
}
