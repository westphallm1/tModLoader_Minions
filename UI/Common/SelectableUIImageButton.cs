using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;

namespace AmuletOfManyMinions.UI.Common
{
	abstract class SelectableUIImageButton : UIImageButtonExtended
	{
		internal const float AlphaOver = 0.9f;
		internal const float AlphaOut = 0.6f;

		internal abstract string ShortHoverText { get; }
		internal abstract string LongHoverText { get; }

		internal abstract Asset<Texture2D> OutlineTexture { get; }
		/// <summary>
		/// Represents if it is the currently selected tactic by the player. Only one tactic can be selected exclusively
		/// </summary>
		internal bool selected = false;

		private int hoverTime = 0;
		private const int StartShowingDescription = 60;

		internal SelectableUIImageButton(Asset<Texture2D> texture) : base(texture)
		{
		}

		public override void OnInitialize()
		{
			SetHoverText(ShortHoverText);
		}

		public override void MouseOut(UIMouseEvent evt)
		{
			hoverTime = 0;
			SetHoverText(ShortHoverText);
			base.MouseOut(evt);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			//Draw the outline when selected. Relies on alpha being 1f when selected otherwise it will look bad
			if (selected && OutlineTexture != null)
			{
				for (int i = 0; i < 4; i++)
				{
					//i: 0 | 1 | 2 | 3 ...
					//x:-1 |-1 | 1 | 1 ...repeat
					//y:-1 | 1 |-1 | 1 ...repeat

					int x = i / 2 % 2 == 0 ? -1 : 1;
					int y = i % 2 == 0 ? -1 : 1;

					DrawInternal(spriteBatch, OutlineTexture.Value, new Vector2(x, y) * 1.5f, Color.White);
				}
			}

			if (IsMouseHovering)
			{
				hoverTime++;
				if (hoverTime == StartShowingDescription)
				{
					//After exactly StartShowingDescription ticks of hovering, change the text
					SetHoverText(LongHoverText);
				}
			}

			base.DrawSelf(spriteBatch);
		}

		internal void SetSelected(bool selected)
		{
			this.selected = selected;
			if (selected)
			{
				SetAlpha(1f, 1f);
			}
			else
			{
				SetAlpha(AlphaOver, AlphaOut);
			}
		}
	}
}
