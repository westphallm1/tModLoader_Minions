using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Squires.StardustSquire
{
	// This is an unfortunate class hierarchy
	class StardustConstellation : TransientMinion
	{
		internal static int TimeToLive = 240;
		internal static int ConstellationSize = 400;
		internal static int BigStarCount = 16;
		internal static int MaxSmallStars = 16;
		internal static float SmallSpawnChance = 0.125f; // 25% chance to spawn a small star each frame

		internal List<ConstellationStar> bigStars;
		internal List<ConstellationSmallStar> smallStars;
		private SpriteCompositionHelper scHelper;
		private Texture2D smallStarTexture;

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.timeLeft = TimeToLive;
			smallStarTexture = ModContent.GetTexture(Texture + "_Small");
			scHelper = new SpriteCompositionHelper(this, new Rectangle(0, 0, ConstellationSize, ConstellationSize))
			{
				idleCycleFrames = projectile.timeLeft,
				frameResolution = 1,
				posResolution = 1
			};
		}
		public override void OnSpawn()
		{
			base.OnSpawn();
			scHelper.Attach();
			SpawnBigStars();
			smallStars = new List<ConstellationSmallStar>();
		}

		private void SpawnBigStars()
		{
			bigStars = new List<ConstellationStar>();
			Vector2 startPoint = Vector2.One * ConstellationSize / 2;
			for (int i = 0; i < BigStarCount; i++)
			{
				bigStars.Add(new ConstellationStar(startPoint, i));
			}
			for (int i = 0; i < BigStarCount; i++)
			{
				bigStars[i].SetConnections(bigStars);
			}
		}

		private void DrawBigStars(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			for(int i = 0; i < bigStars.Count; i++)
			{
				bigStars[i].DrawConnections(Main.projectileTexture[projectile.type], helper.spriteBatch, animationFrame);
			}
			for(int i = 0; i < bigStars.Count; i++)
			{
				bigStars[i].Draw(helper.spriteBatch, animationFrame);
			}
		}

		private void DrawSmallStars(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			// could probably be accomplished with dust
			for(int i = 0; i < smallStars.Count; i++)
			{
				smallStars[i].Draw(helper.spriteBatch, smallStarTexture, animationFrame);
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// no-op
		}

		private void UpdateSmallStars()
		{
			if (animationFrame < 15) 
			{ 
				return; 
			}
			int lastDeadIdx = -1;
			for(int i = 0; i < smallStars.Count; i++)
			{
				if(smallStars[i].IsDead(animationFrame))
				{
					lastDeadIdx = i;
				}
			}
			if(lastDeadIdx > -1)
			{
				// rather inefficient
				smallStars = smallStars.Skip(lastDeadIdx + 1).ToList();
			} 
			if(smallStars.Count < MaxSmallStars && Main.rand.NextFloat() < SmallSpawnChance)
			{
				Vector2 parentPos = bigStars[Main.rand.Next(bigStars.Count)].EndOffset;
				smallStars.Add(new ConstellationSmallStar(animationFrame, parentPos));
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float fadeOutFrames = 30;
			float brightness = projectile.timeLeft > fadeOutFrames ? 1 : projectile.timeLeft / fadeOutFrames;
			scHelper.Draw(spriteBatch, Color.White * brightness);
			return false;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			UpdateSmallStars();
			scHelper.UpdateDrawers(false, DrawSmallStars, DrawBigStars);
		}
	}

	internal struct ConstellationSmallStar
	{
		static int TimeToLive = 60;

		Vector2 position;
		int spawnFrame;

		internal bool IsDead(int frame) => frame - spawnFrame > TimeToLive;

		public ConstellationSmallStar(int spawnFrame, Vector2 center)
		{
			int radius = 16 + Main.rand.Next(48);
			position = center + Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * radius;
			this.spawnFrame = spawnFrame;
		}

		public void Draw(SpriteBatch spriteBatch, Texture2D texture, int frame)
		{
			int timeLeft = frame - spawnFrame;
			if(timeLeft < 0)
			{
				return;
			}
			float brightness = 0.75f * (float)Math.Sin(MathHelper.Pi * timeLeft / TimeToLive);
			spriteBatch.Draw(
				texture, position, texture.Bounds, Color.White * brightness, 0, 
				texture.Bounds.Center.ToVector2(), 0.5f, 0, 0);
		}
	}

	internal struct ConstellationStar
	{
		static int TravelFrames = 15;
		static int AnimFrames = 60;
		static int TierCount = 4; // number of distinct "rings" for stars to appear in
		static int TierSize = StardustConstellation.BigStarCount / TierCount;
		internal static int MaxRange = 3 * StardustConstellation.ConstellationSize / 8;
		static int TierRadius = MaxRange / TierCount;
		static int MaxConnection = 80;

		Vector2 StartPoint;
		internal Vector2 EndOffset;
		Texture2D texture;
		int idx;
		int maxConnections;
		Vector2[] connections;
		
		public ConstellationStar(Vector2 startPoint, int idx)
		{
			this.idx = idx;
			StartPoint = startPoint;
			int tier = idx / TierSize;
			int radius = tier * TierRadius + Main.rand.Next(TierRadius);
			EndOffset = Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * radius;
			texture = ModContent.GetTexture("Terraria/Misc/StarDustSky/Star " + Main.rand.Next(1));
			maxConnections = Main.rand.Next(3) == 0 ? 2 : 1;
			connections = new Vector2[maxConnections];
		}

		public void DrawConnections(Texture2D connectionTexture, SpriteBatch spriteBatch, int frame)
		{
			if(frame < TravelFrames || connections.Length == 0)
			{
				return;
			}
			Vector2 pos = StartPoint + EndOffset;
			int frameOffest = idx * AnimFrames / StardustConstellation.BigStarCount;
			int animFrame = (frame + frameOffest) % AnimFrames;
			float brightness = 0.75f + 0.25f * (float)Math.Sin(MathHelper.TwoPi * animFrame / AnimFrames);
			for(int i = 0; i < connections.Length; i++)
			{
				Vector2 connection = connections[i];
				if(connection == default)
				{
					break;
				}
				Vector2 midPoint = connection / 2;
				int connectionLength = (int)connection.Length();
				Rectangle bounds = new Rectangle(0, 0, connectionLength, connectionTexture.Height);
				float r = connection.ToRotation();
				spriteBatch.Draw(
					connectionTexture, pos + midPoint, bounds, Color.White * brightness, r, 
					bounds.Center.ToVector2(), 1, 0, 0);
			}
		}

		public void Draw(SpriteBatch spriteBatch, int frame)
		{
			Vector2 pos = StartPoint + EndOffset * Math.Min(1, frame / (float)TravelFrames);
			int frameOffest = idx * AnimFrames / StardustConstellation.BigStarCount;
			int animFrame = (frame + frameOffest) % AnimFrames;
			float brightness = 0.625f + 0.125f * (float)Math.Sin(MathHelper.TwoPi * animFrame / AnimFrames);
			float r = 0; // todo oscillate
			float scale = 0.5f; // todo oscillate
			spriteBatch.Draw(
				texture, pos, texture.Bounds, Color.White * brightness, r, 
				texture.Bounds.Center.ToVector2(), scale, 0, 0);
		}

		public void SetConnections(List<ConstellationStar> others)
		{
			Vector2 myEnd = EndOffset;
			int myIdx = idx;
			Vector2[] newConnections = others
				.Where(v => v.idx > myIdx)
				.Select(o => o.EndOffset - myEnd)
				.Where(v => v.LengthSquared() < MaxConnection * MaxConnection)
				.OrderBy(v => v.LengthSquared()).Take(maxConnections)
				.ToArray();
			// a bit hacky
			for(int i = 0; i < newConnections.Length; i++)
			{
				connections[i] = newConnections[i];
			}
		}
	}
}
