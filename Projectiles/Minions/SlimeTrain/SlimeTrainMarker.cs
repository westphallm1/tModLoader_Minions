using AmuletOfManyMinions.Dusts;
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

namespace AmuletOfManyMinions.Projectiles.Minions.SlimeTrain
{
	// A couple methods called independently by both SlimeTrain and SlimeTrainMarker
	// They're deterministic based on SlimeTrain's state on the frame SlimeTrainMarker
	// spawns, so independently computing the values twice is easier than attempting
	// To sync across projectiles/clients
	internal class SlimeTrainRotationTracker
	{
		internal NPC target;
		// 0 or Pi depending on approach direction
		internal float startAngle;
		// 1 for CW, -1 for CCW, depending on approach direction
		internal int rotationDir;

		// radius to circle while summoning a sub projectile
		public static int baseSpawnRadius = 90;

		public int GetCircleRadius()
		{
			if (target == default) { return default; }
			int TargetSize = (int)(target.Size.X + target.Size.Y) / 2;
			return TargetSize + baseSpawnRadius;
		}

		public Vector2 GetStartOffset(Vector2 offset)
		{
			if (target == default) { return default; }
			Vector2 startOffset = offset;
			int approachDir = -Math.Sign(offset.X);
			startOffset += new Vector2(approachDir * GetCircleRadius(), 0);
			return startOffset;
		}

		public void SetRotationInfo(NPC target, Projectile rotator)
		{
			this.target = target;
			int approachDir = -Math.Sign(target.Center.X - rotator.Center.X);
			// use generic values for testing
			startAngle = approachDir == 1 ?  0 : MathHelper.Pi;
			rotationDir = approachDir * Math.Sign(rotator.velocity.Y);
		}

		public Vector2 GetNPCTargetRadius(int frame)
		{
			if (target == default) { return default; }
			float idleAngle = startAngle + rotationDir * MathHelper.TwoPi * frame / SlimeTrainMarkerProjectile.SetupTime;
			Vector2 targetPosition = target.Center;
			int targetRadius = GetCircleRadius();
			targetPosition.X += targetRadius * (float)Math.Cos(idleAngle);
			targetPosition.Y += targetRadius * (float)Math.Sin(idleAngle);
			return targetPosition;
		}

		public Vector2 GetDirection(int baseQuadrant)
		{
			int quadrant = GetQuadrantForReferenceFrame(baseQuadrant);
			float angle = quadrant * MathHelper.PiOver4;
			return -angle.ToRotationVector2();
		}

		public int GetQuadrantForReferenceFrame(int baseQuadrant)
		{
			// values computed by trial and error
			if(rotationDir == 1 && startAngle == 0)
			{
				return baseQuadrant;
			} else if (rotationDir == 1 && startAngle == MathHelper.Pi)
			{
				return (baseQuadrant + 4) % 8;
			} else if(rotationDir == -1 && startAngle == MathHelper.Pi)
			{
				return (12 - baseQuadrant) % 8;
			} else
			{
				return 8 - baseQuadrant;
			}
		}
		public SpriteEffects GetSpriteEffects(int baseQuadrant)
		{
			int quadrant = GetQuadrantForReferenceFrame(baseQuadrant);
			if(quadrant == 3 || quadrant == 4 || quadrant == 7)
			{
				return SpriteEffects.FlipHorizontally;
			} else
			{
				return SpriteEffects.None;
			}
		}
	}

	/// <summary>
	/// Individual SlimeCart projectile summoned by SlimeTrain
	/// SlimeTrain's attack
	/// Uses ai[0] for the NPC to attack, localAi[0] for animation frame
	/// </summary>
	class SlimeTrainCartProjectile: ModProjectile
	{
		public static int TravelVelocity = 6;
		public override string Texture => "AmuletOfManyMinions/Projectiles/Minions/Slimecart/SlimecartMinion";
		// npc to stay on top of
		NPC clingTarget;
		Vector2 startOffset;
		Vector2 travelOffset;
		static int slimeIndex = 0;
		int mySlimeIndex;
		private static Asset<Texture2D> slimeTexture;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 4;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void Load()
		{
			if(!Main.dedServ)
			{
				slimeTexture = ModContent.Request<Texture2D>(Texture + "_Slime");
			}
		}

		public override void Unload()
		{
			slimeTexture = null;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
			Projectile.timeLeft = 30;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override void AI()
		{
			base.AI();
			Projectile.localAI[0]++;
			// failsafe in case we got a bad NPC index
			if (Projectile.ai[0] < 0)
			{
				Projectile.Kill();
				return; 
			}
			// "on spawn" code
			if (clingTarget == null)
			{
				clingTarget = Main.npc[(int)Projectile.ai[0]];
				startOffset = Projectile.Center - clingTarget.Center;
				travelOffset = Projectile.velocity;
				travelOffset.SafeNormalize();
				travelOffset *= TravelVelocity;
				Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.velocity.X > 0 ? 0 : MathHelper.Pi);
				Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
				Projectile.velocity = Vector2.Zero;
				mySlimeIndex = slimeIndex % 7;
				slimeIndex++;
				// SpawnDust();
			}
			if (!clingTarget.active)
			{
				Projectile.Kill();
				return;
			}
			// slide towards enemy on a fixed path
			Projectile.Center = clingTarget.Center + startOffset + travelOffset * Projectile.localAI[0];
		}

		public override bool PreDraw(ref Color lightColor)
		{
			lightColor = Color.White * 0.85f;
			lightColor.A = 128;
			lightColor *= Math.Min(8, Projectile.localAI[0]) / 8;
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;

			// slime
			Texture2D texture = slimeTexture.Value;
			int frameHeight = texture.Height / 7;
			Rectangle bounds = new Rectangle(0, mySlimeIndex * frameHeight, texture.Width, frameHeight);
			Vector2 tangent = travelOffset.X > 0 ? new Vector2(travelOffset.Y, -travelOffset.X) : new Vector2(-travelOffset.Y, travelOffset.X);
			tangent.SafeNormalize();
			tangent *= 14;
			Main.EntitySpriteDraw(texture, pos + tangent - Main.screenPosition,
				bounds, lightColor, r,
				new Vector2(bounds.Width/2, bounds.Height/2), 1, effects, 0);

			// cart
			texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			frameHeight = texture.Height / Main.projFrames[Projectile.type];
			bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
			return false;
		}


		public override void Kill(int timeLeft)
		{
			float goreVel = 0.25f;
			var source = Projectile.GetSource_Death();
			foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
			{
				if(Main.rand.Next(3) > 0)
				{
					continue;
				}
				int goreIdx = Gore.NewGore(source, Projectile.position, default, Main.rand.Next(61, 64));
				Main.gore[goreIdx].velocity *= goreVel;
				Main.gore[goreIdx].velocity += offset;
			}
			base.Kill(timeLeft);
		}
	}

	/// <summary>
	/// not a distinct projectile, but attached to SlimeTrainMarkerProjectile
	/// Used for keeping track of the rail spawn animation 
	/// </summary>
	internal class SlimeTrainRails
	{
		// number of frames to extend the track for
		public static int DrawFrames = 12;

		// size of a tile (not expected to change)
		public static int PlacementInterval = 16;
		// pixels to grow track per frame (not expected to change)
		public static float GrowthRate = 8;

		public int startFrame;

		public int quadrant;

		public readonly Color color;
		
		internal SlimeTrainRails(int quadrant, Color color)
		{
			this.quadrant = quadrant;
			this.color = color;
			float angle = quadrant * MathHelper.PiOver4;
			startFrame = (int)(angle / MathHelper.TwoPi * SlimeTrainMarkerProjectile.SetupTime);
		}


		public void Draw(Texture2D texture, Color lightColor, int frame, SlimeTrainRotationTracker tracker)
		{
			if(frame < startFrame)
			{
				return;
			}
			int startDrawFrame = GetStartFrame(tracker);
			int middleDrawFrame = GetMiddleFrame(tracker);
			int endDrawFrame = GetEndFrame(tracker);
			SpriteEffects effects = tracker.GetSpriteEffects(quadrant);
			Vector2 direction = tracker.GetDirection(quadrant);
			int endFrame = Math.Min(frame - startFrame, DrawFrames);
			lightColor *= Math.Max(endFrame, 1) / (float)DrawFrames;
			float endLength = endFrame * GrowthRate;
			for(int i = 0; i < endLength + PlacementInterval; i+= PlacementInterval)
			{
				Vector2 drawPos = tracker.GetNPCTargetRadius(startFrame) + direction * i;
				// gently undulate
				Vector2 offset = drawPos - (tracker.target?.Center ?? Vector2.Zero);
				offset.SafeNormalize();
				offset *= 8 + 8 * (float)Math.Sin(MathHelper.TwoPi * (frame - startFrame) / 60f);
				int drawFrame = i == 0 ? startDrawFrame : i >= endLength ? endDrawFrame : middleDrawFrame;
				Rectangle bounds = new Rectangle(0, 18 * drawFrame, 16, 16);
				Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
				Main.EntitySpriteDraw(texture, drawPos  + offset - Main.screenPosition,
					bounds, lightColor, 0, origin, 1, effects, 0);
			}
		}

		private int GetEndFrame(SlimeTrainRotationTracker tracker)
		{
			int quadrant = tracker.GetQuadrantForReferenceFrame(this.quadrant);
			if(quadrant % 4 == 0)
			{
				return 3;
			} else if (quadrant < 4)
			{
				return 0;
			} else
			{
				return 2;
			}
		}

		private int GetMiddleFrame(SlimeTrainRotationTracker tracker)
		{
			int quadrant = tracker.GetQuadrantForReferenceFrame(this.quadrant);
			if(quadrant % 4 == 0)
			{
				return 4;
			} else 
			{
				return 1;
			} 
		}

		private int GetStartFrame(SlimeTrainRotationTracker tracker)
		{
			int quadrant = tracker.GetQuadrantForReferenceFrame(this.quadrant);
			if(quadrant % 4 == 0)
			{
				return 5;
			} else if (quadrant < 4)
			{
				return 2;
			} else
			{
				return 0;
			}
		}

		internal void DoneDust(SlimeTrainRotationTracker tracker)
		{
			Vector2 direction = tracker.GetDirection(quadrant);
			float endLength = DrawFrames * GrowthRate;
			for(int i = 0; i < endLength + PlacementInterval; i+= PlacementInterval)
			{
				Vector2 dustPos = tracker.GetNPCTargetRadius(startFrame) + direction * i;
				int idx = Dust.NewDust(dustPos, 16, 16, DustID.Iron, 0, 0);
				Main.dust[idx].velocity *= 0.25f;
				Main.dust[idx].alpha = 128;
				Main.dust[idx].scale *= 0.85f;

			}
		}

		internal void SpawnCart(SlimeTrainRotationTracker tracker, int frame, Projectile baseProj)
		{
			int waitTime = 15;
			int activeFrame = frame - startFrame - DrawFrames - waitTime;
			// ai[1] contains empower count
			int spawnRate = Math.Max(15, 50 - 5 * (int)baseProj.ai[1]);
			if(activeFrame < 0 || activeFrame % spawnRate != 0)
			{
				return;
			}
			Vector2 direction = tracker.GetDirection(quadrant);
			Vector2 spawnPos = tracker.GetNPCTargetRadius(startFrame);
			Vector2 tangent = direction.X > 0 ? new Vector2(direction.Y, -direction.X) : new Vector2(-direction.Y, direction.X);
			Vector2 spawnOffset = 14 * tangent;
			Projectile.NewProjectile(
				baseProj.GetSource_FromThis(),
				spawnPos + spawnOffset,
				direction,
				ModContent.ProjectileType<SlimeTrainCartProjectile>(),
				baseProj.damage,
				baseProj.knockBack,
				baseProj.owner,
				baseProj.ai[0],
				baseProj.ai[1]);
		}
	}
	

	/// <summary>
	/// Invisible, non-damaging projectile that acts as the "brain" of the 
	/// SlimeTrain's attack
	/// Uses ai[0] for the NPC to attack, ai[1] for empower count,
	/// localAi[0] to count up animation frames
	/// </summary>
	class SlimeTrainMarkerProjectile: ModProjectile
	{
		// npc to stay on top of
		NPC clingTarget;
		// 'spawn' animation time
		public static int SetupTime = 90;
		// frames to run attack for after spawning in
		public static int BaseAttackTime = 240;

		// extra empower time for each power up
		int PerEmpowerTime = 30;

		private SlimeTrainRotationTracker rotationTracker;

		private readonly SlimeTrainRails[] spawnFrames = new SlimeTrainRails[] {
			new SlimeTrainRails(0, Color.LightCoral), 
			new SlimeTrainRails(1, Color.LightSalmon), 
			new SlimeTrainRails(3, Color.LemonChiffon), 
			new SlimeTrainRails(4, Color.LightGreen), 
			new SlimeTrainRails(5, Color.LightSkyBlue), 
			new SlimeTrainRails(7, Color.Lavender), 
		};

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 6;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.timeLeft = SetupTime + BaseAttackTime;
			rotationTracker = new SlimeTrainRotationTracker();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override void AI()
		{
			base.AI();
			Projectile.localAI[0]++;
			// failsafe in case we got a bad NPC index
			if (Projectile.ai[0] < 0)
			{
				Projectile.Kill();
				return; 
			}
			// "on spawn" code
			if (clingTarget == null)
			{
				clingTarget = Main.npc[(int)Projectile.ai[0]];
				Projectile.timeLeft += (int)Projectile.ai[1] * PerEmpowerTime;
				rotationTracker.SetRotationInfo(clingTarget, Projectile);
				Projectile.velocity = Vector2.Zero;
			}
			if (!clingTarget.active)
			{
				Projectile.Kill();
				return;
			}
			Projectile.Center = clingTarget.Center;
			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.5f);
			UpdateTracks();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int currentFrame = (int)Projectile.localAI[0];
			lightColor = Color.White * 0.85f;
			lightColor.A = 128;
			for(int i = 0; i < spawnFrames.Length; i++)
			{
				SlimeTrainRails rails = spawnFrames[i];
				rails.Draw(texture, lightColor, currentFrame, rotationTracker);
			}
			return false;
		}

		private void UpdateTracks()
		{
			// for starters, just draw a track at the rotation 
			int currentFrame = (int)Projectile.localAI[0];
			for(int i = 0; i < spawnFrames.Length; i++)
			{
				SlimeTrainRails rails = spawnFrames[i];
				if(Projectile.owner == Main.myPlayer)
				{
					rails.SpawnCart(rotationTracker, currentFrame, Projectile);
				}
				 if (rails.startFrame + SlimeTrainRails.DrawFrames == currentFrame)
				{
					rails.DoneDust(rotationTracker);
				}
			}
		}
	}
}
