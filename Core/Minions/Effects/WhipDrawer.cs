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
	public struct WhipDrawer
	{
		// not customizable enough to warrent an abstract class
		public FrameSelector frameSelector;

		private int duration;


		public WhipDrawer(FrameSelector selector, int duration)
		{
			frameSelector = selector;
			this.duration = duration;
		}

		public void ApplyWhipSegments(Vector2 startPos, Vector2 endPos, int frame, Action<Vector2, float, Rectangle> perSegment)
		{
			int segmentLength = frameSelector.Invoke(0, false).Width;
			int maximalExtensionFrame = duration / 2;
			float rotationMult = (maximalExtensionFrame - frame) / (float)maximalExtensionFrame;
			float lengthMult = 0.75f + 0.5f * MathF.Sin(MathHelper.Pi * frame / maximalExtensionFrame);

			Vector2 chainVector = lengthMult * (endPos - startPos);
			float drawLength = chainVector.Length();
			int segmentCount = (int)Math.Ceiling(drawLength / segmentLength);
			Vector2 pos = startPos;
			Vector2 currentSegment = chainVector;
			currentSegment.Normalize();
			currentSegment *= segmentLength;
			float roationPerSegment = -Math.Sign(chainVector.X) * rotationMult * MathHelper.Pi / segmentCount;
			for (int i = 0; i < segmentCount; i++)
			{
				Rectangle bounds = frameSelector.Invoke(i, i == segmentCount - 1);
				Vector2 midPoint = pos + currentSegment / 2;
				float r = currentSegment.ToRotation();
				perSegment.Invoke(midPoint, r, bounds);
				// move to the next segment
				pos += currentSegment;
				currentSegment = currentSegment.RotatedBy(roationPerSegment);
			}
		}

		public void DrawWhip(Texture2D texture, Vector2 startPos, Vector2 endPos, int frame, Color lightColor = default)
		{
			ApplyWhipSegments(startPos, endPos, frame, (midPoint, rotation, bounds) =>
			{
				Vector2 origin = new(bounds.Width / 2, bounds.Height / 2);
				lightColor = lightColor == default ? Lighting.GetColor((int)midPoint.X / 16, (int)midPoint.Y / 16) : lightColor;
				Main.EntitySpriteDraw(texture, midPoint - Main.screenPosition,
					bounds, lightColor, rotation,
					origin, 1, SpriteEffects.None, 0);
			});
		}
	}
}
