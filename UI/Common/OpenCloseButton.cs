using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace AmuletOfManyMinions.UI.Common
{
	internal class OpenCloseButton : UIImageButtonExtended
	{
		internal OpenCloseButton(Asset<Texture2D> texture) : base(texture)
		{
			SetAlpha(0.85f, 0.85f); //0.7f is the alpha used for panel background
		}
	}
}
