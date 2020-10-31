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
    class BucketDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noLight = true;
            dust.scale = Main.rand.NextFloat()/2f + 0.25f;
            dust.frame = new Rectangle(0, Main.rand.Next(3) * 26, 22, 26);
            dust.noGravity = false;
            dust.customData = Main.rand.NextFloat() * (float) Math.PI;
            dust.scale = 1;
            dust.alpha = 64;
        }

        public override bool Update(Dust dust)
        {
            dust.rotation = (float)dust.customData;
            return true;
        }
    }
}
