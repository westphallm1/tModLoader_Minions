using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.CrossModSystem.Samples
{
	/// <summary>
	/// Sample Mod.Calls using Example Mod's items
	/// </summary>
	internal class ExampleModCrossMod : ModSystem
	{
		public override void PostSetupContent()
		{
			ExampleModRegisterPathfinder();
		}

		/// <summary>
		/// Register Example Mod's ExampleMinion for AoMM's pathfinding. AoMM will override the projectile's
		/// position and velocity while the pathfinder is present and the minion is not attacking an enemy,
		/// but will preserve its normal AI otherwise.
		/// </summary>
		internal void ExampleModRegisterPathfinder()
		{
			if (!ModLoader.TryGetMod("ExampleMod", out Mod exampleMod)) { return; }
			var amuletOfManyMinions = Mod; // this mod

			ModProjectile exampleMinion = exampleMod.Find<ModProjectile>("ExampleSimpleMinion");
			ModBuff exampleBuff = exampleMod.Find<ModBuff>("ExampleSimpleMinionBuff");
			int travelSpeed = 8;

			amuletOfManyMinions.Call("RegisterPathfinder", exampleMinion, exampleBuff, travelSpeed);
		}
	}
}
