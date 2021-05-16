using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.UI;

namespace AmuletOfManyMinions.UI.TacticsUI
{
	/// <summary>
	/// A radial menu that allows fully toggling the active tactic group and tactic
	/// </summary>
	class TacticFullSelectRadialMenu : RadialMenu
	{
		internal TacticFullSelectRadialMenu(List<RadialMenuButton> buttons) : base(buttons)
		{

		}


		public override void OnInitialize()
		{
			base.OnInitialize();
			// depending on the order of children for setup like this is iffy, but relatively efficient
			for(int i = 0; i < MinionTacticsPlayer.TACTICS_GROUPS_COUNT; i++)
			{
				int localI = i;
				buttons[i].OnLeftClick = () =>
				{
					if(!doDisplay) { return; }
					MinionTacticsPlayer tacticsPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
					tacticsPlayer.SetTacticsGroup(localI);
					SetButtonHighlights();
				};
			}
			for (int i = 0; i < TargetSelectionTacticHandler.OrderedIds.Count; i++)
			{
				int buttonIdx = i + MinionTacticsPlayer.TACTICS_GROUPS_COUNT;
				buttons[buttonIdx].OnLeftClick = () =>
				{
					if(!doDisplay) { return; }
					MinionTacticsPlayer tacticsPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
					TacticsRadialMenuButton tacticButton = (TacticsRadialMenuButton)buttons[buttonIdx];
					tacticsPlayer.SetTactic(tacticButton.tacticId);
					SetButtonHighlights();
				};
			}
			buttons.Last().OnLeftClick = () =>
			{
				if (!doDisplay) { return; }
				StopShowing();
			};

			// all buttons have the same left and right click 
			for(int i = 0; i < buttons.Count; i++)
			{
				buttons[i].OnRightClick = buttons[i].OnLeftClick;
			}
		}

		private void SetButtonHighlights()
		{
			MinionTacticsPlayer tacticsPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
			for(int i = 0; i < MinionTacticsPlayer.TACTICS_GROUPS_COUNT; i++)
			{
				buttons[i].Highlighted = i == tacticsPlayer.CurrentTacticGroup;
			}
			for (int i = 0; i < TargetSelectionTacticHandler.OrderedIds.Count; i++)
			{
				int buttonIdx = i + MinionTacticsPlayer.TACTICS_GROUPS_COUNT;
				TacticsRadialMenuButton tacticButton = (TacticsRadialMenuButton)buttons[buttonIdx];
				tacticButton.Highlighted = tacticButton.tacticId == tacticsPlayer.TacticID;
			}

		}
		/// <summary>
		/// This gets called each time the dropdown moves to a new minion buff
		/// </summary>
		/// <param name="id">Tactic ID</param>
		internal override void StartShowing()
		{
			base.StartShowing();
			framesUntilHide = 3000;
			SetButtonHighlights();
		}
	}
}
