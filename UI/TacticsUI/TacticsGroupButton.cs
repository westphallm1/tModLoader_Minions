using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Minions.Tactics.TacticsGroups;
using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using AmuletOfManyMinions.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.UI;

namespace AmuletOfManyMinions.UI.TacticsUI
{
	/// <summary>
	/// The clickable button representing a minion tactic
	/// </summary>
	internal class TacticsGroupButton : SelectableUIImageButton
	{
		/// <summary>
		/// The buttons' index in the list of buttons, and in the list of tactics groups
		/// Using the same variable for both may present issues in the future
		/// </summary>
		internal readonly int index;

		internal TacticsGroup TacticsGroup => TargetSelectionTacticHandler.TacticsGroups[index];

		internal override string ShortHoverText => TacticsGroup.Name;

		internal override string LongHoverText => 
			TacticsGroup.Name + "\n" +
			TacticsGroup.Description;

		internal override Texture2D OutlineTexture => TargetSelectionTacticHandler.GroupOutlineTextures[index];

		internal TacticsGroupButton(int index) : base(TargetSelectionTacticHandler.GroupTextures[index])
		{
			this.index = index;
		}
	}
}
