using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Projectiles.Minions
{

	public enum AttackState
	{
		IDLE,
		ATTACKING,
		RETURNING
	}

	/// <summary>
	/// Uses ai[0] for attack frames
	/// </summary>
	public abstract class GroupAwareMinion : SimpleMinion
	{

		private List<Projectile> others = null;
		private Projectile leader = null;
		private Projectile head = null;
		public int attackFrames = 60;
		public int attackFrame
		{
			get => (int)projectile.ai[0];
			set => projectile.ai[0] = value;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
		}

		public List<Projectile> GetActiveMinions()
		{
			if (others == null)
			{
				others = GetMinionsOfType(projectile.type);
			}
			return others;
		}

		public List<Projectile> GetAllMinionsOwnedByPlayer()
		{
			return Main.projectile
				.Where(p => p.active && p.owner == projectile.owner && (p.minion || ProjectileID.Sets.MinionShot[p.type]))
				.ToList();
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
			if (leader == null)
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
			attackFrame = (attackFrame + 1) % attackFrames;
			return default;
		}

		public bool IsMyTurn()
		{
			if (player.ownedProjectileCounts[projectile.type] == 1)
			{
				// don't obey cycle if only one minion
				return true;
			}
			var minions = GetActiveMinions();
			if(minions.Count == 0)
			{
				return false;
			}
			var leader = GetFirstMinion(minions);
			int order = minions.IndexOf(projectile);
			int attackFrame = order * (attackFrames / minions.Count);
			int currentFrame = (int)leader.ai[0];
			return currentFrame == attackFrame;
		}

		public void DistanceFromGroup(ref Vector2 distanceToTarget, int separation = 16, int closeDistance = 32)
		{
			// if multiple minions are gathered on a target, space them out a little bit
			if (distanceToTarget.Length() < closeDistance)
			{
				foreach (Projectile otherMinion in GetAllMinionsOwnedByPlayer())
				{
					if (otherMinion.whoAmI == projectile.whoAmI)
					{
						continue;
					}
					if (projectile.Hitbox.Intersects(otherMinion.Hitbox))
					{
						Vector2 difference = otherMinion.Center - projectile.Center;
						difference.SafeNormalize();
						distanceToTarget += -separation * difference;
					}
				}
			}

		}

		protected Vector2? FindTargetInTurnOrder(float searchDistance, Vector2 center, float noLOSDistance = 0)
		{
			if (attackState == AttackState.RETURNING)
			{
				return null;
			}
			else if (attackState == AttackState.IDLE && !IsMyTurn())
			{
				return null;
			}
			if (PlayerTargetPosition(searchDistance, center, noLOSDistance) is Vector2 target)
			{
				attackState = AttackState.ATTACKING;
				return target - projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance, noLOSDistance) is Vector2 target2)
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
