using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;
using Terraria;
using AmuletOfManyMinions.Core.Minions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AmuletOfManyMinions.Core.Minions.Tactics;

namespace AmuletOfManyMinions.UI.TacticsUI
{
	// invisible element drawn right behind the buffs, used
	// to capture clicks on buffs
	internal class BuffRowClickCapture : UIElement
	{
		static int buffsPerLine = 11;
		static int buffWidth = 38;
		static int buffHeight = 50;
		// max 2 rows of buffs assuming vanilla buff limit, add more as a courtesy
		static int buffLines = 6;
		// max bounds to be clicking inside buffs
		static int xMin = 32;
		static int yMin = 76;

		// used to capture 'click + drag' action on minion tactic group dropdown
		int clickedBuffIdx = -1;

		internal static TacticsGroupBuffDropdown dropDown;
		public override void OnInitialize()
		{
			base.OnInitialize();
			SetupDropdown();
			Append(dropDown);
			Left.Pixels = xMin;
			Width.Pixels = buffWidth * buffsPerLine;
			Top.Pixels = yMin;
			Height.Pixels = buffLines * buffHeight + dropDown.Height.Pixels;
		}

		private void SetupDropdown()
		{
			List<TacticsGroupButton> groupButtons = new List<TacticsGroupButton>();
			int rows = MinionTacticsPlayer.TACTICS_GROUPS_COUNT;
			int columns = 1;

			int width = 24;
			int height = 24;

			//Keep them even numbers, they are divided by 2 later
			int xMargin = 4;
			int yMargin = 4;

			int widthWithMargin = width + xMargin;
			int heightWithMargin = height + yMargin;

			for(int i = 0; i< MinionTacticsPlayer.TACTICS_GROUPS_COUNT; i++)
			{
				TacticsGroupButton button = new TacticsGroupButton(i, quiet: true);

				//Top left of the position it should insert in
				int yPos = yMargin / 2 + heightWithMargin * i;
				int xPos = xMargin / 2;

				//Calculation so its centered around the center of both calculated pos and button (dynamic!)
				float yOffsetForSize = (heightWithMargin - button.Height.Pixels) / 2;
				float xOffsetForSize = (widthWithMargin - button.Width.Pixels) / 2;

				button.Top.Pixels = yPos + yOffsetForSize;
				button.Left.Pixels = xPos + xOffsetForSize;

				groupButtons.Add(button);
			}

			dropDown = new TacticsGroupBuffDropdown(groupButtons);
			// Initially start in the top corner, below the first buff row
			dropDown.Top.Pixels = 50;
			dropDown.Left.Precent = 0f;

			//Adjust the panels dimensions after populating it
			dropDown.Width.Pixels = xMargin + widthWithMargin * columns;
			dropDown.Height.Pixels = yMargin + heightWithMargin * rows;
			dropDown.PaddingLeft = xMargin;
			dropDown.PaddingRight = xMargin;

		}

		public override void MouseDown(UIMouseEvent evt)
		{
			// another menu is open, so don't check
			if(Main.ingameOptionsWindow || Main.playerInventory)
			{
				return; 
			}
			clickedBuffIdx = GetClickedBuffIdx(evt);
			if(clickedBuffIdx == -1)
			{
				return;
			}
			MinionTacticsPlayer myPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
			int buffType = myPlayer.player.buffType[clickedBuffIdx];
			if(myPlayer.GroupIsSetForMinion(buffType))
			{
				Vector2 buffTop = GetBuffTopLeft(clickedBuffIdx);
				dropDown.Left.Pixels = buffTop.X;
				dropDown.Top.Pixels = buffTop.Y + 38;
				dropDown.SetSelected(buffType);
			}
		}

		public override void MouseUp(UIMouseEvent evt)
		{
			base.MouseUp(evt);
			// only care about mouse-ups in the click + drag action after selecting a buff
			if(clickedBuffIdx == -1)
			{
				return;
			}
			// should probably read this from TacticsPlayer but that's a bit clunky
			MinionTacticsPlayer myPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
			int buffType = myPlayer.player.buffType[clickedBuffIdx];
			clickedBuffIdx = -1;

			float mouseY = evt.MousePosition.Y;
			float startY = Top.Pixels + dropDown.Top.Pixels;
			// don't trigger if not within vertical bounds (any horizontal pos is fine)
			if (mouseY < startY || mouseY > startY + dropDown.Height.Pixels)
			{
				return;
			}
			float regionHeight = dropDown.Height.Pixels / MinionTacticsPlayer.TACTICS_GROUPS_COUNT;
			int region = (int)((mouseY - startY) / regionHeight);
			myPlayer.SetGroupForMinion(region, buffType);
			UnsetSelected();

		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			DrawBuffOverlays(spriteBatch);
		}

		internal void DrawBuffOverlays(SpriteBatch spriteBatch)
		{
			// another menu is open, so don't draw overlays
			if(Main.ingameOptionsWindow || Main.playerInventory)
			{
				return; 
			}
			Vector2 topCorner = new Vector2(xMin, yMin);
			float scale = 1;
			
			MinionTacticsPlayer myPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
			for(int i = 0; i < myPlayer.player.CountBuffs(); i++)
			{
				int buffType = myPlayer.player.buffType[i];
				if(myPlayer.GroupIsSetForMinion(buffType))
				{
					int groupIdx = myPlayer.GetGroupForBuff(buffType);
					Vector2 drawPos = topCorner + GetBuffTopLeft(i);
					Texture2D texture = TargetSelectionTacticHandler.GroupOverlayTextures[groupIdx];
					spriteBatch.Draw(texture, drawPos, null, Color.White, 0f, Vector2.Zero, scale, 0f, 0f);
				}
			}
		}

		public Vector2 GetBuffTopLeft(int buffIdx)
		{
			int row = buffIdx / buffsPerLine;
			int column = buffIdx % buffsPerLine;
			return new Vector2(column * buffWidth, row * buffHeight);
		}
		private int GetClickedBuffIdx(UIMouseEvent evt)
		{
			// check for UI state to ensure buffs are showing
			int clickX = (int)evt.MousePosition.X;
			int clickY = (int)evt.MousePosition.Y;
			Player myPlayer = Main.player[Main.myPlayer];
			int nBuffs = myPlayer.CountBuffs();
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
			return buffIdx;
		}

		internal void UnsetSelected()
		{
			dropDown.UnsetSelected();
		}
	}
}
