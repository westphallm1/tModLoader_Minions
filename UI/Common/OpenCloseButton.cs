using Microsoft.Xna.Framework.Graphics;

namespace AmuletOfManyMinions.UI.Common
{
	internal class OpenCloseButton : UIImageButtonExtended
	{
		internal OpenCloseButton(Texture2D texture) : base(texture)
		{
			SetAlpha(0.7f, 0.7f); //0.7f is the alpha used for panel background
		}
	}
}
