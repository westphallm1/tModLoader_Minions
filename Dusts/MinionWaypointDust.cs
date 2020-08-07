using DemoMod.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace DemoMod.Dusts
{
    class MinionWaypointDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale = 1.2f;
        }

        public override bool Update(Dust dust)
        {
            dust.scale -= .7f / (MinionWaypoint.rotationFrames/3f - 1);
            if(dust.scale < 0.5)
            {
                dust.active = false;
            }
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(dust.color.R, dust.color.G, dust.color.B, dust.scale * 0.5f);
        }
    }
}
