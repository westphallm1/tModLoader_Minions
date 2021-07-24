using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Minions;
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
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.StardustSquire
{


	/// <summary>
	/// Uses ai[0], ai[1] to store target position
	/// </summary>
	class ConstellationSeed : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.Twinkle;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 56;
			projectile.height = 56;
			projectile.penetrate = -1;
			projectile.friendly = false;
			projectile.tileCollide = false;
			projectile.usesLocalNPCImmunity = true;
			projectile.timeLeft = 60;
		}

		public override void AI()
		{
			projectile.rotation = projectile.velocity.ToRotation();
			Vector2 targetPos = new Vector2(projectile.ai[0], projectile.ai[1]);
			if(Vector2.DistanceSquared(projectile.Center, targetPos) < 32 * 32)
			{
				projectile.Kill();
			}
			for (int i = 0; i < 5; i++)
			{
				Vector2 velocity = Vector2.Zero;
				Dust.NewDust(projectile.Center, 1, 1, DustType<MovingWaypointDust>(), velocity.X, velocity.Y, newColor: Color.DeepSkyBlue, Scale: 0.9f);
			}
		}

		public override void Kill(int timeLeft)
		{
			Vector2 targetPos = new Vector2(projectile.ai[0], projectile.ai[1]);
			if(projectile.owner == Main.myPlayer)
			{
				Projectile.NewProjectile(
					targetPos,
					Vector2.Zero,
					ProjectileType<StardustConstellation>(),
					projectile.damage,
					projectile.knockBack,
					projectile.owner);
			}
		}
	}
	/// <summary>
	/// Uses ai[0] for target
	/// </summary>
	class StardustHomingStar : ModProjectile
	{
		NPC target;
		float baseVelocity;
		MotionBlurDrawer blurDrawer;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.penetrate = 1;
			projectile.friendly = true;
			projectile.tileCollide = false;
			projectile.usesLocalNPCImmunity = true;
			projectile.timeLeft = 300;
			blurDrawer = new MotionBlurDrawer(10);
		}

		public override void AI()
		{
			base.AI();
			if(projectile.ai[0] == 0)
			{
				return; // failsafe in case we got a bad NPC index
			}
			if(target == null)
			{
				target = Main.npc[(int)projectile.ai[0]];
				baseVelocity = projectile.velocity.Length();
			}
			if(!target.active)
			{
				projectile.Kill();
				return;
			}
			Vector2 vectorToTarget = target.Center - projectile.Center;
			float distanceToTarget = vectorToTarget.Length();
			if(distanceToTarget > baseVelocity)
			{
				vectorToTarget.SafeNormalize();
				vectorToTarget *= baseVelocity;
			}
			int inertia = projectile.timeLeft > 285 ? 12 : 4;
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTarget) / inertia;
			blurDrawer.Update(projectile.Center);
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 10; i++)
			{
				int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 229);
				Dust dust = Main.dust[dustId];
				dust.noGravity = true;
				dust.velocity *= 3f;
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			Texture2D texture = Main.projectileTexture[projectile.type];
			for(int i = 0; i < blurDrawer.BlurLength; i++)
			{
				if(!blurDrawer.GetBlurPosAndColor(i, Color.White, out Vector2 blurPos, out Color blurColor)) { break; }
				float scale = MathHelper.Lerp(0.75f, 0.25f, i / (float)blurDrawer.BlurLength);
				spriteBatch.Draw(texture, blurPos - Main.screenPosition,
					texture.Bounds, blurColor * 0.5f, r,
					texture.Bounds.Center(), scale, 0, 0);
			}
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				texture.Bounds, Color.White, r,
				texture.Bounds.Center(), 0.75f, 0, 0);
			return false;
		}
	}
	// This is an unfortunate class hierarchy
	class StardustConstellation : TransientMinion
	{
		internal static int TimeToLive = 8 * 60;
		internal static int ConstellationSize = 400;
		internal static int AttackRange = 600;
		internal static int BigStarCount = 16;
		internal static int MaxSmallStars = 16;
		internal static float SmallSpawnChance = 0.125f; // 25% chance to spawn a small star each frame

		internal List<ConstellationStar> bigStars;
		internal List<ConstellationSmallStar> smallStars;
		private SpriteCompositionHelper scHelper;
		private Texture2D smallStarTexture;

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

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

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(player.whoAmI == Main.myPlayer && animationFrame % 10 == 0 && targetNPCIndex is int idx)
			{
				Vector2 launchPos = projectile.Center + bigStars[Main.rand.Next(bigStars.Count)].EndOffset;
				int projectileVelocity = 12;
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= projectileVelocity;
				Vector2 launchVel = vectorToTargetPosition.RotatedBy(Main.rand.NextFloat(MathHelper.Pi / 8) - MathHelper.Pi / 16);

				Projectile.NewProjectile(
					launchPos,
					launchVel,
					ProjectileType<StardustHomingStar>(),
					projectile.damage,
					projectile.knockBack,
					player.whoAmI,
					ai0: idx);
			}
		}

		public override Vector2? FindTarget()
		{
			if (animationFrame < 15) 
			{ 
				return null; 
			}
			if(GetClosestEnemyToPosition(projectile.Center, AttackRange, false) is NPC target)
			{
				targetNPCIndex = target.whoAmI;
				return target.Center - projectile.Center;
			}
			return null;
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
				Vector2 parentPos = bigStars[Main.rand.Next(bigStars.Count)].position;
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
		internal Vector2 EndOffset { get; private set; }
		internal Vector2 position => StartPoint + EndOffset;
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
