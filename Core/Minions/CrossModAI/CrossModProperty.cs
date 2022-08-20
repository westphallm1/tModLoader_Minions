using System;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI
{
	[AttributeUsage(AttributeTargets.Property)]
	internal class CrossModProperty : Attribute
	{
		public string name;
		public CrossModProperty()
		{

		}

		public CrossModProperty(string name)
		{
			this.name = name;
		}
	}
}
