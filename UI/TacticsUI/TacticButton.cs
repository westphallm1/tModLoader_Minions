using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using AmuletOfManyMinions.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
	internal class TacticButton : SelectableUIImageButton
	{
		/// <summary>
		/// The buttons' index in the list of buttons
		/// </summary>
		internal readonly int index;

		/// <summary>
		/// The tactic ID this button is associated with
		/// </summary>
		internal readonly byte ID;

		internal TargetSelectionTactic Tactic => TargetSelectionTacticHandler.GetTactic(ID);

		internal override string ShortHoverText => TargetSelectionTacticHandler.GetDisplayName(ID).ToString();

		internal override string LongHoverText =>
			ShortHoverText + "\n" +
			TargetSelectionTacticHandler.GetDescription(ID).ToString();

		internal override Asset<Texture2D> OutlineTexture => TargetSelectionTacticHandler.GetOutlineTexture(ID);

		internal TacticButton(int index, byte id) : base(TargetSelectionTacticHandler.GetTexture(id)/*do not refactor this to Tactic*/)
		{
			this.index = index;
			ID = id;
		}
	}
}
