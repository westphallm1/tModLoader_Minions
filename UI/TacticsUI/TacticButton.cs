using AmuletOfManyMinions.Core.Minions.Tactics;
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
	internal class TacticButton : UIImageButtonExtended
	{
		internal const float AlphaOver = 0.9f;
		internal const float AlphaOut = 0.6f;

		/// <summary>
		/// The buttons' index in the list of buttons
		/// </summary>
		internal readonly int index;

		/// <summary>
		/// The tactic ID this button is associated with
		/// </summary>
		internal readonly byte ID;

		/// <summary>
		/// Represents if it is the currently selected tactic by the player. Only one tactic can be selected exclusively
		/// </summary>
		internal bool selected = false;

		private int hoverTime = 0;
		private const int StartShowingDescription = 60;

		internal TargetSelectionTactic Tactic => TargetSelectionTacticHandler.GetTactic(ID);

		internal TacticButton(int index, byte id) : base(TargetSelectionTacticHandler.GetTexture(id)/*do not refactor this to Tactic*/)
		{
			this.index = index;
			ID = id;
		}

		public override void OnInitialize()
		{
			SetHoverText(TargetSelectionTacticHandler.GetDisplayName(ID));
		}

		public override void MouseOut(UIMouseEvent evt)
		{
			hoverTime = 0;
			SetHoverText(TargetSelectionTacticHandler.GetDisplayName(ID));
			base.MouseOut(evt);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			//Draw the outline when selected. Relies on alpha being 1f when selected otherwise it will look bad
			if (selected)
			{
				for (int i = 0; i < 4; i++)
				{
					//i: 0 | 1 | 2 | 3 ...
					//x:-1 |-1 | 1 | 1 ...repeat
					//y:-1 | 1 |-1 | 1 ...repeat

					int x = i / 2 % 2 == 0 ? -1 : 1;
					int y = i % 2 == 0 ? -1 : 1;

					DrawInternal(spriteBatch, TargetSelectionTacticHandler.GetOutlineTexture(ID), new Vector2(x, y) * 1.5f, Color.White);
				}
			}

			if (IsMouseHovering)
			{
				hoverTime++;
				if (hoverTime == StartShowingDescription)
				{
					//After exactly StartShowingDescription ticks of hovering, change the text
					SetHoverText(TargetSelectionTacticHandler.GetDisplayName(ID) + "\n" +
						TargetSelectionTacticHandler.GetDescription(ID));
				}
			}

			base.DrawSelf(spriteBatch);
		}

		internal void SetSelected(bool selected)
		{
			this.selected = selected;
			if (selected)
			{
				SetAlpha(1f, 1f);
			}
			else
			{
				SetAlpha(AlphaOver, AlphaOut);
			}
		}
	}
}
