using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Twinkle;
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 56;
			Projectile.height = 56;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.timeLeft = 60;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Vector2 targetPos = new Vector2(Projectile.ai[0], Projectile.ai[1]);
			if(Vector2.DistanceSquared(Projectile.Center, targetPos) < 32 * 32)
			{
				Projectile.Kill();
			}
			for (int i = 0; i < 5; i++)
			{
				Vector2 velocity = Vector2.Zero;
				Dust.NewDust(Projectile.Center, 1, 1, DustType<MovingWaypointDust>(), velocity.X, velocity.Y, newColor: Color.DeepSkyBlue, Scale: 0.9f);
			}
		}

		public override void Kill(int timeLeft)
		{
			Vector2 targetPos = new Vector2(Projectile.ai[0], Projectile.ai[1]);
			if(Projectile.owner == Main.myPlayer)
			{
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					targetPos,
					Vector2.Zero,
					ProjectileType<StardustConstellation>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner);
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
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.timeLeft = 300;
			blurDrawer = new MotionBlurDrawer(10);
		}

		public override void AI()
		{
			base.AI();
			if(Projectile.ai[0] == 0)
			{
				return; // failsafe in case we got a bad NPC index
			}
			if(target == null)
			{
				target = Main.npc[(int)Projectile.ai[0]];
				baseVelocity = Projectile.velocity.Length();
			}
			if(!target.active)
			{
				Projectile.Kill();
				return;
			}
			Vector2 vectorToTarget = target.Center - Projectile.Center;
			float distanceToTarget = vectorToTarget.Length();
			if(distanceToTarget > baseVelocity)
			{
				vectorToTarget.SafeNormalize();
				vectorToTarget *= baseVelocity;
			}
			int inertia = Projectile.timeLeft > 285 ? 12 : 4;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTarget) / inertia;
			blurDrawer.Update(Projectile.Center);
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 10; i++)
			{
				int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 229);
				Dust dust = Main.dust[dustId];
				dust.noGravity = true;
				dust.velocity *= 3f;
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			for(int i = 0; i < blurDrawer.BlurLength; i++)
			{
				if(!blurDrawer.GetBlurPosAndColor(i, Color.White, out Vector2 blurPos, out Color blurColor)) { break; }
				float scale = MathHelper.Lerp(0.75f, 0.25f, i / (float)blurDrawer.BlurLength);
				Main.EntitySpriteDraw(texture, blurPos - Main.screenPosition,
					texture.Bounds, blurColor * 0.5f, r,
					texture.Bounds.Center(), scale, 0, 0);
			}
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
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

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

		public override void LoadAssets()
		{
			AddTexture(Texture + "_Small");
			AddTexture("Terraria/Images/Misc/StarDustSky/Star 0");
			AddTexture("Terraria/Images/Misc/StarDustSky/Star 1");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = TimeToLive;
		}
		public override void OnSpawn()
		{
			base.OnSpawn();
			scHelper = new SpriteCompositionHelper(this, new Rectangle(0, 0, ConstellationSize, ConstellationSize))
			{
				idleCycleFrames = Projectile.timeLeft,
				frameResolution = 1,
				posResolution = 1
			};
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
				bigStars.Add(new ConstellationStar(ExtraTextures[Main.rand.Next(1, 2)], startPoint, i));
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
				bigStars[i].DrawConnections(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value, helper.spriteBatch, AnimationFrame);
			}
			for(int i = 0; i < bigStars.Count; i++)
			{
				bigStars[i].Draw(helper.spriteBatch, AnimationFrame);
			}
		}

		private void DrawSmallStars(SpriteCompositionHelper helper, int frame, float cycleAngle)
		{
			// could probably be accomplished with dust
			for(int i = 0; i < smallStars.Count; i++)
			{
				smallStars[i].Draw(helper.spriteBatch, ExtraTextures[0].Value, AnimationFrame);
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// no-op
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(Player.whoAmI == Main.myPlayer && AnimationFrame % 10 == 0 && TargetNPCIndex is int idx)
			{
				Vector2 launchPos = Projectile.Center + bigStars[Main.rand.Next(bigStars.Count)].EndOffset;
				int projectileVelocity = 12;
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= projectileVelocity;
				Vector2 launchVel = vectorToTargetPosition.RotatedBy(Main.rand.NextFloat(MathHelper.Pi / 8) - MathHelper.Pi / 16);

				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					launchPos,
					launchVel,
					ProjectileType<StardustHomingStar>(),
					Projectile.damage,
					Projectile.knockBack,
					Player.whoAmI,
					ai0: idx);
			}
		}

		public override Vector2? FindTarget()
		{
			if (AnimationFrame < 15) 
			{ 
				return null; 
			}
			if(GetClosestEnemyToPosition(Projectile.Center, AttackRange, false) is NPC target)
			{
				TargetNPCIndex = target.whoAmI;
				return target.Center - Projectile.Center;
			}
			return null;
		}

		private void UpdateSmallStars()
		{
			if (AnimationFrame < 15) 
			{ 
				return; 
			}
			int lastDeadIdx = -1;
			for(int i = 0; i < smallStars.Count; i++)
			{
				if(smallStars[i].IsDead(AnimationFrame))
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
				smallStars.Add(new ConstellationSmallStar(AnimationFrame, parentPos));
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float fadeOutFrames = 30;
			float brightness = Projectile.timeLeft > fadeOutFrames ? 1 : Projectile.timeLeft / fadeOutFrames;
			scHelper.Draw(Color.White * brightness);
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
			Main.EntitySpriteDraw(
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
		Asset<Texture2D> texture;
		int idx;
		int maxConnections;
		Vector2[] connections;
		
		public ConstellationStar(Asset<Texture2D> texture, Vector2 startPoint, int idx)
		{
			this.idx = idx;
			this.texture = texture;
			StartPoint = startPoint;
			int tier = idx / TierSize;
			int radius = tier * TierRadius + Main.rand.Next(TierRadius);
			EndOffset = Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * radius;
			maxConnections = Main.rand.NextBool(3)? 2 : 1;
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
				Main.EntitySpriteDraw(
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
			Main.EntitySpriteDraw(
				texture.Value, pos, texture.Value.Bounds, Color.White * brightness, r, 
				texture.Value.Bounds.Center.ToVector2(), scale, 0, 0);
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
