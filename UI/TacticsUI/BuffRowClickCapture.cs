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
using AmuletOfManyMinions.UI.Common;
using Terraria.ModLoader;
using Terraria.GameContent;
using ReLogic.Content;

namespace AmuletOfManyMinions.UI.TacticsUI
{
	// giant invisible element drawn across the entire screen,
	// used to capture left clicks on buffs and right clicks with waypoint rods
	internal class BuffRowClickCapture : UIElement
	{
		static int buffsPerLine = 11;
		static int buffWidth = 38;
		static int buffHeight = 50;

		// max bounds to be clicking inside buffs
		static int xMin = 32;
		static int yMin = 76;

		// used to capture 'click + drag' action on minion tactic group dropdown
		int clickedBuffIdx = -1;

		// number of buff rows, used to update container height
		int buffRows = 1;

		// hardcoded list of position offsets for buttons in the radial quick select
		internal Vector2[] radialOffsets = new Vector2[]
		{
			new Vector2(0, 30),
			new Vector2(30, 0),
			new Vector2(30, 60),
			new Vector2(60, 30)
		};

		internal static TacticsGroupBuffDropdown dropDown;
		internal static TacticQuickSelectRadialMenu radialMenu;

		internal Asset<Texture2D> moreTexture;
		internal Asset<Texture2D> cancelTexture;
		internal Asset<Texture2D> smallBgTexture;
		public override void OnInitialize()
		{
			if (moreTexture == null)
			{
				moreTexture = ModContent.Request<Texture2D>("AmuletOfManyMinions/UI/Common/MoreIcon", AssetRequestMode.ImmediateLoad);
			}
			if (cancelTexture == null)
			{
				cancelTexture = ModContent.Request<Texture2D>("AmuletOfManyMinions/UI/Common/CancelIcon", AssetRequestMode.ImmediateLoad);
			}
			if(smallBgTexture == null)
			{
				smallBgTexture = ModContent.Request<Texture2D>("AmuletOfManyMinions/UI/Common/BgCircle_Small", AssetRequestMode.ImmediateLoad);
			}
			base.OnInitialize();
			SetupDropdown();
			Append(dropDown);
			SetupRadialMenu();
			Append(radialMenu);
			Left.Pixels = 0f;
			// go from left edge of screen to a bit past the edge of buffs
			Width.Pixels = xMin + (buffsPerLine * buffWidth) + 32;
			Top.Pixels = yMin;
			Height.Pixels = buffRows * buffHeight + 40;
		}

		private void SetupDropdown()
		{
			List<TacticsGroupButton> groupButtons = new List<TacticsGroupButton>();
			int rows = 1;
			int columns = MinionTacticsPlayer.TACTICS_GROUPS_COUNT - 1;

			int width = 40;
			int height = 40;

			//Keep them even numbers, they are divided by 2 later
			int xMargin = 0;
			int yMargin = 0;

			int widthWithMargin = width + xMargin;
			int heightWithMargin = height + yMargin;

			for (int i = 0; i < MinionTacticsPlayer.TACTICS_GROUPS_COUNT - 1; i++)
			{
				TacticsGroupButton button = new TacticsGroupButton(i, quiet: true, radialHover: true);

				//Top left of the position it should insert in
				int yPos = yMargin / 2;
				int xPos = xMargin / 2 + widthWithMargin * i;

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

		private List<RadialMenuButton> GetSharedTacticsButtons()
		{
			List<RadialMenuButton> groupButtons = new List<RadialMenuButton>();
			for(int i = 0; i< MinionTacticsPlayer.TACTICS_GROUPS_COUNT; i++)
			{
				RadialMenuButton button = new RadialMenuButton(
					TextureAssets.WireUi[0], 
					TargetSelectionTacticHandler.GroupTextures[i], 
					radialOffsets[i]);
				groupButtons.Add(button);
			}
			return groupButtons;
		}
		private void SetupRadialMenu()
		{
			List<RadialMenuButton> groupButtons = new List<RadialMenuButton>();
			groupButtons.AddRange(GetSharedTacticsButtons());
			RadialMenuButton button = new RadialMenuButton(
				TextureAssets.WireUi[0], 
				cancelTexture, 
				radialOffsets[MinionTacticsPlayer.TACTICS_GROUPS_COUNT]);
			groupButtons.Add(button);
			radialMenu = new TacticQuickSelectRadialMenu(groupButtons)
			{
				IgnoresMouseInteraction = true
			};
			radialMenu.Width.Pixels = 100;
			radialMenu.Height.Pixels = 100;
		}

		internal void PlaceTacticSelectRadial(Vector2 mouseScreen)
		{
			// place centered about the position
			// this is technically outside of the parent, so we'll see what happens
			radialMenu.Top.Pixels = mouseScreen.Y - Top.Pixels - radialMenu.Height.Pixels / 2 - 4;
			radialMenu.Left.Pixels = mouseScreen.X - radialMenu.Width.Pixels / 2;
			radialMenu.StartShowing();
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			// another menu is open, so don't check
			if(Main.ingameOptionsWindow || Main.playerInventory || 
			  (dropDown.framesUntilHide > 0 && dropDown.ContainsPoint(evt.MousePosition)))
			{
				return; 
			}
			clickedBuffIdx = GetClickedBuffIdx(evt);
			if(clickedBuffIdx == -1)
			{
				return;
			}
			MinionTacticsPlayer myPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
			int buffType = myPlayer.Player.buffType[clickedBuffIdx];
			if(myPlayer.GroupIsSetForMinion(buffType))
			{
				Vector2 buffTop = GetBuffTopLeft(clickedBuffIdx);
				dropDown.Left.Pixels = xMin + buffTop.X - dropDown.Width.Pixels/2 + TextureAssets.Buff[buffType].Width()/2;
				dropDown.Top.Pixels = buffTop.Y + 38;
				dropDown.SetSelected(buffType);
			}
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			radialMenu.IgnoresMouseInteraction = radialMenu.framesUntilHide <= 0; //IMPORANT; OTHERWISE IT WILL CONSUME MOUSE HOVERS
			float buffCount = (float)Main.player[Main.myPlayer].CountBuffs();
			int nextBuffRows = (int)Math.Ceiling(buffCount / buffsPerLine);
			if(buffRows != nextBuffRows)
			{
				Height.Pixels = buffRows * buffHeight + 120;
				buffRows = nextBuffRows;
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
			int buffType = myPlayer.Player.buffType[clickedBuffIdx];
			clickedBuffIdx = -1;

			float mouseY = evt.MousePosition.Y;
			float startY = Top.Pixels + dropDown.Top.Pixels;
			// don't trigger if not within vertical bounds (any horizontal pos is fine)
			if (mouseY < startY || mouseY > startY + dropDown.Height.Pixels)
			{
				return;
			}
			float regionHeight = dropDown.Height.Pixels / (MinionTacticsPlayer.TACTICS_GROUPS_COUNT - 1);
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
			for(int i = 0; i < myPlayer.Player.CountBuffs(); i++)
			{
				int buffType = myPlayer.Player.buffType[i];
				if(myPlayer.GroupIsSetForMinion(buffType))
				{
					int groupIdx = myPlayer.GetGroupForBuff(buffType);
					Vector2 drawPos = topCorner + GetBuffTopLeft(i);
					Texture2D texture = TargetSelectionTacticHandler.GroupOverlayTextures[groupIdx].Value;
					spriteBatch.Draw(texture, drawPos, null, Color.White, 0f, Vector2.Zero, scale, 0f, 0);
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
