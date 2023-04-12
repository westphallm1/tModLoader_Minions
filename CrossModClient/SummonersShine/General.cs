using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
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
			bool rv = !CrossModSetup.SummonersShineLoaded || !ModLoader.TryGetMod("SummonersShine", out rvMod) || ServerConfig.Instance.DisableSummonersShineAI;
			summonersShine = rvMod;
			return rv;
		}
		internal static void ApplyChanges_COUNTASMINION(int ProjType)
		{
			if (SummonersShineDisabled(out Mod summonersShine))
				return;
			summonersShine.Call(CHANGECONFIG, COUNTASMINION, ProjType);
		}

		internal static void ApplyChanges_CHARREDCHIMERAMINIONHEAD(int ProjType, int ItemType)
		{
			if (SummonersShineDisabled(out Mod summonersShine))
				return;
			ApplyChanges_COUNTASMINION(ProjType);
			const int ADD_FILTER = 0;
			const int SET_SUMMON_MINION_WEAPON_STAT_SOURCE = 15;
			summonersShine.Call(ADD_FILTER, SET_SUMMON_MINION_WEAPON_STAT_SOURCE, ProjType, ItemType);
		}

		internal static void ApplyChanges_STEPPED(int ProjType)
		{
			if (SummonersShineDisabled(out Mod summonersShine))
				return;
			summonersShine.Call(CHANGEMINIONSTATICS, ProjType, CHANGELERPTYPE, STEPPED);
		}
	}
}
