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

namespace AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt
{
	/// <summary>
	/// Uses ai[1] for positioning/sprite selection purposes, 
	/// localAI[0] to communicate attack animations with main Ent
	/// ai[0] already used by GroupAwareMinion
	/// </summary>
	public class LandChunkProjectile : GroupAwareMinion
	{
		public override string Texture => "Terraria/Item_0";

		internal override int BuffId => BuffType<TerrarianEntMinionBuff>();

		internal SpriteCompositionHelper scHelper;
		private int attackStyle;
		internal CompositeSpriteBatchDrawer[] drawers;
		internal SpriteCycleDrawer[] drawFuncs;
		internal Vector2 travelDir;
		internal int targetStartFrame;

		// projectile needs to remain at a fixed position relative to the player
		// during the 'wind up' animation
		internal Vector2 attackStartPlayerOffset;
		internal float attackStartRotation;

		internal NPC targetNPC;


		internal int spawnFrames = 30;
		internal int attackDelayFrames = 40;
		internal int framesToLiveAfterAttack = 120;

		int windupFrames = 20;

		private Vector2[] myOldPos = new Vector2[4];

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
			Main.projFrames[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			projectile.localNPCHitCooldown = 6;
			projectile.minionSlots = 0;
			attackThroughWalls = true;
			useBeacon = false;
			attackFrames = 60;
			scHelper = new SpriteCompositionHelper(this)
			{
				idleCycleFrames = 160,
				frameResolution = 1,
				posResolution = 1
			};
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			int treeIdx = Math.Max(0,(int)projectile.ai[1] - 1);
			attackStyle = (int)projectile.ai[1] / 2;
			drawers = LandChunkConfigs.templates[treeIdx % LandChunkConfigs.templates.Length]();
			drawFuncs = new SpriteCycleDrawer[drawers.Length];
			for(int i = 0; i < drawers.Length; i++)
			{
				drawFuncs[i] = drawers[i].Draw;
			}
			if(attackStyle == 2)
			{
				windupFrames = 30;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(travelDir == default || animationFrame - targetStartFrame < windupFrames)
			{
				return false;
			}
			projHitbox.Inflate(96, 96);
			projHitbox.Offset(scHelper.CenterOfRotation.ToPoint());
			return projHitbox.Intersects(targetHitbox);
		}

		// for layering purposes, this needs to be done manually
		// Called from TerrarianEnt.PreDraw
		public void SubPreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// this is a lot of sprite drawing
			// lifted from ExampleMod's ExampleBullet
			int attackFrame = animationFrame - targetStartFrame;
			if(targetStartFrame != default && attackFrame > windupFrames)
			{
				for (int k = 1; k < myOldPos.Length; k++)
				{
					if(myOldPos[k] == default)
					{
						break;
					}
					Color color = projectile.GetAlpha(lightColor) * 0.5f * ((myOldPos.Length - k) / (float)myOldPos.Length);
					scHelper.positionOverride = myOldPos[k];
					scHelper.Process(spriteBatch, color, false, drawFuncs);
				}
			}
			scHelper.positionOverride = null;
			scHelper.Process(spriteBatch, lightColor, false, drawFuncs);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return false;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			float[] angleOffsets = { 0, MathHelper.PiOver4, -MathHelper.PiOver4 };
			int ai1 = (int)projectile.ai[1];
			bool isEven = ai1 % 2 == 0;
			Vector2 center = new Vector2(-16, -64);
			float baseAngle = isEven ? angleOffsets[attackStyle] : MathHelper.Pi - angleOffsets[attackStyle];
			baseAngle += MathHelper.Pi / 16 * (float) Math.Sin(MathHelper.TwoPi * groupAnimationFrame / groupAnimationFrames);
			Vector2 offset = 164 * baseAngle.ToRotationVector2();
			offset.Y *= 0.5f;
			projectile.rotation = MathHelper.Pi/48 * (float) Math.Sin(MathHelper.TwoPi * animationFrame / 120);
			projectile.position = player.Center + center + offset;
			projectile.velocity = Vector2.Zero;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Array.ForEach(drawers, d => d.Update(projectile, animationFrame, spawnFrames));
			return Vector2.Zero;
		}

		public override Vector2? FindTarget()
		{
			// TODO lift some EmpoweredMinion stuff from here
			if(animationFrame < attackDelayFrames)
			{
				return null;
			} else if (travelDir != default)
			{
				return travelDir;
			} else if (IsMyTurn() && SelectedEnemyInRange(1000, player.Center, 1000) is Vector2 target)
			{
				attackStartPlayerOffset = projectile.position - player.Center;
				attackStartRotation = projectile.rotation;
				travelDir = target - projectile.Center;
				travelDir.SafeNormalize();
				travelDir *= 14;
				if(targetNPCIndex is int idx)
				{
					targetNPC = Main.npc[idx];
					travelDir += targetNPC.velocity;
				}
				targetStartFrame = animationFrame;
				return travelDir;
			}
			return null;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// localAI[0] used to communicate attack animation progress with main ent
			int attackFrame = animationFrame - targetStartFrame;
			projectile.localAI[0] = attackFrame > windupFrames ? 0 : Math.Sign(vectorToTargetPosition.X) * attackFrame;
			if(attackStyle == 0)
			{
				BoomerangTargetedMovement(vectorToTargetPosition);
			}
			else if (attackStyle == 1)
			{
				MissileTargetedMovement(vectorToTargetPosition);
			}
			else
			{
				HammerTargetedMovement(vectorToTargetPosition);
			}
		}

		internal void HammerTargetedMovement(Vector2 vectorToTargetPosition)
		{
			int attackFrame = animationFrame - targetStartFrame;
			int offsetDir = -Math.Sign(vectorToTargetPosition.X);
			Vector2 targetBase = player.Center - new Vector2(0, 96);
			int windupRadius = 256;
			int genericSwingRadius = 30;
			if(targetNPC.active)
			{
				travelDir = targetNPC.position - targetBase;
				if(travelDir.LengthSquared() > 128 * 128)
				{
					Vector2 oppositeDir = -travelDir;
					oppositeDir.SafeNormalize();
					travelDir += oppositeDir * 48;
				}
			} else if (attackFrame == 0)
			{
				travelDir = travelDir * genericSwingRadius;
			}
			if (attackFrame > windupFrames)
			{
				float angle0 = -MathHelper.PiOver2 + MathHelper.Pi / 16;
				float travelLength = travelDir.Length();
				int downSwingFrames = (int)Math.Max(10, travelLength / 50);
				if(attackFrame - windupFrames > downSwingFrames)
				{
					// continue travelling along velocity just for funsies
					return;
				}
				// "swing" towards the target
				float destinationAngle = travelDir.ToRotation();
				if (offsetDir == 1 && destinationAngle > projectile.rotation)
				{
					destinationAngle -= MathHelper.TwoPi;
				}
				float swingFraction = (attackFrame - windupFrames) / (float)downSwingFrames;
				float radius = MathHelper.Lerp(windupRadius, travelDir.Length(), swingFraction);
				float angle = MathHelper.Lerp(angle0, destinationAngle, swingFraction);
				projectile.velocity = targetBase + radius * angle.ToRotationVector2() - projectile.position;
				projectile.rotation = angle + MathHelper.PiOver2;
			}
			else
			{
				float windupRatio = 0.5f;
				int swingFrame = (int)(attackFrame - windupFrames * windupRatio);
				float swingFraction = attackFrame / (float) windupFrames;
				float windupFraction = swingFrame < 0 ? 0 : (float)Math.Sin(MathHelper.PiOver2 * swingFrame / ((1-windupRatio) * windupFrames));
				float targetRotation = -offsetDir * (-MathHelper.PiOver2 * windupFraction + MathHelper.Pi/16) + (offsetDir == -1 ? 0 : MathHelper.Pi);
				Vector2 targetOffset = windupRadius * targetRotation.ToRotationVector2();
				targetOffset.X *= 0.5f;
				Vector2 target = targetBase + targetOffset;

				// asymptotically approach the correct location
				projectile.rotation = MathHelper.Lerp(projectile.rotation, targetRotation + MathHelper.PiOver2, swingFraction);
				projectile.position.X = MathHelper.Lerp(projectile.position.X, target.X, swingFraction);
				projectile.position.Y = MathHelper.Lerp(projectile.position.Y, target.Y, swingFraction);
			}
		}

		internal void MissileTargetedMovement(Vector2 vectorToTargetPosition)
		{
			int attackFrame = animationFrame - targetStartFrame;
			int offsetDir = -Math.Sign(vectorToTargetPosition.X);
			if (attackFrame > windupFrames)
			{
				// simply spin towards the target
				projectile.rotation = vectorToTargetPosition.ToRotation() -MathHelper.PiOver2;
				projectile.velocity = vectorToTargetPosition * 1.5f * attackFrame / (float) windupFrames;
			}
			else
			{
				int windupRadius = 64;
				float targetRotation = vectorToTargetPosition.ToRotation() - MathHelper.PiOver2;
				if(vectorToTargetPosition.X < 0 && vectorToTargetPosition.Y < 0)
				{
					targetRotation += MathHelper.TwoPi;
				}
				float rotation = MathHelper.Lerp(attackStartRotation, targetRotation, attackFrame / (float)windupFrames);
				// swing backwards a little bit before swinging forwards a little bit
				Vector2 centerOfRotation = attackStartPlayerOffset + offsetDir * windupRadius * attackStartRotation.ToRotationVector2();
				projectile.rotation = rotation;
				projectile.position = player.Center + centerOfRotation + -offsetDir * windupRadius * projectile.rotation.ToRotationVector2();
			}
		}

		internal void BoomerangTargetedMovement(Vector2 vectorToTargetPosition)
		{
			int attackFrame = animationFrame - targetStartFrame;
			int offsetDir = -Math.Sign(vectorToTargetPosition.X);
			if(attackFrame > windupFrames)
			{
				// spinning from the center looks nicer
				scHelper.CenterOfRotation = new Vector2(0, -48);
				// simply spin towards the target
				projectile.rotation += -offsetDir * MathHelper.Pi / 8;
				projectile.velocity = vectorToTargetPosition * attackFrame / (float) windupFrames;
			} else
			{
				int windupRadius = 64;
				float rotationScale = MathHelper.Lerp(MathHelper.Pi / 128, MathHelper.Pi / 32, attackFrame / (float)windupFrames);
				// swing backwards a little bit before swinging forwards a little bit
				Vector2 centerOfRotation = attackStartPlayerOffset + offsetDir * windupRadius * attackStartRotation.ToRotationVector2();
				projectile.rotation += offsetDir * rotationScale;
				projectile.position = player.Center + centerOfRotation + -offsetDir * windupRadius * projectile.rotation.ToRotationVector2();
			}
		}

		public override void AfterMoving()
		{
			// left shift old position
			int attackFrame = animationFrame - targetStartFrame;
			if(targetStartFrame != default && attackFrame > windupFrames)
			{
				for(int i = myOldPos.Length -1; i > 0; i--)
				{
					myOldPos[i] = myOldPos[i - 1];
				}
				myOldPos[0] = projectile.position;
			} 
			if(targetStartFrame != default && (animationFrame - targetStartFrame > framesToLiveAfterAttack 
				|| Vector2.DistanceSquared(player.Center, projectile.Center) > 1300f * 1300f))
			{
				projectile.Kill();
			}
		}
	}

}
