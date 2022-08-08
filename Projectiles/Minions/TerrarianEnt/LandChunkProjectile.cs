using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions.Effects;
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
		public override string Texture => "Terraria/Images/Item_0";

		public override int BuffId => BuffType<TerrarianEntMinionBuff>();

		internal SpriteCompositionHelper scHelper;
		private int attackStyle;
		private bool isEven;
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

		internal bool hasSpawnedSwarm = false;
		private MotionBlurDrawer blurHelper;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = false;
			Main.projFrames[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.localNPCHitCooldown = 18;
			Projectile.minionSlots = 0;
			Projectile.minion = false;
			AttackThroughWalls = true;
			UseBeacon = false;
			attackFrames = 60;
			blurHelper = new MotionBlurDrawer(4);
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			scHelper = new SpriteCompositionHelper(this, new Rectangle(0, 0, 120, 400))
			{
				idleCycleFrames = 160,
				frameResolution = 1,
				posResolution = 1,
				BaseOffset = new Vector2(0, 100)
			};

			int treeIdx = Math.Max(0,(int)Projectile.ai[1] - 1);
			attackStyle = (int)Projectile.ai[1] / 2;
			isEven = Projectile.ai[1] % 2 == 0;
			drawers = LandChunkConfigs.templates[treeIdx % LandChunkConfigs.templates.Length]();
			drawFuncs = new SpriteCycleDrawer[drawers.Length];
			scHelper.Attach();
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
			if(travelDir == default || AnimationFrame - targetStartFrame < windupFrames)
			{
				return false;
			}
			projHitbox.Inflate(96, 96);
			projHitbox.Offset(scHelper.CenterOfRotation.ToPoint());
			return projHitbox.Intersects(targetHitbox);
		}

		// for layering purposes, this needs to be done manually
		// Called from TerrarianEnt.PreDraw
		public void SubPreDraw(Color lightColor)
		{
			// this is a lot of sprite drawing
			// lifted from ExampleMod's ExampleBullet
			int attackFrame = AnimationFrame - targetStartFrame;
			if(targetStartFrame != default && attackFrame > windupFrames)
			{
				for (int k = 0; k < blurHelper.BlurLength; k++)
				{
					if(!blurHelper.GetBlurPosAndColor(k, lightColor, out Vector2 blurPos, out Color blurColor))
					{
						break;
					}
					scHelper.positionOverride = blurPos;
					scHelper.Draw(blurColor * 0.25f);
				}
			}
			scHelper.positionOverride = null;
			scHelper.Draw(lightColor);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			float[] angleOffsets = { 0, MathHelper.PiOver4, -MathHelper.PiOver4 };
			Vector2 center = new Vector2(-16, -64);
			float baseAngle = isEven ? angleOffsets[attackStyle] : MathHelper.Pi - angleOffsets[attackStyle];
			baseAngle += MathHelper.Pi / 16 * (float) Math.Sin(MathHelper.TwoPi * GroupAnimationFrame / GroupAnimationFrames);
			Vector2 offset = 164 * baseAngle.ToRotationVector2();
			offset.Y *= 0.5f;
			Projectile.rotation = MathHelper.Pi/48 * (float) Math.Sin(MathHelper.TwoPi * AnimationFrame / 120);
			Projectile.position = Player.Center + center + offset;
			Projectile.velocity = Vector2.Zero;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Array.ForEach(drawers, d => d.Update(Projectile, AnimationFrame, spawnFrames));
			return Vector2.Zero;
		}

		public override Vector2? FindTarget()
		{
			// TODO lift some EmpoweredMinion stuff from here
			int attackFrame = AnimationFrame - targetStartFrame;
			if(AnimationFrame < attackDelayFrames)
			{
				return null;
			} else if (attackFrame == windupFrames && targetNPC != null && targetNPC.active)
			{
				// adjust the target direction right before flinging,
				// increases accuracy a bit
				travelDir = targetNPC.Center - Projectile.Center;
				travelDir.SafeNormalize();
				travelDir *= 14;
				travelDir += targetNPC.velocity;
				return travelDir;
			} else if (travelDir != default)
			{
				return travelDir;
			} else if (IsMyTurn() && SelectedEnemyInRange(1400, 1400) is Vector2 target)
			{
				if(TargetNPCIndex == null)
				{
					return null; // need an actual npc target for this to work
				}
				targetNPC = Main.npc[(int)TargetNPCIndex];
				attackStartPlayerOffset = Projectile.position - Player.Center;
				attackStartRotation = Projectile.rotation;
				travelDir = target - Projectile.Center;
				travelDir.SafeNormalize();
				travelDir *= 14;
				targetStartFrame = AnimationFrame;
				return travelDir;
			}
			return null;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// localAI[0] used to communicate attack animation progress with main ent
			int attackFrame = AnimationFrame - targetStartFrame;
			if (attackFrame > windupFrames)
			{
				Projectile.ai[1] = -1; // no longer 'attached' the the main npc, can spawn a new one
				Projectile.localAI[0] = 0;
			} else
			{
				Projectile.localAI[0] = Math.Sign(vectorToTargetPosition.X) * attackFrame;
			}
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
			int attackFrame = AnimationFrame - targetStartFrame;
			int offsetDir = -Math.Sign(vectorToTargetPosition.X);
			Vector2 targetBase = Player.Center - new Vector2(0, 96);
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
				if (offsetDir == 1 && destinationAngle > Projectile.rotation)
				{
					destinationAngle -= MathHelper.TwoPi;
				}
				float swingFraction = (attackFrame - windupFrames) / (float)downSwingFrames;
				float radius = MathHelper.Lerp(windupRadius, travelDir.Length(), swingFraction);
				float angle = MathHelper.Lerp(angle0, destinationAngle, swingFraction);
				Projectile.velocity = targetBase + radius * angle.ToRotationVector2() - Projectile.position;
				Projectile.rotation = angle + MathHelper.PiOver2;
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
				Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation + MathHelper.PiOver2, swingFraction);
				Projectile.position.X = MathHelper.Lerp(Projectile.position.X, target.X, swingFraction);
				Projectile.position.Y = MathHelper.Lerp(Projectile.position.Y, target.Y, swingFraction);
			}
		}

		internal void MissileTargetedMovement(Vector2 vectorToTargetPosition)
		{
			int attackFrame = AnimationFrame - targetStartFrame;
			int offsetDir = -Math.Sign(vectorToTargetPosition.X);
			if (attackFrame > windupFrames)
			{
				// simply spin towards the target
				Projectile.rotation = vectorToTargetPosition.ToRotation() -MathHelper.PiOver2;
				Projectile.velocity = vectorToTargetPosition * 1.5f * attackFrame / (float) windupFrames;
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
				Projectile.rotation = rotation;
				Projectile.position = Player.Center + centerOfRotation + -offsetDir * windupRadius * Projectile.rotation.ToRotationVector2();
			}
		}

		internal void BoomerangTargetedMovement(Vector2 vectorToTargetPosition)
		{
			int attackFrame = AnimationFrame - targetStartFrame;
			int offsetDir = -Math.Sign(vectorToTargetPosition.X);
			if(attackFrame > windupFrames)
			{
				// spinning from the center looks nicer
				scHelper.CenterOfRotation = new Vector2(0, -48);
				// simply spin towards the target
				Projectile.rotation += -offsetDir * MathHelper.Pi / 8;
				Projectile.velocity = vectorToTargetPosition * attackFrame / (float) windupFrames;
			} else
			{
				int windupRadius = 64;
				float rotationScale = MathHelper.Lerp(MathHelper.Pi / 128, MathHelper.Pi / 32, attackFrame / (float)windupFrames);
				// swing backwards a little bit before swinging forwards a little bit
				Vector2 centerOfRotation = attackStartPlayerOffset + offsetDir * windupRadius * attackStartRotation.ToRotationVector2();
				Projectile.rotation += offsetDir * rotationScale;
				Projectile.position = Player.Center + centerOfRotation + -offsetDir * windupRadius * Projectile.rotation.ToRotationVector2();
			}
		}

		public override void AfterMoving()
		{
			// left shift old position
			int attackFrame = AnimationFrame - targetStartFrame;
			blurHelper.Update(Projectile.Center, targetStartFrame != default && attackFrame > windupFrames);
			if(targetStartFrame != default && (AnimationFrame - targetStartFrame > framesToLiveAfterAttack 
				|| Vector2.DistanceSquared(Player.Center, Projectile.Center) > 1300f * 1300f))
			{
				Projectile.Kill();
			}
			scHelper.UpdateDrawers(false, drawFuncs);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			int maxSwarmSize = 4;
			int projType = ProjectileType<CritterSwarmProjectile>();
			if(!hasSpawnedSwarm && Main.projectile.Where(p =>
				p.active && p.owner == Player.whoAmI &&
				p.type == projType && (int)p.ai[0] == target.whoAmI).Count() < maxSwarmSize)
			{
				hasSpawnedSwarm = true;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					target.Center,
					Vector2.Zero,
					projType,
					(int)(Projectile.damage * 0.33f),
					0,
					Main.myPlayer,
					ai0: target.whoAmI,
					// a bit silly to work back the original ai[1] here
					ai1: 2 * attackStyle + (isEven ? 0 : 1));
			}
		}
	}

}
