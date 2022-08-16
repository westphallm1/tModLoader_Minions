using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Minions.Tactics.TacticsGroups;
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
	internal class TacticsGroupButton : SelectableUIImageButton
	{
		/// <summary>
		/// The buttons' index in the list of buttons, and in the list of tactics groups
		/// Using the same variable for both may present issues in the future
		/// </summary>
		internal readonly int index;
		/// <summary>
		/// Whether to display the full name and detail text while mousing over the button
		/// </summary>
		private readonly bool quiet;
		/// <summary>
		/// Whether to show the button's outline while it's selected
		/// </summary>
		private readonly bool showOutline;

		internal TacticsGroup TacticsGroup => TargetSelectionTacticHandler.TacticsGroups[index];

		internal override string ShortHoverText => TacticsGroup.Name;

		internal override string LongHoverText => 
			quiet ? 
			ShortHoverText :
			TacticsGroup.Name + "\n" +
			TacticsGroup.Description;

		internal override Asset<Texture2D> OutlineTexture => showOutline ? null : TargetSelectionTacticHandler.GroupOutlineTextures[index];

		internal TacticsGroupButton(int index, bool quiet = false, bool radialHover = false) : base(TargetSelectionTacticHandler.GroupTextures[index])
		{
			this.index = index;
			this.quiet = quiet;
			this.showOutline = radialHover;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			// draw a little icon in the bottem left corner the current tactic for the given group
			MinionTacticsPlayer tacticsPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
			byte tacticsId = tacticsPlayer.TacticIDByGroup[index];
			Texture2D tacticSmallTexture = TargetSelectionTacticHandler.SmallTextures[tacticsId].Value;
			CalculatedStyle dimensions = GetDimensions();
			float scale = 0.75f;
			Vector2 bottomLeft = new Vector2(dimensions.X, dimensions.Y + dimensions.Height);
			Vector2 tacticPosition = bottomLeft - new Vector2(0, tacticSmallTexture.Height * scale);
			Color color = Color.White * (InHoverState || selected ? 1 : 0.7f);
			spriteBatch.Draw(tacticSmallTexture, tacticPosition, null, color, 0f, Vector2.Zero, scale, 0f, 0f);
		}
	}
}
