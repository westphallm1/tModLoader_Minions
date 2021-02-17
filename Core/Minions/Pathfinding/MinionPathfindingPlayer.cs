using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.Pathfinding
{
	internal class MinionPathfindingPlayer : ModPlayer
	{
		internal PathfindingHelper pHelper;
		public override void OnEnterWorld(Player player)
		{
			base.OnEnterWorld(player);
			pHelper = new PathfindingHelper(this.player);
		}

		public override void PostUpdate()
		{
			pHelper?.NextBeaconPosition();
		}
	}
}
