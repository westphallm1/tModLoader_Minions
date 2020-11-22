using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.WhackAMole
{
	public class WhackAMoleMinionBuff : MinionBuff
	{
		public WhackAMoleMinionBuff() : base(ProjectileType<WhackAMoleCounterMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Star-eyed Mole");
			Description.SetDefault("A magic mole will fight for you!");
		}
	}

	public class WhackAMoleMinionItem : MinionItem<WhackAMoleMinionBuff, WhackAMoleCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Magic Mole Hammer");
			Tooltip.SetDefault("Summons a magic whack-a-mole to fight for you!\nDon't bonk it too hard...");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.damage = 34;
			item.value = Item.buyPrice(0, 15, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}
	}

	public class WhackAMoleMinionProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.friendly = true;
			projectile.penetrate = 1;
			projectile.tileCollide = true;
			projectile.timeLeft = 60;
		}

		public override void AI()
		{
			projectile.rotation += 0.25f;
			if(projectile.timeLeft < 30 && projectile.velocity.Y < 16)
			{
				projectile.velocity.Y += 0.5f;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture);
			
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
				texture.Bounds, WhackAMoleMinion.shades[(int)projectile.ai[0]], projectile.rotation,
				texture.Bounds.Center.ToVector2(), 1, 0, 0);
			return false;
		}
	}

	public class WhackAMoleCounterMinion : CounterMinion<WhackAMoleMinionBuff> {
		protected override int MinionType => ProjectileType<WhackAMoleMinion>();
	}
	public class WhackAMoleMinion : EmpoweredMinion<WhackAMoleMinionBuff>
	{
		protected override int CounterType => ProjectileType<WhackAMoleCounterMinion>();

		protected override int dustType => DustID.Dirt;

		protected int idleGroundDistance = 300;
		protected int idleStopChasingDistance = 800;
		protected bool isFlying = false;
		protected Vector2? theGround;
		protected int animationFrame;
		protected int AnimationFrames = 60;
		protected int TeleportFrames = 60;
		protected int? teleportStartFrame = null;
		protected Vector2 teleportDestination;
		private bool didHitWall = false;
		private bool isOnGround = false;
		private int offTheGroundFrames = 0;
		private int projectileIndex;
		private static Vector2[] positionOffsets =
		{
			Vector2.Zero,
			new Vector2(-16, 0),
			new Vector2(-8, -14),
			new Vector2(16, 0),
			new Vector2(8, -14),
			new Vector2(0, -28)
		};

		// x offset of every mole
		private static int[] xOffsets = { 0, 8, 8, 0, 0, 0 };

		private static int[] widths = { 24, 32, 32, 48, 48, 48 };
		private static Rectangle[] platformBounds =
		{
			new Rectangle(0, 0, 28, 22),
			new Rectangle(0, 24, 44, 22),
			new Rectangle(0, 24, 44, 22),
			new Rectangle(0, 48, 52, 24),
			new Rectangle(0, 48, 52, 24),
			new Rectangle(0, 48, 52, 24),
		};

		// color of every mole
		public static Color[] shades =
		{
			new Color(101, 196, 255),
			new Color(153, 221, 146),
			new Color(255, 101, 132),
			new Color(233, 229, 146),
			new Color(173, 101, 255),
			new Color(255, 101, 244),
		};
		
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Goblin Gunner");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}


		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			projectile.tileCollide = false;
			projectile.friendly = true;
			attackThroughWalls = false;
			useBeacon = false;
			frameSpeed = 5;
			animationFrame = 0;
			projectile.hide = true;
			projectileIndex = 0;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if(oldVelocity.X != 0 && projectile.velocity.X == 0)
			{
				didHitWall = true;
			}
			return false;
		}

		// offset of each individual mole

		private int DrawIndex => Math.Max(0,Math.Min(shades.Length, EmpowerCount)-1);
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture);
			// draw in reverse order for layering purposes
			float headBobOffset = (float)(2 * Math.PI / Math.Max(DrawIndex, 1));
			for(int i = DrawIndex; i >= 0; i--)
			{
				int offsetPixels;
				Vector2 pos = projectile.Center;
				if (teleportStartFrame is int teleportStart) 
				{
					int teleportFrame = animationFrame - teleportStart;
					int teleportHalf = TeleportFrames / 2;
					float heightToSink = 24 - positionOffsets[i].Y;
					if(teleportFrame < teleportHalf)
					{
						offsetPixels = -(int)(heightToSink * (float)teleportFrame / teleportHalf);
					}
					else
					{
						offsetPixels = -(int)(heightToSink * (float)(TeleportFrames - teleportFrame) / teleportHalf);
					}
					pos.X += positionOffsets[i].X;
					pos.Y += positionOffsets[i].Y /2;
				} else 
				{
					offsetPixels = (int) (3 * Math.Sin(headBobOffset * i + 2 * Math.PI * (animationFrame % AnimationFrames) / AnimationFrames));
					pos += positionOffsets[i];
				}
				Rectangle bounds = new Rectangle(0, 0, 24, 24 + offsetPixels);
				pos.Y += 4 - (offsetPixels/2);
				pos.X += xOffsets[DrawIndex];
				SpriteEffects effects = projectile.velocity.X < 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				spriteBatch.Draw(texture, pos - Main.screenPosition,
					bounds, shades[i], 0,
					bounds.Center.ToVector2(), 1, effects, 0);
			}
			DrawPlatform(spriteBatch, lightColor);
			return false;
		}

		private void DrawPlatform(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D platform = GetTexture(Texture + "_Ground");
			Rectangle bounds = platformBounds[DrawIndex];
			Vector2 pos = projectile.Bottom + new Vector2(0, bounds.Height / 2);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(platform, pos - Main.screenPosition,
				bounds, lightColor, 0, origin, 1, 0, 0);

		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = isFlying? player.Top : player.Bottom;
			idlePosition.X += 48 * -player.direction;
			Vector2 idleHitLine = player.Center;
			idleHitLine.X += 48 * -player.direction;
			if (!Collision.CanHitLine(idleHitLine, 1, 1, player.Center, 1, 1))
			{
				idlePosition = player.Bottom;
			}
			animationFrame++;
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			isOnGround = InTheGround(new Vector2(projectile.Bottom.X, projectile.Bottom.Y + 8)) ||
				InTheGround(new Vector2(projectile.Bottom.X, projectile.Bottom.Y + 24));
			if(offTheGroundFrames == 20 || (offTheGroundFrames > 60 && Main.rand.Next(120) == 0) ||(isOnGround && offTheGroundFrames > 20))
			{
				DrawPlatformDust();
			}
			Lighting.AddLight(projectile.Center, Color.PaleGreen.ToVector3() * 0.75f);
			return vectorToIdlePosition;
		}

		private void DrawPlatformDust()
		{
			for(int i = 0; i < 3; i++)
			{
				Dust.NewDust(projectile.Bottom - new Vector2(8, 0), 16, 16, 47, -projectile.velocity.X/2, -projectile.velocity.Y/2);
			}
		}

		// determine whether the minion should be flying or not
		private void SetFlyingState()
		{
			theGround = NearestGroundLocation();
			if(vectorToIdle.Length() > ComputeSearchDistance() || 
				(vectorToTarget is null && (vectorToIdle.Y > idleGroundDistance || theGround is null)))
			{
				isFlying = true;
			} else if (!InTheGround(projectile.Bottom) && Math.Abs(vectorToIdle.Y) < idleGroundDistance/2 && theGround != null)
			{
				isFlying = false;
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			SetFlyingState();
			if(teleportStartFrame != null)
			{
				DoTeleport();
			} else if(isFlying)
			{
				IdleFlyingMovement(vectorToIdlePosition);
			} else
			{
				IdleGroundedMovement(vectorToIdlePosition);
			}
		}

		public void IdleFlyingMovement(Vector2 vectorToIdlePosition)
		{
			projectile.tileCollide = false;
			base.IdleMovement(vectorToIdlePosition);
		}

		// if we've run up against a block or a ledge, find the nearest clear space in the direction
		// of the target
		private bool TeleportTowardsTarget(Vector2 vectorToTarget, int increment = 16)
		{
			Vector2 incrementVector = vectorToTarget;
			incrementVector.Normalize();
			float maxLength = vectorToTarget.Length();
			Vector2? clearSpace = null;
			bool hasFoundGround = false;
			for(int i = increment; i < maxLength; i+= increment)
			{
				Vector2 position = projectile.Center + incrementVector * i;
				bool inTheGround = InTheGround(position);
				hasFoundGround |= inTheGround;
				if(hasFoundGround && !inTheGround)
				{
					clearSpace = position;
					break;
				}
			}
			if(clearSpace == null && !InTheGround(projectile.Center + vectorToTarget)) {
				clearSpace = projectile.Center + vectorToTarget;
			}
			if(clearSpace is Vector2 clear && NearestGroundLocation(searchStart: clear) is Vector2 clearGround)
			{
				clearGround.Y -= clearGround.Y % 16; // snap to the nearest block
				clearGround.Y -= projectile.height;
				teleportDestination = clearGround;
				teleportDestination.X += Math.Sign(vectorToTarget.X) * projectile.width / 2;
				teleportStartFrame = animationFrame;
				return true;
			}
			return false;
		}

		// See if there's a solid block with *top* between us and the target, and teleport on
		// top of it if so
		private bool TeleportTowardsCeiling(Vector2 vectorToTarget, int increment = 16)
		{
			vectorToTarget.Y -= 16; // give a bit of leeway
			vectorToTarget.X = 0; // only go straight up
			Vector2 incrementVector = vectorToTarget;
			incrementVector.Normalize();
			float maxLength = vectorToTarget.Length();
			Vector2? clearSpace = null;
			bool hasFoundGround = false;
			for(int i = 16; i < maxLength; i+= increment)
			{
				Vector2 position = projectile.Center + incrementVector * i;
				bool inTheGround = InTheGround(position);
				hasFoundGround |= inTheGround;
				if(hasFoundGround && !inTheGround)
				{
					clearSpace = position;
					break;
				}
			}
			if(clearSpace is Vector2 clear && NearestGroundLocation(searchStart: clear) is Vector2 clearGround)
			{
				clearGround.Y -= clearGround.Y % 16; // snap to the nearest block
				clearGround.Y -= (projectile.height);
				teleportDestination = clearGround;
				teleportStartFrame = animationFrame;
				return true;
			}
			return false;
		}

		private void DoTeleport()
		{
			projectile.velocity = Vector2.Zero;
			int teleportFrame = animationFrame - (int)teleportStartFrame;
			int width = widths[DrawIndex];
			if (teleportFrame == 1 || teleportFrame == 1+ TeleportFrames/2)
			{
				Collision.HitTiles(projectile.Bottom + new Vector2(-width / 2, 8), new Vector2(0, 8), width, 8);
			}
			if(teleportFrame == TeleportFrames / 2)
			{
				// do the actual teleport
				projectile.position = teleportDestination;
			} else if(teleportFrame >= TeleportFrames)
			{
				teleportStartFrame = null;
			}
		}

		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
		{
			drawCacheProjsBehindNPCsAndTiles.Add(index);
		}

		public void IdleGroundedMovement(Vector2 vectorToIdlePosition)
		{
			bool needsToGoThroughWall = didHitWall && vectorToIdlePosition.Length() > 32;
			bool needsToGoThroughFloor = isOnGround && vectorToIdlePosition.Y > 32 &&
				Math.Abs(vectorToIdlePosition.X) < 32;
			bool needsToGoThroughCeiling = isOnGround && vectorToIdlePosition.Y < -64 &&
				Math.Abs(vectorToIdlePosition.X) < 32 && !Collision.CanHit(projectile.Center, 1, 1, projectile.Center + vectorToIdlePosition, 1, 1);
			bool needsToGoOverLedge = isOnGround && NearestCliffLocation(vectorToIdlePosition, maxSearchDistance: 32, cliffHeight: 128) != null;
			bool ableToNavigate = false;
			if(needsToGoThroughFloor || needsToGoThroughWall || needsToGoThroughCeiling || needsToGoOverLedge)
			{
				if(needsToGoThroughWall && NearestLedgeLocation(vectorToIdlePosition,maxSearchDistance: 128) != null)
				{
					projectile.velocity.Y = -4;
					isOnGround = false;
					offTheGroundFrames = 20;
					ableToNavigate = true;
				} else if(needsToGoThroughWall || needsToGoThroughFloor || needsToGoOverLedge)
				{
					ableToNavigate = TeleportTowardsTarget(vectorToIdlePosition);
				} else if(needsToGoThroughCeiling)
				{
					ableToNavigate = TeleportTowardsCeiling(vectorToIdlePosition);
				}
				didHitWall = false;
				isFlying = !ableToNavigate;
				Main.NewText("F: " + needsToGoThroughFloor + 
					" W: " + needsToGoThroughWall + 
					" C: " + needsToGoThroughCeiling + 
					" L: " + needsToGoOverLedge + 
					" S: " + ableToNavigate);
				return;
			}
			projectile.tileCollide = true;
			if (projectile.velocity.Y > 16)
			{
				projectile.velocity.Y = 16;
			}
			projectile.velocity.Y += 0.5f ;
			float intendedY = projectile.velocity.Y;
			base.IdleMovement(vectorToIdlePosition);
			projectile.velocity.Y = intendedY;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = false;
			return true;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int attackRate = Math.Max(10, 60 / (DrawIndex+1));
			bool isAttackFrame = player.whoAmI == Main.myPlayer && animationFrame % attackRate == 0 && teleportStartFrame == null;
			bool canHitTarget = isAttackFrame && Collision.CanHit(projectile.Center, 1, 1, projectile.Center + vectorToTargetPosition, 1, 1);
			bool isAbove = isAttackFrame && Math.Abs(vectorToTargetPosition.X) < 96 && vectorToTargetPosition.Y < -24;
			bool isAttackingFromAir = isAttackFrame && isFlying;
			if(isAttackFrame && canHitTarget && (isAbove || isAttackingFromAir))
			{
				Vector2 velocity = vectorToTargetPosition;
				velocity += Main.npc[(int)targetNPCIndex].velocity;
				velocity.SafeNormalize();
				velocity *= 12;
				Projectile.NewProjectile(
					projectile.Center, 
					velocity, 
					ProjectileType<WhackAMoleMinionProjectile>(), 
					baseDamage, 
					projectile.knockBack, 
					player.whoAmI, 
					projectileIndex);
				projectileIndex = (projectileIndex + 1) % (DrawIndex + 1);	
			}
			IdleMovement(isFlying? vectorToIdle : vectorToTargetPosition);
		}

		protected override int ComputeDamage()
		{
			return baseDamage + (baseDamage / 3) * EmpowerCount;
		}

		private Vector2? GetTargetVector()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center, 0.75f * searchDistance, player.Center) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (ClosestEnemyInRange(searchDistance, player.Center, 0.75f * searchDistance, losCenter: player.Center) is Vector2 target2)
			{
				return target2 - projectile.Center;
			}
			else
			{
				return null;
			}
		}
		public override Vector2? FindTarget()
		{
			return GetTargetVector();
		}

		protected override float ComputeSearchDistance()
		{
			return 700 + 25 * EmpowerCount;
		}

		protected override float ComputeInertia()
		{
			return 5;
		}

		protected override float ComputeTargetedSpeed()
		{
			// ComputeTargetedSpeed is never called 
			// since the same AI is used for targetted and non-targetted movement
			return ComputeIdleSpeed();
		}

		protected override float ComputeIdleSpeed()
		{
			return isFlying ? 12 : 6 + (vectorToTarget == null ? 0 : Math.Min(2, 0.5f * EmpowerCount));
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 0;
		}

		public override void AfterMoving()
		{
			if(isOnGround)
			{
				offTheGroundFrames = 0;
			} else
			{
				offTheGroundFrames++;
			}
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if (Math.Abs(projectile.velocity.X) > 4)
			{
				projectile.spriteDirection = projectile.velocity.X > 0 ? -1 : 1;
			}
		}
	}
}
