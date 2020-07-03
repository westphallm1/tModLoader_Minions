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
			vectorToIdle = IdleBehavior();
			targetVector = FindTarget();
			if(targetVector is Vector2 targetPosition)
            {
				TargetedMovement(targetPosition);
            } else
            {
				IdleMovement(vectorToIdle);
            }
			AfterMoving();
			Animate();
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

		public Vector2? PlayerTargetPosition(float maxRange)
        {
			if(player.HasMinionAttackTargetNPC)
            {
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				if(Vector2.Distance(npc.Center, projectile.Center) < maxRange)
                {
					return npc.Center;
                }
            }
			return null;
        }

		public Vector2? ClosestEnemyInRange(float maxRange)
        {
			Vector2 targetCenter = projectile.position;
			bool foundTarget = false;
			for(int i = 0; i < Main.maxNPCs; i++)
            {
				NPC npc = Main.npc[i];
				if(!npc.CanBeChasedBy())
                {
					continue;
                }
                float between = Vector2.Distance(npc.Center, projectile.Center);
                bool closest = Vector2.Distance(projectile.Center, targetCenter) > between;
				// don't let a minion infinitely chain attacks off progressively further enemies
                bool inRange = Vector2.Distance(npc.Center, player.Center) < maxRange;
                bool lineOfSight = Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height);
				if(lineOfSight && inRange && (closest || !foundTarget))
                {
					targetCenter = npc.Center;
					foundTarget = true;
                }
            }
			return foundTarget ? targetCenter : (Vector2?)null;
        }
    }
}
