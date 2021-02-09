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
		private OpenCloseButton openCloseButton;

		internal bool opened = false;

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

			//These get appended to tacticsPanel later
			List<TacticButton> tacticsButtons = new List<TacticButton>();

			List<byte> orderedIDs = new List<byte>
			{
				//First row
				TargetSelectionTacticHandler.GetTactic<ClosestEnemyToMinion>().ID,
				TargetSelectionTacticHandler.GetTactic<StrongestEnemy>().ID,
				TargetSelectionTacticHandler.GetTactic<LeastDamagedEnemy>().ID,
				TargetSelectionTacticHandler.GetTactic<SpreadOut>().ID,

				//Second row
				TargetSelectionTacticHandler.GetTactic<ClosestEnemyToPlayer>().ID,
				TargetSelectionTacticHandler.GetTactic<WeakestEnemy>().ID,
				TargetSelectionTacticHandler.GetTactic<MostDamagedEnemy>().ID,
				TargetSelectionTacticHandler.GetTactic<AttackGroups>().ID,
			};

			int assignedCount = orderedIDs.Count;
			//int count = TargetSelectionTacticHandler.Count;

			//if (count != assignedCount)
			//{
			//	throw new Exception($"Tactic count does not match the buttons added to the tactics UI (loaded: {count}, assigned: {assignedCount})");
			//}

			//Keep them even numbers, they are divided by 2 later
			int xMargin = 6;
			int yMargin = 4;

			int rows = 2;
			int columns = assignedCount / rows;

			//Assume same size for all icons
			//TODO change these when icons are done
			int width = 32;
			int height = 32;
			int widthWithMargin = width + xMargin;
			int heightWithMargin = height + yMargin;

			for (int i = 0; i < assignedCount; i++)
			{
				int row = i / columns;
				int column = i % columns;

				TacticButton button = new TacticButton(i, orderedIDs[i]);

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

			//Make it so the panel aligns with the top left corner of the main element
			tacticsPanel.Top.Precent = 0f;
			tacticsPanel.Left.Precent = 0f;

			//Adjust the panels dimensions after populating it
			tacticsPanel.Width.Pixels = xMargin + widthWithMargin * columns;
			tacticsPanel.Height.Pixels = yMargin + heightWithMargin * rows;
			Append(tacticsPanel); //Append links the two UIs together in a children/parent relation

			//Finally adjust the button
			openCloseButton = new OpenCloseButton(closeTexture);
			openCloseButton.Top.Pixels = tacticsPanel.Height.Pixels; //Attach at the bottom of the panel
			openCloseButton.HAlign = 0.5f; //Center aligns with center of parent
			openCloseButton.OnClick += OpenCloseButton_OnClick;
			Append(openCloseButton);

			//After all childrens widths and heights are determined, adjust the element one so it covers everything that got appended to
			this.Width.Pixels = tacticsPanel.Width.Pixels;
			this.Height.Pixels = tacticsPanel.Height.Pixels + openCloseButton.Height.Pixels;
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			//Align main UI here instead of in OnInitialize, so it updates on resolution changes etc.

			float openedY = 0;

			//If closed, hide it so only the button is visible
			float closedY = openedY - this.Height.Pixels + openCloseButton.Height.Pixels;

			HandleFadeIn();

			this.Top.Pixels = MathHelper.Lerp(closedY, openedY, openAmount - 1f);
			this.Left.Pixels = GetAnchorLeft();
		}

		private void OpenCloseButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			opened = !opened;
			openCloseButton.SetImage(opened ? openTexture : closeTexture);
			openCloseButton.SetHoverText(!opened ? Open : Close);
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
			if (opened)
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
			else
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
		/// <param name="id">Tactic ID</param>
		internal void SetSelected(int id)
		{
			tacticsPanel.SetSelected(id);
		}
	}
}