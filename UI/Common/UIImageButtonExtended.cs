using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace AmuletOfManyMinions.UI.Common
{
	/// <summary>
	/// Same as UIImageButton from vanilla, but has no sound on mouseover, a hover text, and can't be clicked outside of the screens top/left edges
	/// </summary>
	public class UIImageButtonExtended : UIElement
	{
		protected Texture2D texture;
		private float alphaOver = 1f;
		private float alphaOut = 0.4f;

		private string hoverText = "";

		// need to override in TacticsGroupButton
		public virtual bool InHoverState => IsMouseHovering;
		public UIImageButtonExtended(Texture2D texture)
		{
			SetImage(texture);
			Recalculate();
		}

		public override void Click(UIMouseEvent evt)
		{
			if (evt.MousePosition.X < 0 || evt.MousePosition.Y < 0) return;

			base.Click(evt);
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			if (evt.MousePosition.X < 0 || evt.MousePosition.Y < 0) return;

			base.MouseOver(evt);
		}

		protected void DrawInternal(SpriteBatch spriteBatch, Texture2D texture, Vector2 off = default, Color color = default)
		{
			if (color == default) color = Color.White;

			spriteBatch.Draw(position: GetDimensions().Position() + off, texture: texture, color: color * (InHoverState ? alphaOver : alphaOut));
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			DrawInternal(spriteBatch, texture);

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);
				Main.hoverItemName = hoverText;
			}
		}

		public void SetImage(Texture2D texture)
		{
			this.texture = texture;
			Width.Pixels = this.texture.Width;
			Height.Pixels = this.texture.Height;
		}

		public void SetHoverText(string hoverText)
		{
			this.hoverText = hoverText;
		}

		public void SetAlpha(float whenOver, float whenOut)
		{
			alphaOver = MathHelper.Clamp(whenOver, 0f, 1f);
			alphaOut = MathHelper.Clamp(whenOut, 0f, 1f);
		}
	}
}