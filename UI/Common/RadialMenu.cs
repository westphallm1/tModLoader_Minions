using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.UI;

namespace AmuletOfManyMinions.UI.Common
{
	class RadialMenu : UIElement
	{
		protected readonly List<RadialMenuButton> buttons;

		internal bool doDisplay = false;

		internal int framesUntilHide = 0; // auto-hide if not clicked in a couple seconds

		internal RadialMenu(List<RadialMenuButton> buttons)
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
				Color drawColor = Color.White;
				if(framesUntilHide > 10 || (buttons[i].Highlighted && framesUntilHide > 0))
				{
					if(framesUntilHide < 10)
					{
						drawColor = Color.Multiply(drawColor, framesUntilHide / 10f);
					}
					buttons[i].DrawSelf(spriteBatch, top, drawColor);
				}
			}
		}

		internal virtual void StartShowing()
		{
			framesUntilHide = 180;
			doDisplay = true;
		}

		internal virtual void StopShowing()
		{
			framesUntilHide = 10;
		}
	}
}
