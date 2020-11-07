using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Squires;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace AmuletOfManyMinions
{
	public class AmuletOfManyMinions : Mod
	{
		public override void Load()
		{
			SquireMinionTypes.Load();
			NecromancerAccessory.Load();
		}

		public override void Unload()
		{
			SquireMinionTypes.Unload();
			NecromancerAccessory.Unload();
		}
	}
}
