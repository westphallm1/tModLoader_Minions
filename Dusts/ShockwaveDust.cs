using AmuletOfManyMinions.Projectiles.Minions.StoneCloud;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Dusts
{
	class ShockwaveDust : ModDust
	{
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = true;
		}

		public override bool Update(Dust dust)
		{
			if (dust.alpha > StoneCloudMinion.ShockwaveDustAlpha + StoneCloudMinion.ShockwaveMaxSpeedFrames)
			{
				dust.velocity *= StoneCloudMinion.ShockwaveDecay;
			}
			if (dust.alpha < StoneCloudMinion.ShockwaveDustAlpha + StoneCloudMinion.ShockwaveTotalFrames)
			{
				dust.alpha += 1;
			}
			else
			{
				dust.active = false;
			}
			if (dust.alpha % 10 < 5)
			{
				dust.scale += 0.1f;
			}
			else
			{
				dust.scale -= 0.1f;
			}
			dust.position += dust.velocity;
			int x = (int)dust.position.X / 16;
			int y = (int)dust.position.Y / 16;
			Tile tile = Framing.GetTileSafely(x, y);
			if (tile.HasTile && tile.BlockType == BlockType.Solid)
			{
				dust.active = false;
			}
			return false;
		}
	}
}
