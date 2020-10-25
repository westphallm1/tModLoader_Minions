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
    class StarDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.alpha = 128;
            dust.scale = Main.rand.NextFloat()/2f + 0.25f;
            dust.rotation = Main.rand.NextFloat() * (float) Math.PI;
            dust.frame = new Rectangle(0, Main.rand.Next(3) * 18, 14, 18);
        }

        public override bool Update(Dust dust)
        {
            dust.alpha -= 1;
            dust.position += dust.velocity;
            dust.rotation += 0.3f;
            if(dust.alpha < 96)
            {
                dust.active = false;
            }
            return false;
        }
    }
}
