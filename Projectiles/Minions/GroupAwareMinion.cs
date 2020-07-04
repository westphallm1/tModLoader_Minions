using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DemoMod.Projectiles.Minions
{

    public enum AttackState
    {
        IDLE,
        ATTACKING,
        RETURNING
    }
    public abstract class GroupAwareMinion<T> : SimpleMinion<T> where T : ModBuff
    {

        public int attackFrames = 60;
        public AttackState attackState = AttackState.IDLE;

        public override void SetDefaults()
        {
            base.SetDefaults();
			projectile.ai[0] = 0;
        }

        public List<Projectile> GetActiveMinions()
        {
            return GetMinionsOfType(projectile.type);
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

		public Projectile GetFirstMinion(List<Projectile> others = null)
        {
			return (others ?? GetActiveMinions())
				.Where(p => p.minionPos == others.Min(p2 => p2.minionPos)).FirstOrDefault();
        }

        public override Vector2 IdleBehavior()
        {
            projectile.ai[0] = (projectile.ai[0] + 1) % attackFrames;
            return default;
        }

        public bool IsMyTurn()
        {
            var minions = GetActiveMinions();
            var leader = GetFirstMinion(minions);
            int order = projectile.minionPos - leader.minionPos;
            int attackFrame = order * (attackFrames / minions.Count);
            int currentFrame = (int)leader.ai[0];
            return currentFrame == attackFrame;
        }

        protected Vector2? FindTargetInTurnOrder(float searchDistance, Vector2 center)
        {
            if(attackState == AttackState.RETURNING)
            {
                return null;
            }
            else if (attackState == AttackState.IDLE && !IsMyTurn())
            {
                return null;
            }
            if (PlayerTargetPosition(searchDistance, center) is Vector2 target)
            {
                attackState = AttackState.ATTACKING;
                return target - projectile.Center;
            }
            else if (ClosestEnemyInRange(searchDistance, center) is Vector2 target2)
            {
                attackState = AttackState.ATTACKING;
                return target2 - projectile.Center;
            }
            else
            {
                return null;
            }

        }

    }
}
