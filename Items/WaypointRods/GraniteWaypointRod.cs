using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace AmuletOfManyMinions.Items.WaypointRods
{
	public class GraniteWaypointRod : WaypointRod
	{
		public override string Texture => "Terraria/Item_"+ItemID.SapphireStaff;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Rod of Minion Guidance");
			Tooltip.SetDefault(
				"[c/32CD32:30 tile range.]\n" +
				"Click to place a minion guidance waypoint!\n" +
				"Minions will automatically navigate to the waypoint\n" +
				"Attacking with a non-summon weapon dispels the waypoint");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.useTime = 60;
			item.useAnimation = 60;
			placementRange = 16 * 30;
		}
	}
}
