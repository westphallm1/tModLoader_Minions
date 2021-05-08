using AmuletOfManyMinions.Core.Minions;
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
	class TacticQuickSelectRadialMenu : UIElement
	{
		private readonly List<RadialMenuButton> buttons;

		private bool doDisplay = false;

		internal int framesUntilHide = 0; // auto-hide if not clicked in a couple seconds

		internal TacticQuickSelectRadialMenu(List<RadialMenuButton> buttons, Vector2[] buttonOffsets)
		{
			this.buttons = buttons;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if(framesUntilHide -- <= 0 || Main.ingameOptionsWindow || Main.playerInventory)
			{
				doDisplay = false;
			}
			Vector2 top = new Vector2(GetDimensions().X, GetDimensions().Y);
			for(int i = 0; i < buttons.Count; i++)
			{
				RadialMenuButton button = buttons[i];
				button.Update(top);
				if(button.LeftClicked || button.RightClicked)
				{
					Main.LocalPlayer.GetModPlayer<MinionTacticsPlayer>().SetTacticsGroup(i);
					UnsetSelected();
					break;
				}
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if(!doDisplay)
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
			// draw background buttons
			Vector2 top = new Vector2(GetDimensions().X, GetDimensions().Y);
			for(int i = 0; i < buttons.Count; i++)
			{
				buttons[i].DrawSelf(spriteBatch, top);
			}
		}

		/// <summary>
		/// This gets called each time the dropdown moves to a new minion buff
		/// </summary>
		/// <param name="id">Tactic ID</param>
		internal void StartShowing()
		{
			MinionTacticsPlayer tacticsPlayer = Main.player[Main.myPlayer].GetModPlayer<MinionTacticsPlayer>();
			framesUntilHide = 180;
			doDisplay = true;
			for(int i = 0; i < MinionTacticsPlayer.TACTICS_GROUPS_COUNT; i++)
			{
				buttons[i].Highlighted = tacticsPlayer.CurrentTacticGroup == i;
			}
		}

		internal void UnsetSelected()
		{
			framesUntilHide = Math.Min(framesUntilHide, 2);
		}
	}
}
