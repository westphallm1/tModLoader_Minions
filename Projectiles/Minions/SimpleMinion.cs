using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions
{
    public abstract class SimpleMinion<T> : Minion<T> where T : ModBuff
    {
		Vector2 vectorToIdle;
		Vector2? targetVector;
		public override void SetStaticDefaults() 
		{
            base.SetDefaults();
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
			// Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
			ProjectileID.Sets.Homing[projectile.type] = true;
		}

        public override void SetDefaults()
        {
            base.SetDefaults();
			// These below are needed for a minion weapon
			// Only controls if it deals damage to enemies on contact (more on that later)
			projectile.friendly = true;
			// Only determines the damage type
			projectile.minion = true;
			// Amount of slots this minion occupies from the total minion slots available to the player (more on that later)
			projectile.minionSlots = 1f;
			// Needed so the minion doesn't despawn on collision with enemies or tiles
			projectile.penetrate = -1;
        }

		public abstract Vector2 IdleBehavior();
		public abstract Vector2? FindTarget();
		public abstract void IdleMovement(Vector2 idlePosition);
		public abstract void TargetedMovement(Vector2 targetPosition);

		public virtual void Animate() {

			// This is a simple "loop through all frames from top to bottom" animation
			int frameSpeed = 5;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed) {
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= Main.projFrames[projectile.type]) {
					projectile.frame = 0;
				}
			}
		}

        public override void Behavior()
        {
			vectorToIdle = IdleBehavior();
			targetVector = FindTarget();
			if(targetVector is Vector2 targetPosition)
            {
				TargetedMovement(targetPosition);
            } else
            {
				IdleMovement(vectorToIdle);
            }
			Animate();
        }
    }
}
