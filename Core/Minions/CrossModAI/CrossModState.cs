using System;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI
{
	/// <summary>
	/// Read-only property that can be accessed via mod.Calls, but not updated
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	internal class CrossModState : Attribute
	{
	}
}
