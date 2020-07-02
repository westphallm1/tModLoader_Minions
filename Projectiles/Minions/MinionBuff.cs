using Terraria;
using Terraria.ModLoader;
using System.Linq;

namespace DemoMod.Projectiles.Minions
{
    public abstract class MinionBuff : ModBuff
    {
        private int[] projectileTypes;
        public MinionBuff(params int[] projectileTypes)
        {
            this.projectileTypes = projectileTypes;
        }

		public override void SetDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			if (projectileTypes.Select(p=>player.ownedProjectileCounts[p]).Sum() > 0) {
				player.buffTime[buffIndex] = 18000;
			}
			else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
    }
}
