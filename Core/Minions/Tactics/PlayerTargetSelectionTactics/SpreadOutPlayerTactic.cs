using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics
{
	class SpreadOutPlayerTactic : PlayerTargetSelectionTactic
	{
		private Dictionary<int, NPC> enemyProjectileMatches;

		public SpreadOutPlayerTactic()
		{
			enemyProjectileMatches = new Dictionary<int, NPC>();
		}
		public override NPC ChooseTargetFromList(Projectile projectile, List<NPC> possibleTargets)
		{
			if(enemyProjectileMatches.ContainsKey(projectile.whoAmI))
			{
				return enemyProjectileMatches[projectile.whoAmI];
			}
			if(possibleTargets.Count == 0)
			{
				return default;
			}

			var usedEnemyIds = enemyProjectileMatches.Values;
			NPC selected = possibleTargets
				.OrderBy(npc => usedEnemyIds.Contains(npc) ? 1 : 0)
				.ThenBy(npc => Vector2.DistanceSquared(projectile.Center, npc.Center))
				.FirstOrDefault();
			enemyProjectileMatches[projectile.whoAmI] = selected;
			return selected;
		}

		public override void PreUpdate()
		{
			var toRemove = enemyProjectileMatches.Where(kv => !kv.Value.active).Select(kv=>kv.Key).ToArray();
			foreach(var remove in toRemove)
			{
				enemyProjectileMatches.Remove(remove);
			}
		}
	}
}
