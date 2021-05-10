using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Pathfinding;
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
	/// A radial menu that allows quickly toggling the active tactic group, and dispelling the minion waypoint
	/// </summary>
	class TacticQuickSelectRadialMenu : RadialMenu
	{
		internal TacticQuickSelectRadialMenu(List<RadialMenuButton> buttons) : base(buttons)
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
					for(int j = 0; j < MinionTacticsPlayer.TACTICS_GROUPS_COUNT; j++)
					{
						buttons[j].Highlighted = tacticsPlayer.CurrentTacticGroup == j;
					}
					StopShowing();
				};
			}
			buttons[MinionTacticsPlayer.TACTICS_GROUPS_COUNT].OnLeftClick = () =>
			{
				if(!doDisplay) { return; }
				// remove every waypoint, lack of ability to remove individual ones is a bit annoying
				// but less annoying than having to click multiple times to remove a single waypoint
				MinionTacticsPlayer tacticsPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
				MinionPathfindingPlayer waypointPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionPathfindingPlayer>();
				int oldGroup = tacticsPlayer.CurrentTacticGroup;
				tacticsPlayer.CurrentTacticGroup = MinionTacticsPlayer.TACTICS_GROUPS_COUNT - 1;
				waypointPlayer.ToggleWaypoint(true);
				tacticsPlayer.CurrentTacticGroup = oldGroup;
				for(int j = 0; j < MinionTacticsPlayer.TACTICS_GROUPS_COUNT; j++)
				{
					buttons[j].Highlighted = false;
				}
				buttons.Last().Highlighted = true;
				StopShowing();
			};

			// all buttons have the same left and right click 
			for(int i = 0; i < buttons.Count; i++)
			{
				buttons[i].OnRightClick = buttons[i].OnLeftClick;
			}
		}

		/// <summary>
		/// This gets called each time the dropdown moves to a new minion buff
		/// </summary>
		/// <param name="id">Tactic ID</param>
		internal override void StartShowing()
		{
			base.StartShowing();
			MinionTacticsPlayer tacticsPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
			for(int i = 0; i < buttons.Count; i++)
			{
				buttons[i].Highlighted = tacticsPlayer.CurrentTacticGroup == i;
			}
		}
	}
}
