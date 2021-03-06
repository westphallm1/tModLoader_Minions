﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmuletOfManyMinions.Core.Minions.Tactics.TacticsGroups
{
	class TacticsGroup
	{
		// there is not much unique information per group,
		// so 
		internal int index;

		internal static string[] GroupNames = new string[] { "Sun Group", "Moon Group", "Both Groups" };
		internal string Name => GroupNames[index];
		internal string Description => "Each tactics group can be assigned a distinct tactic and waypoint.\n" +
			"Set a minion's tactic group by left clicking its buff icon";

		public TacticsGroup(int index)
		{
			this.index = index;
		}

		public virtual string Texture => (GetType().Namespace + "." + GetType().Name + "_" + index).Replace('.', '/');
	}
}
