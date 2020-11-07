using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Squires;
using Terraria.ModLoader;

namespace AmuletOfManyMinions
{
	public class AmuletOfManyMinions : Mod
	{
		public override void Load()
		{
			NPCSets.Load();
			SquireMinionTypes.Load();
			NecromancerAccessory.Load();
			SquireGlobalProjectile.Load();
		}

		public override void PostSetupContent()
		{
			NPCSets.Populate();
		}

		public override void Unload()
		{
			NPCSets.Unload();
			SquireMinionTypes.Unload();
			NecromancerAccessory.Unload();
			SquireGlobalProjectile.Unload();
		}
	}
}
