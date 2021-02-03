using System.Collections.Generic;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions
{
	internal class IdleLocationSets
	{
		public static HashSet<int> trailingOnGround;
		public static HashSet<int> trailingInAir;
		public static HashSet<int> trailingAboveHead;
		public static HashSet<int> circlingBody;
		public static HashSet<int> circlingHead;

		public static void Load()
		{
			trailingOnGround = new HashSet<int>();
			trailingInAir = new HashSet<int>();
			trailingAboveHead = new HashSet<int>();
			circlingBody = new HashSet<int>();
			circlingHead = new HashSet<int>();
		}
		public static void Unload()
		{
			trailingOnGround = null;
			trailingInAir = null;
			trailingAboveHead = null;
			circlingBody = null;
			circlingHead = null;
		}

		public static List<Projectile> GetProjectilesInSet(HashSet<int> matchingSet, int ownerId)
		{
			var otherMinions = new List<Projectile>();
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (other.active && other.owner == ownerId && matchingSet.Contains(other.type))
				{
					otherMinions.Add(other);
				}
			}
			otherMinions.Sort((x, y) => x.minionPos - y.minionPos);
			return otherMinions;
		}

		public static int GetXOffsetInSet(List<Projectile> projectiles, Projectile self, int spacing = 4)
		{
			int offset = 0;
			foreach (Projectile proj in projectiles)
			{
				// minion hitboxes are usually a bit smaller than the texture to fit in 2x2 blocks,
				// so include extra spacing with each offset
				offset += spacing + proj.width;
				if (proj.whoAmI == self.whoAmI)
				{
					return offset;
				}
			}
			return offset;

		}
		public static int GetXOffsetInSet(HashSet<int> matchingSet, Projectile self, int spacing = 4)
		{
			return GetXOffsetInSet(GetProjectilesInSet(matchingSet, self.owner), self, spacing);
		}
	}
}
