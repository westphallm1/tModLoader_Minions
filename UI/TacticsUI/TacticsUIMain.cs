using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using AmuletOfManyMinions.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace AmuletOfManyMinions.UI.TacticsUI
{

	internal enum OpenedTriState
	{
		FALSE,
		TRUE,
		HIDDEN
	}
	/// <summary>
	/// This is the actual UI used for aligning and most of the logic, it is invisible by itself. The reason it exists is so the button is within bounds of "being clickable"
	/// </summary>
	internal class TacticsUIMain : UIElement
	{
		//Localize this later, there are actually no vanilla keys for "Close" and "Open" anywhere by themselves
		internal const string Close = "Close Minion Tactics";
		internal const string Open = "Open Minion Tactics";

		internal Texture2D openTexture;
		internal Texture2D closeTexture;

		private float openAmount = 1f; //1f closed, 2f opened

		//We keep references of the children for easier access later
		private TacticsPanel tacticsPanel;
		private TacticsGroupPanel tacticsGroupPanel;
		private OpenCloseButton openCloseButton;

		internal OpenedTriState opened = OpenedTriState.HIDDEN;

		public override void OnInitialize()
		{
			if (openTexture == null)
			{
				openTexture = ModContent.GetTexture("AmuletOfManyMinions/UI/Common/OpenButton");
			}
			if (closeTexture == null)
			{
				closeTexture = ModContent.GetTexture("AmuletOfManyMinions/UI/Common/CloseButton");
			}

			//Move outside of screen initially, we update the location manually each tick in Update
			Left.Pixels = -int.MaxValue / 2;
			Top.Pixels = -int.MaxValue / 2;

			// first, set up the tactics group panel
			SetupTacticsGroupPanel();

			// then, set up the tactics panel
			SetupTacticsPanel();

			// group panel is drawn on top of the tactics panel, so append in reverse order of creation
			Append(tacticsPanel); //Append links the two UIs together in a children/parent relation
			Append(tacticsGroupPanel);

			//Finally adjust the open/close button
			openCloseButton = new OpenCloseButton(closeTexture);
			openCloseButton.Top.Pixels = tacticsPanel.Top.Pixels + tacticsPanel.Height.Pixels; //Attach at the bottom of the panel
			openCloseButton.HAlign = 0.65f; //Center aligns with center-ish of parent
			openCloseButton.OnClick += OpenCloseButton_OnClick;
			Append(openCloseButton);

			//After all childrens widths and heights are determined, adjust the element one so it covers everything that got appended to
			this.Width.Pixels = tacticsPanel.Left.Pixels + tacticsPanel.Width.Pixels;
			this.Height.Pixels = tacticsPanel.Top.Pixels + tacticsPanel.Height.Pixels + openCloseButton.Height.Pixels;
		}

		/// <summary>
		/// Setup the panel that allows the user to set the active tactics group
		/// </summary>
		private void SetupTacticsGroupPanel()
		{
			List<TacticsGroupButton> groupButtons = new List<TacticsGroupButton>();
			int rows = 2;
			int columns = 2;

			int width = 24;
			int height = 24;

			//Keep them even numbers, they are divided by 2 later
			int xMargin = 4;
			int yMargin = 4;

			int widthWithMargin = width + xMargin;
			int heightWithMargin = height + yMargin;

			for(int i = 0; i< MinionTacticsPlayer.TACTICS_GROUPS_COUNT; i++)
			{
				TacticsGroupButton button = new TacticsGroupButton(i);
				// todo space these out radially

				int row = i / columns;
				int column = i % columns;

				//Top left of the position it should insert in
				int yPos = yMargin / 2 + heightWithMargin * row;
				// offset the X position of the icon in the second row
				int xPos = xMargin / 2 + widthWithMargin * column + (row * widthWithMargin / 2);

				//Calculation so its centered around the center of both calculated pos and button (dynamic!)
				float yOffsetForSize = (heightWithMargin - button.Height.Pixels) / 2;
				float xOffsetForSize = (widthWithMargin - button.Width.Pixels) / 2;

				button.Top.Pixels = yPos + yOffsetForSize;
				button.Left.Pixels = xPos + xOffsetForSize;

				groupButtons.Add(button);
			}

			tacticsGroupPanel = new TacticsGroupPanel(groupButtons);
			//Make it so the panel aligns with the top left corner of the main element
			tacticsGroupPanel.Top.Precent = 0f;
			tacticsGroupPanel.Left.Precent = 0f;

			//Adjust the panels dimensions after populating it
			tacticsGroupPanel.Width.Pixels = xMargin + widthWithMargin * columns;
			tacticsGroupPanel.Height.Pixels = yMargin + heightWithMargin * rows;
		}

		/// <summary>
		/// Set up the tactics panel which allows choosing a tactic for the currently selected tactic.
		/// Must be called after SetupTacticsGroupPanel, for alignment reasons
		/// </summary>
		private void SetupTacticsPanel()
		{
			//These get appended to tacticsPanel later
			List<TacticButton> tacticsButtons = new List<TacticButton>();

			int assignedCount = TargetSelectionTacticHandler.OrderedIds.Count;

			//Keep them even numbers, they are divided by 2 later
			int xMargin = 4;
			int yMargin = 4;

			int rows = 2;
			int columns = assignedCount / rows;

			//Assume same size for all icons
			int width = 30;
			int height = 30;
			int widthWithMargin = width + xMargin;
			int heightWithMargin = height + yMargin;

			for (int i = 0; i < assignedCount; i++)
			{
				int row = i / columns;
				int column = i % columns;

				TacticButton button = new TacticButton(i, TargetSelectionTacticHandler.OrderedIds[i]);

				//Top left of the position it should insert in
				int yPos = yMargin / 2 + heightWithMargin * row;
				int xPos = xMargin / 2 + widthWithMargin * column;

				//Calculation so its centered around the center of both calculated pos and button (dynamic!)
				float yOffsetForSize = (heightWithMargin - button.Height.Pixels) / 2;
				float xOffsetForSize = (widthWithMargin - button.Width.Pixels) / 2;

				button.Top.Pixels = yPos + yOffsetForSize;
				button.Left.Pixels = xPos + xOffsetForSize;

				tacticsButtons.Add(button);
			}

			tacticsPanel = new TacticsPanel(tacticsButtons);

			// Make it so the panel aligns with the right edge of the tactics group panel, slightly offset downwards
			tacticsPanel.Top.Pixels = 8;
			tacticsPanel.Left.Pixels = tacticsGroupPanel.Width.Pixels - 2;
			//tacticsPanel.Left.Precent = 0f; // tacticsGroupPanel.Left.Pixels + tacticsGroupPanel.Width.Pixels;

			//Adjust the panels dimensions after populating it
			tacticsPanel.Width.Pixels = xMargin + widthWithMargin * columns;
			tacticsPanel.Height.Pixels = yMargin + heightWithMargin * rows;

		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			//Align main UI here instead of in OnInitialize, so it updates on resolution changes etc.

			float openedY = 0;

			//If closed, hide it so only the button is visible
			float closedY = openedY - this.Height.Pixels + openCloseButton.Height.Pixels;

			// If not yet unlocked, draw off the top of the screen
			float hiddenY = openedY - this.Height.Pixels - openCloseButton.Height.Pixels;

			if(opened == OpenedTriState.HIDDEN)
			{
				this.Top.Pixels = hiddenY;
			} else
			{
				HandleFadeIn();
				this.Top.Pixels = MathHelper.Lerp(closedY, openedY, openAmount - 1f);
			}
			this.Left.Pixels = GetAnchorLeft();
		}

		private void OpenCloseButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			SetOpenClosedState(opened == OpenedTriState.TRUE ? OpenedTriState.FALSE : OpenedTriState.TRUE);
		}

		internal void SetOpenClosedState(OpenedTriState newState)
		{
			opened = newState;
			if(newState != OpenedTriState.HIDDEN)
			{
				openCloseButton.SetImage(opened == OpenedTriState.TRUE ? openTexture : closeTexture);
				openCloseButton.SetHoverText(opened  == OpenedTriState.TRUE ? Close : Open);
			}
		}

		private float GetAnchorLeft()
		{
			if (ClientConfig.Instance.AnchorToHealth)
			{
				//Code copied from minimap button code
				bool flag = false;
				int num9 = Main.screenWidth - 440;
				int leftEdgeOfMap = num9;
				int num10 = 40;
				if (Main.screenWidth < 940)
					flag = true;

				if (flag || !Main.playerInventory || !Main.mapEnabled)
				{
					//Additional conditions and code added here
					leftEdgeOfMap += 4 * 32;
					num9 = Main.screenWidth - 40;
					num10 = Main.screenHeight - 200;
				}

				for (int k = 0; k < 4; k++)
				{
					int num12 = num9 + k * 32;
					int num13 = num10;
					if (flag)
					{
						num12 = num9;
						num13 = num10 + k * 32;
					}

					int num14 = k;
					int num11 = 120;
					if (k > 0 && Main.mapStyle == k - 1)
						num11 = 200;

					//Main.spriteBatch.Draw(Main.mapIconTexture[num14], new Vector2(num12, num13), new Microsoft.Xna.Framework.Rectangle(0, 0, Main.mapIconTexture[num14].Width, Main.mapIconTexture[num14].Height), new Microsoft.Xna.Framework.Color(num11, num11, num11, num11), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
				}

				return leftEdgeOfMap - this.Width.Pixels;
			}
			else/* if (ClientConfig.Instance.AnchorToInventory)*/
			{
				int leftEdgeOfAmmoSlot = 534;
				int leftEdgeOfCoinSlot = 497;

				return leftEdgeOfCoinSlot;
			}
		}


		private void HandleFadeIn()
		{
			if (opened == OpenedTriState.TRUE)
			{
				if (openAmount <= 1f)
				{
					openAmount = 1.005f;
				}

				openAmount *= 1.05f;

				if (openAmount > 2f)
				{
					openAmount = 2f;
				}
			}
			else if (opened == OpenedTriState.FALSE)
			{
				if (openAmount >= 2f)
				{
					openAmount = 1.995f;
				}

				openAmount *= 0.95f;

				if (openAmount < 1f)
				{
					openAmount = 1f;
				}
			}
		}

		/// <summary>
		/// This gets called once when the player enters the world to initialize the UI with values
		/// </summary>
		/// <param name="tacticId">Tactic ID</param>
		internal void SetSelected(int tacticId, int groupId)
		{
			tacticsPanel.SetSelected(tacticId);
			tacticsGroupPanel.SetSelected(groupId);
		}

	}
}