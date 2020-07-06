using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace DemoMod.Projectiles.Minions
{
	public abstract class Minion<T>  : ModProjectile where T: ModBuff
	{
        public readonly float PI = (float)Math.PI;

        public Player player;

		protected int buffType;
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
			buffType = BuffType<T>();
        }
        public override void AI() {
			player = Main.player[projectile.owner];
			CheckActive();
			Behavior();
		}

		public void CheckActive() {
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(buffType);
			}
			if (player.HasBuff(buffType)) {
				projectile.timeLeft = 2;
			}
		}


		public abstract void Behavior();
	}
}