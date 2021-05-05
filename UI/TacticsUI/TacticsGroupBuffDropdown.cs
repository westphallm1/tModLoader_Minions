using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Tactics.TacticsGroups;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace AmuletOfManyMinions.UI.TacticsUI
{
	// largely lifted from TacticsGroupPanel, a decent bit of cruft left over
	class TacticsGroupBuffDropdown : UIElement
	{
		private readonly List<TacticsGroupButton> buttons;

		private int selectedIndex; //From the buttons list
		private int currentBuffId = -1;

		internal int framesUntilHide = 0; // auto-hide if not clicked in a couple seconds

		internal TacticsGroupBuffDropdown(List<TacticsGroupButton> buttons)
		{
			this.buttons = buttons;
		}

		public override void OnInitialize()
		{
			foreach (var button in buttons)
			{
				button.OnClick += Button_OnClick;
				//Padding of the panel screws up alignment for the buttons, so revert it
				button.Top.Pixels -= this.PaddingTop;
				button.Left.Pixels -= this.PaddingLeft;
				Append(button);
			}
		}

		private void Button_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			if(currentBuffId == -1)
			{
				return;
			}
			if (listeningElement is TacticsGroupButton clickedButton)
			{
				if (selectedIndex != clickedButton.index)
				{
					selectedIndex = clickedButton.index;
					Main.PlaySound(SoundID.MenuTick);
				}

				//Recalculate selected status for each button, and set players tactic
				foreach (var button in buttons)
				{
					bool selected = selectedIndex == button.index;
					button.SetSelected(selected);

					if (selected)
					{
						Main.LocalPlayer.GetModPlayer<MinionTacticsPlayer>().SetGroupForMinion(button.index, currentBuffId);
					}
				}
				UnsetSelected();
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if(currentBuffId == -1)
			{
				return;
			}
			if (ContainsPoint(Main.MouseScreen))
			{
				//Prevents drawing item textures on mouseover (bed, selected tool etc.)
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);
			}
			base.DrawSelf(spriteBatch);
			// Draw panels
			Texture2D bgTexture = Main.wireUITexture[0];
			Vector2 top = new Vector2(GetDimensions().X, GetDimensions().Y);
			float scale = 1;
			for(int i = 0; i < buttons.Count; i++)
			{
				bool shouldHover = buttons[i].selected || buttons[i].InHoverState;
				// extremely hardcoded
				Vector2 drawPos = top + i * new Vector2(40, 0);
				Color color = shouldHover ? Color.White : Color.Gray;
				spriteBatch.Draw(bgTexture, drawPos, null, color, 0f, Vector2.Zero, scale, 0f, 0f);
			}
		}

		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			if(currentBuffId == -1)
			{
				return;
			}
			base.DrawChildren(spriteBatch);
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if(framesUntilHide -- <= 0 || Main.ingameOptionsWindow || Main.playerInventory)
			{
				currentBuffId = -1;
			}
		}

		/// <summary>
		/// This gets called each time the dropdown moves to a new minion buff
		/// </summary>
		/// <param name="id">Tactic ID</param>
		internal void SetSelected(int buffId)
		{
			currentBuffId = buffId;
			MinionTacticsPlayer tacticsPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
			int tacticIdx = tacticsPlayer.GetGroupForBuff(buffId);
			framesUntilHide = 180;
			foreach (var button in buttons)
			{
				bool selected = button.index == tacticIdx;
				button.SetSelected(selected);
			}
		}

		internal void UnsetSelected()
		{
			framesUntilHide = Math.Min(framesUntilHide, 5);
		}

	}
}
