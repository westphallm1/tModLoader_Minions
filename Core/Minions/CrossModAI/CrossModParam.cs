using System;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI
{
	/// <summary>
	/// Read/Write Cross-Mod property that can be both read and written by a mod.Call.
	/// Usually meant to mirror the initial mod.Call registry parameters
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	internal class CrossModParam : Attribute
	{
	}
}
