using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DemoMod.Projectiles.Minions
{
    public abstract class SimpleMinion<T> : Minion<T> where T : ModBuff
    {
		protected Vector2 vectorToIdle;
		protected Vector2? vectorToTarget;
		protected Vector2 oldVectorToIdle;
		protected Vector2? oldVectorToTarget = null;
		protected int framesSinceHadTarget = 0;
		public override void SetStaticDefaults() 
		{
            base.SetStaticDefaults();
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
			// Makes the minion not go through tiles
			projectile.tileCollide = true;
        }


		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage() {
			return true;
		}

		public abstract Vector2 IdleBehavior();
		public abstract Vector2? FindTarget();
		public abstract void IdleMovement(Vector2 vectorToIdlePosition);
		public abstract void TargetedMovement(Vector2 vectorToTargetPosition);

		public virtual void AfterMoving() { }
		public virtual void Animate(int minFrame = 0, int? maxFrame = null) {

			// This is a simple "loop through all frames from top to bottom" animation
			int frameSpeed = 5;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed) {
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= (maxFrame ?? Main.projFrames[projectile.type])) {
					projectile.frame = minFrame;
				}
			}
		}

        public override void Behavior()
        {
            targetNPCIndex = null;
			vectorToIdle = IdleBehavior();
			vectorToTarget = FindTarget();
			if(vectorToTarget is Vector2 targetPosition)
            {
				TargetedMovement(targetPosition);
                oldVectorToTarget = vectorToTarget;
            } else if(oldVectorToTarget is Vector2 oldTarget && framesSinceHadTarget++ < 30)
            {
				oldVectorToTarget = oldTarget - projectile.velocity;
				TargetedMovement(oldTarget); // don't immediately give up if losing LOS
            } else
            {
				IdleMovement(vectorToIdle);
            }
			AfterMoving();
			Animate();
			oldVectorToIdle = vectorToIdle;
        }


		// utility methods
		public void TeleportToPlayer(Vector2 vectorToIdlePosition, float maxDistance)
        {
			if(Main.myPlayer == player.whoAmI && vectorToIdlePosition.Length() > maxDistance)
            {
				projectile.position += vectorToIdlePosition;
				projectile.velocity = Vector2.Zero;
				projectile.netUpdate = true;
            }
        }


        public List<Projectile> GetMinionsOfType(int projectileType)
        {
			var otherMinions = new List<Projectile>();
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (other.active && other.owner == projectile.owner && other.type == projectileType )
				{
					otherMinions.Add(other);
				}
			}
            otherMinions.Sort((x, y)=>x.minionPos - y.minionPos);
			return otherMinions;
        }
    }
}
