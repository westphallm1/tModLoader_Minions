using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	internal delegate void SpriteCycleDrawer(SpriteCompositionHelper helper, int frame, float angle);

	class SpriteCompositionHelper
	{
		private SimpleMinion minion;

		private Projectile projectile => minion.projectile;

		internal int walkCycleFrames = 60;
		internal int idleCycleFrames = 90;
		internal int walkCycleFrame = 0;
		internal int walkVelocityThreshold = 1;

		internal SpriteBatch spriteBatch;
		internal Color lightColor;

		internal float WalkCycleAngle => Math.Sign(projectile.velocity.X) * MathHelper.TwoPi * (walkCycleFrame % walkCycleFrames) / walkCycleFrames;
		internal float IdleCycleAngle => MathHelper.TwoPi * (minion.animationFrame % idleCycleFrames) / idleCycleFrames;
		internal bool IsWalking => Math.Abs(projectile.velocity.X) > walkVelocityThreshold;

		public SpriteCompositionHelper(SimpleMinion minion)
		{
			this.minion = minion;
		}

		public void UpdateMovement()
		{
			if(Math.Abs(projectile.velocity.X) > walkVelocityThreshold)
			{
				walkCycleFrame++;
			} else
			{
				walkCycleFrame = 0;
			}
		}

		public void SetDrawInfo(SpriteBatch spriteBatch, Color lightColor)
		{
			this.spriteBatch = spriteBatch;
			this.lightColor = lightColor;
		}

		public void ClearDrawInfo()
		{
			// don't hang onto reference for too long
			this.spriteBatch = null;
		}

		public void AddSpriteToBatch(Texture2D texture, Rectangle bounds, Vector2 offsetFromCenter, float r, float scale)
		{
			float frameOfReferenceR = projectile.rotation + r;
			Vector2 pos = projectile.Center + offsetFromCenter.RotatedBy(frameOfReferenceR);
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition, bounds, lightColor, frameOfReferenceR, origin, scale, effects, 0);
		}

		public void AddSpriteToBatch(Texture2D texture, Vector2 offsetFromCenter, float r, float scale)
		{
			AddSpriteToBatch(texture, texture.Bounds, offsetFromCenter, r, scale);
		}

		public void AddSpriteToBatch(Texture2D texture, Rectangle bounds, Vector2 offsetFromCenter)
		{
			AddSpriteToBatch(texture, bounds, offsetFromCenter, 0, 1);
		}
		public void AddSpriteToBatch(Texture2D texture, Vector2 offsetFromCenter)
		{
			AddSpriteToBatch(texture, texture.Bounds, offsetFromCenter, 0, 1);
		}

		internal void Process(SpriteBatch spriteBatch, Color lightColor, bool isWalking, params SpriteCycleDrawer[] drawers)
		{
			SetDrawInfo(spriteBatch, lightColor);
			if(isWalking)
			{
				for(int i = 0; i < drawers.Length; i++)
				{
					drawers[i].Invoke(this, walkCycleFrame, WalkCycleAngle);
				}
			} else
			{
				for(int i = 0; i < drawers.Length; i++)
				{
					drawers[i].Invoke(this, minion.animationFrame, IdleCycleAngle);
				}
			}
			ClearDrawInfo();
		}
	}
}
