using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.Effects
{
	class MotionBlurHelper
	{
		// something in the ai overrides seems to prevent projectile.oldPos from populating properly,
		// so just replicate it manually
		private Vector2[] myOldPos;
		public int BlurLength { get; private set; }

		private bool isCleared;
		public MotionBlurHelper(int blurLength)
		{
			BlurLength = blurLength;
			myOldPos = new Vector2[blurLength];
		}



		public bool GetBlurPosAndColor(int idx, Color lightColor, out Vector2 blurPos, out Color blurColor)
		{
			blurPos = myOldPos[idx];
			blurColor = lightColor * ((myOldPos.Length - idx) / (float)myOldPos.Length);
			return myOldPos[idx] != default;
		}

		public void Clear()
		{
			if(isCleared)
			{
				return;
			}
			isCleared = true;
			myOldPos = new Vector2[BlurLength];
		}

		public void Update(Vector2 position, bool addPosition = true)
		{
			if(!addPosition)
			{
				Clear();
			} else
			{
				isCleared = false;
				for(int i = myOldPos.Length -1; i > 0; i--)
				{
					myOldPos[i] = myOldPos[i - 1];
				}
				myOldPos[0] = position;
			}
		}
	}
}
