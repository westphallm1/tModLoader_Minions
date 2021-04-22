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
		// update angles every X frames
		internal static int frameResolution = 5;
		// use the Xth frame of each block for the angle
		internal static int frameShift = 0;
		// snap all offset vectors to a Y x Y grid
		internal static int posResolution = 2;

		internal SpriteBatch spriteBatch;
		internal Color lightColor;

		internal int idleFrame => frameShift + minion.animationFrame - (minion.animationFrame % frameResolution);

		internal int walkFrame => frameShift + walkCycleFrame - (walkCycleFrame % frameResolution);

		internal float WalkCycleAngle => Math.Sign(projectile.velocity.X) * MathHelper.TwoPi * walkFrame / walkCycleFrames;
		internal float IdleCycleAngle => MathHelper.TwoPi * idleFrame / idleCycleFrames;
		internal bool IsWalking => Math.Abs(projectile.velocity.X) > walkVelocityThreshold;

		internal int snapToGrid(float val) => Math.Sign(val) * (int)(Math.Abs(val) / posResolution) *  posResolution;

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

		public Rectangle BoundsForFrame(Texture2D texture, int frame, int frames)
		{
			int frameHeight = texture.Height / frames;
			return new Rectangle(0, frame * frameHeight, texture.Width, frameHeight);
		}

		public void AddSpriteToBatch(Texture2D texture, (int, int) boundsInfo, Vector2 offsetFromCenter, float r, float scale)
		{

			offsetFromCenter = new Vector2(snapToGrid(offsetFromCenter.X), snapToGrid(offsetFromCenter.Y));
			// don't rotate if snapping to grid
			r = posResolution > 1 ? 0 : r;
			float frameOfReferenceR = projectile.rotation + r;
			Vector2 pos = projectile.Center + offsetFromCenter.RotatedBy(frameOfReferenceR);
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			int frameHeight = texture.Height / boundsInfo.Item2;
			Rectangle bounds = new Rectangle(0, boundsInfo.Item1 * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition, bounds, lightColor, frameOfReferenceR, origin, scale, effects, 0);
		}

		public void AddSpriteToBatch(Texture2D texture, Vector2 offsetFromCenter, float r, float scale)
		{
			AddSpriteToBatch(texture, (0, 1), offsetFromCenter, r, scale);
		}

		public void AddSpriteToBatch(Texture2D texture, (int, int) boundsInfo, Vector2 offsetFromCenter)
		{
			AddSpriteToBatch(texture, boundsInfo, offsetFromCenter, 0, 1);
		}
		public void AddSpriteToBatch(Texture2D texture, Vector2 offsetFromCenter)
		{
			AddSpriteToBatch(texture, (0, 1), offsetFromCenter, 0, 1);
		}

		internal void Process(SpriteBatch spriteBatch, Color lightColor, bool isWalking, params SpriteCycleDrawer[] drawers)
		{
			SetDrawInfo(spriteBatch, lightColor);
			if(isWalking)
			{
				for(int i = 0; i < drawers.Length; i++)
				{
					drawers[i].Invoke(this, walkFrame, WalkCycleAngle);
				}
			} else
			{
				for(int i = 0; i < drawers.Length; i++)
				{
					drawers[i].Invoke(this, idleFrame, IdleCycleAngle);
				}
			}
			ClearDrawInfo();
		}
	}
}
