using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Gores
{
	class SeaSquireBubbleGore : ModGore
	{
		public override void OnSpawn(Gore gore)
		{
			base.OnSpawn(gore);
			updateType = 61;
		}
	}
}
