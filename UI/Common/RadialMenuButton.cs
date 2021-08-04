using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using ReLogic.Content;

namespace AmuletOfManyMinions.UI.Common
{
	class RadialMenuButton
	{
		public Asset<Texture2D> bgTexture;
		public Asset<Texture2D> fgTexture;
		private Vector2 bgRelativeTopLeft;
		private Vector2 fgRelativeTopLeft;
		private Vector2 relativeCenter;
		// Where on the screen the rectangle is
		public Rectangle bounds;

		// why implement by hand instead of using UI builtins? good question
		internal bool MouseHover { get; private set; }
		internal bool LeftClicked { get; private set; }
		internal bool RightClicked { get; private set; }
		internal Action OnLeftClick;
		internal Action OnRightClick;

		internal bool Highlighted;

		private bool lastMouseLeft;
		private bool lastMouseRight;

		public RadialMenuButton(Asset<Texture2D> bgTexture, Asset<Texture2D> fgTexture, Vector2 relativeTopLeft)
		{
			this.bgTexture = bgTexture;
			this.fgTexture = fgTexture;
			this.bgRelativeTopLeft = relativeTopLeft;
			bounds = bgTexture.Frame(1, 1);
			this.relativeCenter = relativeTopLeft + bounds.Center();
			this.fgRelativeTopLeft = relativeCenter - fgTexture.Frame(1, 1).Center();
		}

		public void Update(Vector2 absoluteTopLeft)
		{
			// roughly circular, so use mouse radius from center to determine 'mouseover'
			Vector2 absoluteCenter = absoluteTopLeft + relativeCenter;
			int radius = (bounds.Width + bounds.Height) / 4;
			MouseHover = (Main.MouseScreen - absoluteCenter).LengthSquared() < radius * radius;
			LeftClicked = MouseHover && lastMouseLeft && Main.mouseLeftRelease;
			RightClicked = MouseHover && lastMouseRight && Main.mouseRightRelease;
			if(LeftClicked)
			{
				// this fixes so many issues...
				Main.isMouseLeftConsumedByUI = true;
				SoundEngine.PlaySound(SoundID.MenuTick);
				OnLeftClick?.Invoke();
			}
			if(RightClicked)
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				OnRightClick?.Invoke();
			}
			lastMouseLeft = Main.mouseLeft;
			lastMouseRight = Main.mouseRight;

		}

		public void DrawSelf(SpriteBatch spriteBatch, Vector2 absoluteTopLeft, Color color = default)
		{
			Vector2 bgDrawPos = absoluteTopLeft + bgRelativeTopLeft;
			Vector2 fgDrawPos = absoluteTopLeft + fgRelativeTopLeft;
			color = color == default ? Color.White : color;
			if(!(Highlighted || MouseHover))
			{
				color = Color.Multiply(color, 0.6f);
			}
			spriteBatch.Draw(bgTexture.Value, bgDrawPos, null, color, 0f, Vector2.Zero, 1, 0f, 0);
			spriteBatch.Draw(fgTexture.Value, fgDrawPos, null, color, 0f, Vector2.Zero, 1, 0f, 0);
		}
	}
}
