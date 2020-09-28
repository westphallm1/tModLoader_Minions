using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AmuletOfManyMinions.Projectiles.Minions;

namespace AmuletOfManyMinions.Projectiles.NonMinionSummons
{
    public abstract class TransientMinion : SimpleMinion<ModBuff>
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.minion = false;
            projectile.minionSlots = 0;
            useBeacon = false;
            ProjectileID.Sets.Homing[projectile.type] = true;
            ProjectileID.Sets.MinionShot[projectile.type] = true;
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = false;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[projectile.type] = false;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[projectile.type] = false;
			// Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
        }

        public override Vector2? FindTarget()
        {
            return null;
        }
        public override Vector2 IdleBehavior()
        {
            return Vector2.Zero;
        }

        public override void Behavior()
        {
            base.Behavior();
            projectile.tileCollide = true;
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            return;
        }

        public override void CheckActive()
        {
            // no-op
        }
    }
}
