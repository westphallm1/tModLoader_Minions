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
	// needs a per-minion implementation
	public abstract class WormDrawer
	{
		private float[] backingArray;
		internal int frame;
		internal CircularLengthQueue PositionLog;
		protected SpriteBatch spriteBatch;
		protected Texture2D texture;
		protected Color lightColor;

		internal int SegmentCount { get; set; } = 0;

		public WormDrawer(int segmentCapacity = 512, int queueSize = 255, int maxLength = 1200)
		{
			backingArray = new float[segmentCapacity];
			CircularVectorQueue.Initialize(backingArray);
			PositionLog = new CircularLengthQueue(backingArray, queueSize: queueSize, maxLength: maxLength);
		}

		protected virtual SpriteEffects GetEffects(float angle)
		{
			SpriteEffects effects = SpriteEffects.FlipHorizontally;
			angle = (angle + 2 * (float)Math.PI) % (2 * (float)Math.PI); // get to (0, 2PI) range
			if (angle > Math.PI / 2 && angle < 3 * Math.PI / 2)
			{
				effects |= SpriteEffects.FlipVertically;
			}
			return effects;
		}

		public virtual void Draw(Texture2D texture, SpriteBatch spriteBatch, Color lightColor)
		{
			this.texture = texture;
			this.spriteBatch = spriteBatch;
			this.lightColor = lightColor;

			DrawTail();
			DrawBody();
			DrawHead();
		}

		protected abstract void DrawTail();
		protected abstract void DrawBody();
		protected abstract void DrawHead();
		protected virtual void AddSprite(float dist, Rectangle bounds, Color c = default)
		{
			Vector2 origin = new Vector2(bounds.Width / 2f, bounds.Height / 2f);
			Vector2 angle = new Vector2();
			Vector2 pos = PositionLog.PositionAlongPath(dist, ref angle);
			float r = angle.ToRotation();
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, c == default ? lightColor : c, r,
				origin, 1, GetEffects(r), 0);
		}

		public void AddPosition(Vector2 position)
		{
			PositionLog.AddPosition(position);
		}

		public void Update(int frame)
		{
			this.frame = frame;
		}
	}
}
