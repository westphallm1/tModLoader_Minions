using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.UI.Common
{
	class RadialMenuButton
	{
		public Texture2D bgTexture;
		public Texture2D fgTexture;
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

		public RadialMenuButton(Texture2D bgTexture, Texture2D fgTexture, Vector2 relativeTopLeft)
		{
			this.bgTexture = bgTexture;
			this.fgTexture = fgTexture;
			this.bgRelativeTopLeft = relativeTopLeft;
			bounds = bgTexture.Bounds;
			this.relativeCenter = relativeTopLeft + bounds.Center();
			this.fgRelativeTopLeft = relativeCenter - fgTexture.Bounds.Center();
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
				OnLeftClick?.Invoke();
			}
			if(RightClicked)
			{
				OnRightClick?.Invoke();
			}
			lastMouseLeft = Main.mouseLeft;
			lastMouseRight = Main.mouseRight;

		}

		public void DrawSelf(SpriteBatch spriteBatch, Vector2 absoluteTopLeft)
		{
			Vector2 bgDrawPos = absoluteTopLeft + bgRelativeTopLeft;
			Vector2 fgDrawPos = absoluteTopLeft + fgRelativeTopLeft;
			Color color = (MouseHover || Highlighted) ? Color.White : Color.Gray;
			spriteBatch.Draw(bgTexture, bgDrawPos, null, color, 0f, Vector2.Zero, 1, 0f, 0f);
			spriteBatch.Draw(fgTexture, fgDrawPos, null, color, 0f, Vector2.Zero, 1, 0f, 0f);
		}
	}
}
