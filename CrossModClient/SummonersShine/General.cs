using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.CrossModClient.SummonersShine
{
	internal static class General
	{
		const int CHANGECONFIG = 0;
		const int COUNTASMINION = 5;


		const int CHANGEMINIONSTATICS = 1;
		const int CHANGELERPTYPE = 23;
		const int STEPPED = 1;

		internal static bool SummonersShineDisabled(out Mod summonersShine)
		{
			Mod rvMod = null;
			bool rv = !CrossMod.SummonersShineLoaded || ServerConfig.Instance.DisableSummonersShineAI || !ModLoader.TryGetMod("SummonersShine", out rvMod);
			summonersShine = rvMod;
			return rv;
		}
		internal static void ApplyChanges_COUNTASMINION(int ProjType)
		{
			if (SummonersShineDisabled(out Mod summonersShine))
				return;
			summonersShine.Call(CHANGECONFIG, COUNTASMINION, ProjType);
		}

		internal static void ApplyChanges_STEPPED(int ProjType)
		{
			if (SummonersShineDisabled(out Mod summonersShine))
				return;
			summonersShine.Call(CHANGEMINIONSTATICS, ProjType, CHANGELERPTYPE, STEPPED);
		}
	}
}
