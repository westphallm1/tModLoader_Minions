using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.Effects
{
	// needs a per-minion implementation
	public abstract class WormDrawer
	{
		private float[] backingArray;
		internal int frame;
		internal CircularLengthQueue PositionLog;
		protected Asset<Texture2D> texture;
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

		public virtual void Draw(Asset<Texture2D> texture, Color lightColor)
		{
			this.texture = texture;
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
			Vector2 angle = new Vector2();
			Vector2 pos = PositionLog.PositionAlongPath(dist, ref angle);
			float r = angle.ToRotation();
			Main.EntitySpriteDraw(texture.Value, pos - Main.screenPosition,
				bounds, c == default ? lightColor : c, r,
				bounds.GetOrigin(), 1, GetEffects(r), 0);
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

	/// <summary>
	/// Used for drawing worms with vertically oriented spritesheets, mostly vanilla clones
	/// </summary>
	public abstract class VerticalWormDrawer : WormDrawer
	{
		protected override SpriteEffects GetEffects(float angle)
		{
			SpriteEffects effects = SpriteEffects.FlipVertically;
			angle = (angle + 2 * (float)Math.PI) % (2 * (float)Math.PI); // get to (0, 2PI) range
			if (angle > Math.PI / 2 && angle < 3 * Math.PI / 2)
			{
				effects |= SpriteEffects.FlipHorizontally;
			}
			return effects;
		}

		protected override void AddSprite(float dist, Rectangle bounds, Color c = default)
		{
			Vector2 angle = new Vector2();
			Vector2 pos = PositionLog.PositionAlongPath(dist, ref angle);
			float r = angle.ToRotation();
			Main.EntitySpriteDraw(texture.Value, pos - Main.screenPosition,
				bounds, c == default ? lightColor : c, r + MathHelper.PiOver2,
				bounds.GetOrigin(), 1, GetEffects(r), 0);
		}

	}
}
