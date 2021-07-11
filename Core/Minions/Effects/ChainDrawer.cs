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
	public struct ChainDrawer
	{
		// not customizable enough to warrent an abstract class
		internal Rectangle EvenFrame;
		internal Rectangle OddFrame;

		public ChainDrawer(Rectangle evenFrame, Rectangle oddFrame)
		{
			EvenFrame = evenFrame;
			OddFrame = oddFrame;
		}
		public ChainDrawer(Rectangle evenFrame)
		{
			EvenFrame = evenFrame;
			OddFrame = evenFrame;
		}

		public void DrawChain(SpriteBatch spriteBatch, Texture2D texture, Vector2 startPos, Vector2 endPos, Color lightColor = default)
		{
			Vector2 chainVector = endPos - startPos;
			Rectangle bounds = EvenFrame;
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Vector2 pos;
			float r;
			if (chainVector.Length() > 16)
			{
				Vector2 unitToIdle = chainVector;
				unitToIdle.Normalize();
				r = (float)Math.PI / 2 + chainVector.ToRotation();
				for (int i = bounds.Height / 2; i < chainVector.Length(); i += bounds.Height)
				{
					if (chainVector.Length() - i < bounds.Height / 2)
					{
						i = (int)(chainVector.Length() - bounds.Height / 2);
					}
					bounds = bounds == EvenFrame ? OddFrame : EvenFrame;
					pos = startPos + unitToIdle * i;
					lightColor = lightColor == default ? Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16) : lightColor;
					spriteBatch.Draw(texture, pos - Main.screenPosition,
						bounds, lightColor, r,
						origin, 1, SpriteEffects.None, 0);
				}
			}

		}
	}
}
