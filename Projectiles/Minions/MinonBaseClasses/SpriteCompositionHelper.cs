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

	internal class SpriteCompositionManager
	{
		internal static HashSet<SpriteCompositionHelper> activeHelpers;
		public static void Load()
		{
			activeHelpers = new HashSet<SpriteCompositionHelper>();
			Main.OnPreDraw += OnPreDraw;

		}

		private static void OnPreDraw(GameTime gameTime)
		{
			// clear out all the helpers for despawned projectiles
			foreach(SpriteCompositionHelper helper in activeHelpers.Where(h => h.projectile != null && !h.projectile.active && h.renderTarget != null))
			{
				helper.renderTarget.Dispose();
				helper.renderTarget = null;
			}
			activeHelpers.RemoveWhere(h => !h.projectile.active);
			foreach(SpriteCompositionHelper helper in activeHelpers)
			{
				helper.Process();
			}

		}

		public static void Unload()
		{
			activeHelpers = null;
			Main.OnPreDraw -= OnPreDraw;
		}
	}


	class SpriteCompositionHelper
	{
		private SimpleMinion minion;
		private Rectangle bounds;
		internal RenderTarget2D renderTarget;

		internal Projectile projectile => minion.Projectile;

		internal int walkCycleFrames = 60;
		internal int idleCycleFrames = 90;
		internal int walkCycleFrame = 0;
		internal int walkVelocityThreshold = 1;
		// update angles every X frames
		internal int frameResolution = 5;
		// use the Xth frame of each block for the angle
		internal static int frameShift = 0;
		// snap all offset vectors to a Y x Y grid
		internal int posResolution = 2;

		internal Vector2? positionOverride { get; set; }
		private Vector2 Center => positionOverride ?? projectile.Center;

		internal Vector2 CenterOfRotation = Vector2.Zero;
		internal Vector2 BaseOffset = Vector2.Zero;

		internal SpriteBatch spriteBatch;
		internal Color lightColor;
		private bool _isWalking;
		private SpriteCycleDrawer[] drawers;

		internal int idleFrame => frameShift + minion.animationFrame - (minion.animationFrame % frameResolution);

		internal int walkFrame => frameShift + walkCycleFrame - (walkCycleFrame % frameResolution);

		internal float WalkCycleAngle => MathHelper.TwoPi * walkFrame / walkCycleFrames;
		internal float IdleCycleAngle => MathHelper.TwoPi * idleFrame / idleCycleFrames;
		internal bool IsWalking => Math.Abs(projectile.velocity.X) > walkVelocityThreshold;

		internal int snapToGrid(float val) => Math.Sign(val) * (int)(Math.Abs(val) / posResolution) *  posResolution;

		internal static Rectangle DefaultBounds = new Rectangle(0, 0, 120, 120);

		public SpriteCompositionHelper(SimpleMinion minion, Rectangle bounds = default)
		{
			if (Main.dedServ) { return; }
			this.minion = minion;
			this.bounds = bounds == default ? DefaultBounds : bounds;
			renderTarget = new RenderTarget2D(
				Main.graphics.GraphicsDevice, 
				this.bounds.Width, 
				this.bounds.Height, 
				false, 
				SurfaceFormat.Color, 
				DepthFormat.None, 
				0, 
				RenderTargetUsage.PreserveContents);
		}

		public void Attach()
		{
			if (Main.dedServ) { return; }
			SpriteCompositionManager.activeHelpers.Add(this);
		}

		public void UpdateMovement()
		{
			if (Main.dedServ) { return; }
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
			if (Main.dedServ) { return; }
			this.spriteBatch = spriteBatch;
			this.lightColor = lightColor;
		}

		public void ClearDrawInfo()
		{
			if (Main.dedServ) { return; }
			// don't hang onto reference for too long
			this.spriteBatch = null;
		}

		public Rectangle BoundsForFrame(Texture2D texture, int frame, int frames)
		{
			int frameHeight = texture.Height / frames;
			return new Rectangle(0, frame * frameHeight, texture.Width, frameHeight);
		}

		public void AddSpriteToBatch(Texture2D texture, Rectangle bounds, Vector2 offsetFromCenter, float r, float scale)
		{
			if (Main.dedServ) { return; }
			// offsetFromCenter -= CenterOfRotation;
			offsetFromCenter = new Vector2(snapToGrid(offsetFromCenter.X), snapToGrid(offsetFromCenter.Y)) + BaseOffset;
			r = posResolution > 1 ? 0 : r; // don't rotate if snapping to grid
			Vector2 pos = this.bounds.Center.ToVector2() + offsetFromCenter.RotatedBy(r);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(texture, pos, bounds, lightColor, r, origin, scale, 0, 0);
		}

		public void AddSpriteToBatch(Texture2D texture, (int, int) boundsInfo, Vector2 offsetFromCenter, float r, float scale)
		{
			int frameHeight = texture.Height / boundsInfo.Item2;
			Rectangle bounds = new Rectangle(0, boundsInfo.Item1 * frameHeight, texture.Width, frameHeight);
			AddSpriteToBatch(texture, bounds, offsetFromCenter, r, scale);
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

		public void AddTileSpritesToBatch(Texture2D texture, int drawIdx, (byte, byte)?[,,] tilesInfo, Vector2 offsetFromCenter, float r = 0, int tileSize = 16)
		{
			if (Main.dedServ) { return; }
			offsetFromCenter += BaseOffset;
			// offsetFromCenter -= CenterOfRotation;
			int tileSpacing = tileSize+2;
			// don't rotate if snapping to grid
			r = posResolution > 1 ? 0 : r;
			Vector2 pos = bounds.Center.ToVector2() + offsetFromCenter.RotatedBy(r);
			Vector2 foRX = Vector2.UnitY.RotatedBy(r) * tileSize;
			Vector2 foRY = Vector2.UnitX.RotatedBy(r) * tileSize;
			int xLength = tilesInfo.GetLength(1);
			int yLength = tilesInfo.GetLength(2);
			Vector2 startOffset = -(xLength / 2f) * foRX + -(yLength / 2f) * foRY;
			for(int i = 0; i < xLength; i++)
			{
				for(int j = 0; j < yLength; j++)
				{
					if(tilesInfo[drawIdx, i, j] == null)
					{
						continue;
					}
					(byte, byte) current = ((byte, byte))tilesInfo[drawIdx, i, j];

					Rectangle bounds = new Rectangle(tileSpacing * current.Item1, tileSpacing * current.Item2, tileSize, tileSize);
					Vector2 currentOffset = startOffset + foRX * (i + 0.5f) + foRY * (j + 0.5f);
					Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
					Main.EntitySpriteDraw(texture, pos + currentOffset, bounds, lightColor, r, origin, 1, 0, 0);
				}
			}
		}
		internal void UpdateDrawers(bool isWalking, params SpriteCycleDrawer[] drawers)
		{
			_isWalking = isWalking;
			this.drawers = drawers;
		}

		internal void Process()
		{
			// don't draw if server, or not an update frame, or there are no drawers
			if(Main.dedServ || minion.animationFrame % frameResolution != 0 || drawers == null || drawers.Length == 0)
			{
				return;
			}
			var spriteBatch = Main.spriteBatch;
			SetDrawInfo(spriteBatch, Color.White);
			Main.instance.GraphicsDevice.SetRenderTarget(renderTarget);
			Main.instance.GraphicsDevice.Clear(Color.Transparent);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			if(_isWalking)
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
			spriteBatch.End();
			Main.instance.GraphicsDevice.SetRenderTarget(null);
			ClearDrawInfo();
		}

		internal void Draw(Color lightColor)
		{
			if(renderTarget == null)
			{
				return; // need this check here for some reason, should probably investigate further
			}
			SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Vector2 pos = Center - Main.screenPosition - BaseOffset + CenterOfRotation.RotatedBy(projectile.rotation);
			Main.EntitySpriteDraw(renderTarget, pos, 
				bounds, lightColor, projectile.rotation, bounds.Center(), 1, effects, 0);
		}
	}
}
