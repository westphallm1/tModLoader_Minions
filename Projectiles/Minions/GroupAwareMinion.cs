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

        private List<Projectile> others = null;
        private Projectile leader = null;
        private Projectile head = null;
        public int attackFrames = 60;
        public int animationFrames = 120;
        public AttackState attackState = AttackState.IDLE;

        public override void SetDefaults()
        {
            base.SetDefaults();
			projectile.ai[0] = 0;
			projectile.ai[1] = 0;
        }

        public List<Projectile> GetActiveMinions()
        {
            if(others == null)
            {
                others = GetMinionsOfType(projectile.type);
            }
            return others;
        }

        public Projectile GetHead(int headType)
        {
            if (head == null)
            {
                head = GetMinionsOfType(headType).FirstOrDefault();
            }
            return head;
        }

		public Projectile GetFirstMinion(List<Projectile> others = null)
        {
            if(leader == null)
            {
                leader = (others ?? GetActiveMinions()).FirstOrDefault();

            }
            return leader;   
        }

        public override Vector2 IdleBehavior()
        {
            leader = null;
            others = null;
            head = null;
            projectile.ai[0] = (projectile.ai[0] + 1) % attackFrames;
            projectile.ai[1] = (projectile.ai[1] + 1) % animationFrames;
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
