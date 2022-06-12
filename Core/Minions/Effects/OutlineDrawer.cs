using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.Effects
{
	internal struct OutlineDrawer
	{

		// TODO do we need any state here?
		public static void DrawOutline(Texture2D texture, Vector2 pos, Rectangle bounds, Color outlineColor, float rotation, SpriteEffects effects)
		{
			for(int i = -1; i <= 1; i+= 1)
			{
				for(int j = -1; j <= 1; j+= 1)
				{
					Vector2 offset = 2 * new Vector2(i, j).RotatedBy(rotation);
					Main.EntitySpriteDraw(texture, pos + offset,
						bounds, outlineColor, rotation, bounds.GetOrigin(), 1, effects, 0);
				}
			}
		}

	}
}
