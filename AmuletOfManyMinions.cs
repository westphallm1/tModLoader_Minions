using AmuletOfManyMinions.Items.Accessories;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace AmuletOfManyMinions
{
	public class AmuletOfManyMinions : Mod
	{
		public override void Load()
		{
			NecromancerAccessory.accessories = new List<NecromancerAccessory>();
		}

		public override void Unload()
		{
			NecromancerAccessory.accessories?.Clear();
			NecromancerAccessory.accessories = null;
		}
	}
}
