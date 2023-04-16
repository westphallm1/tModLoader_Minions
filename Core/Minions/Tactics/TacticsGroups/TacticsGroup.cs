using System;
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

		internal static string[] GroupNameKeys = new string[] { "Sun", "Moon", "Both" };
		internal string NameKey => GroupNameKeys[index];

		public TacticsGroup(int index)
		{
			this.index = index;
		}

		public virtual string Texture => (GetType().Namespace + "." + GetType().Name + "_" + index).Replace('.', '/');
	}
}
