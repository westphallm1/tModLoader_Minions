using AmuletOfManyMinions.Core;
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

		public int attackFrames = 60;
		public int attackFrame
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public List<Projectile> GetActiveMinions() => Behavior.GetActiveMinions();

		public List<Projectile> GetAllMinionsOwnedByPlayer() => Behavior.GetAllMinionsOwnedByPlayer();

		public Projectile GetHead(int headType) => Behavior.GetHead(headType);

		public Projectile GetFirstMinion(List<Projectile> others = null) => Behavior.GetFirstMinion(others);

		public void DistanceFromGroup(ref Vector2 distanceToTarget, int separation = 16, int closeDistance = 32)
			=> Behavior.DistanceFromGroup(ref distanceToTarget, separation, closeDistance);

		public override Vector2 IdleBehavior()
		{
			if(IsPrimaryFrame)
			{
				attackFrame = (attackFrame + 1) % attackFrames;
			}
			return default;
		}

		public bool IsMyTurn()
		{
			if (Player.ownedProjectileCounts[Projectile.type] == 1)
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
			int order = minions.IndexOf(Projectile);
			int attackFrame = order * (attackFrames / minions.Count);
			int currentFrame = (int)leader.ai[0];
			return currentFrame == attackFrame;
		}

		protected Vector2? FindTargetInTurnOrder(float searchDistance, Vector2 center, float noLOSDistance = 0)
		{
			if (AttackState == AttackState.RETURNING)
			{
				return null;
			}
			else if (AttackState == AttackState.IDLE && !IsMyTurn())
			{
				return null;
			}
			if (PlayerTargetPosition(searchDistance, center, noLOSDistance) is Vector2 target)
			{
				AttackState = AttackState.ATTACKING;
				return target - Projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance, noLOSDistance) is Vector2 target2)
			{
				AttackState = AttackState.ATTACKING;
				return target2 - Projectile.Center;
			}
			else
			{
				return null;
			}

		}

	}
}
