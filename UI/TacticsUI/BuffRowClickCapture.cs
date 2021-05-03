using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;
using Terraria;
using AmuletOfManyMinions.Core.Minions;

namespace AmuletOfManyMinions.UI.TacticsUI
{
	// invisible element drawn right behind the buffs, used
	// to capture clicks on buffs
	class BuffRowClickCapture : UIElement
	{
		static int buffsPerLine = 11;
		static int buffWidth = 38;
		static int buffHeight = 50;
		// max 2 rows of buffs assuming vanilla buff limit, double as a courtesy
		static int buffLines = 4;
		// max bounds to be clicking inside buffs
		static int xMin = 32;
		static int yMin = 76;
		public override void OnInitialize()
		{
			base.OnInitialize();
			Left.Pixels = xMin;
			Width.Pixels = buffWidth * buffsPerLine;
			Top.Pixels = yMin;
			Height.Pixels = buffLines * buffHeight;
		}

		public override void Click(UIMouseEvent evt)
		{
			// another menu is open, so don't check
			if(Main.ingameOptionsWindow || Main.playerInventory)
			{
				return; 
			}
			int clickedBuff = GetClickedBuffIdx(evt);
			if(clickedBuff == -1)
			{
				return;
			}
			MinionTacticsPlayer tacticsPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
			if(tacticsPlayer.GroupIsSetForMinion(clickedBuff))
			{
				tacticsPlayer.SetGroupForMinion(tacticsPlayer.CurrentTacticGroup, clickedBuff);
			}
			
		}
		public static int GetClickedBuffIdx(UIMouseEvent evt)
		{
			// check for UI state to ensure buffs are showing
			int clickX = (int)evt.MousePosition.X;
			int clickY = (int)evt.MousePosition.Y;
			Player myPlayer = Main.player[Main.myPlayer];
			int buffsPerLine = 11;
			int nBuffs = myPlayer.CountBuffs();
			int buffWidth = 38;
			int buffHeight = 50;
			// max bounds to be clicking inside buffs
			int relativeX = clickX - xMin;
			int relativeY = clickY - yMin;
			// check that we're not clicking between bounds
			if(relativeX % buffWidth > 32 || relativeY % buffHeight > 32)
			{
				return -1;
			}
			int xPos = (clickX - xMin) / buffWidth;
			int yPos = (clickY - yMin) / buffHeight;
			int buffIdx = buffsPerLine * yPos + xPos;
			if(buffIdx >= nBuffs)
			{
				return -1;
			}
			return myPlayer.buffType[buffIdx];
		}
	}
}
