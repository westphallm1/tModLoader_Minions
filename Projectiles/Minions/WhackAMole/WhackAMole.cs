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

	public class WhackAMoleCounterMinion : CounterMinion<WhackAMoleMinionBuff> {
		protected override int MinionType => ProjectileType<WhackAMoleMinion>();
	}
	public class WhackAMoleMinion : EmpoweredMinion<WhackAMoleMinionBuff>
	{
		protected override int CounterType => ProjectileType<WhackAMoleCounterMinion>();

		protected override int dustType => DustID.Dirt;

		protected int idleGroundDistance = 300;
		protected bool isFlying = false;
		protected Vector2? theGround;
		protected int animationFrame;
		protected int AnimationFrames = 60;
		protected int TeleportFrames = 60;
		protected int? teleportStartFrame = null;
		protected Vector2 teleportDestination;
		private bool didHitWall = false;
		private bool isOnGround = false;
		
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
		private static int[] xOffsets =
		{
			0,
			8,
			8,
			0,
			0,
			0
		};

		private static int[] widths =
		{
			24,
			32,
			32,
			48,
			48,
			48
		};

		// color of every mole
		private static Color[] shades =
		{
			new Color(101, 196, 255),
			new Color(153, 221, 146),
			new Color(255, 101, 132),
			new Color(233, 229, 146),
			new Color(173, 101, 255),
			new Color(255, 101, 244),
		};

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture);
			float r = 0;
			int maxToDraw = Math.Min(shades.Length, EmpowerCount);
			// draw in reverse order for layering purposes
			float headBobOffset = (float)(2 * Math.PI / Math.Max(maxToDraw, 1));
			for(int i = maxToDraw - 1; i >= 0; i--)
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
				pos.X += xOffsets[maxToDraw - 1];
				SpriteEffects effects = projectile.velocity.X < 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				spriteBatch.Draw(texture, pos - Main.screenPosition,
					bounds, shades[i], 0,
					bounds.Center.ToVector2(), 1, effects, 0);
			}

			return false;
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
			isOnGround = InTheGround(new Vector2(projectile.Bottom.X, projectile.Bottom.Y + 8));
			return vectorToIdlePosition;
		}

		// determine whether the minion should be flying or not
		private void SetFlyingState()
		{
			theGround = NearestGroundLocation();
			if(Math.Abs(vectorToIdle.Y) > idleGroundDistance || theGround is null)
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

		// if we've run up against a block, find the nearest clear space in the direction
		// of the target
		private void TeleportTowardsTarget(Vector2 vectorToTarget, int increment = 32)
		{
			Vector2 incrementVector = vectorToTarget;
			incrementVector.Normalize();
			float maxLength = vectorToTarget.Length();
			Vector2? clearSpace = null;
			for(int i = 48; i < maxLength; i+= increment)
			{
				Vector2 position = projectile.Center + incrementVector * i;
				if(!InTheGround(position))
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
			}
		}

		// See if there's a solid block with *top* between us and the target, and teleport on
		// top of it if so
		private void TeleportTowardsCeiling(Vector2 vectorToTarget, int increment = 16)
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
			}
		}

		private void DoTeleport()
		{
			projectile.velocity = Vector2.Zero;
			int teleportFrame = animationFrame - (int)teleportStartFrame;
			int width = widths[Math.Min(shades.Length, EmpowerCount) - 1];
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
				Math.Abs(vectorToIdlePosition.X) < 32;
			if(needsToGoThroughFloor || needsToGoThroughWall || needsToGoThroughCeiling)
			{
				if(needsToGoThroughWall && NearestLedgeLocation(vectorToIdlePosition) != null)
				{
					projectile.velocity.Y = -4;
				} else if(needsToGoThroughWall || needsToGoThroughFloor)
				{
					TeleportTowardsTarget(vectorToIdlePosition);
				} else if(needsToGoThroughCeiling)
				{
					TeleportTowardsCeiling(vectorToIdlePosition);
				}
				didHitWall = false;
				return;
			}
			projectile.tileCollide = true;
			Vector2 groundLocation = (Vector2)theGround;
			float distanceToGround = groundLocation.Y - projectile.Bottom.Y;
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

		public void TargetedFlyingMovement(Vector2 vectorToTargetPosition)
		{
			IdleFlyingMovement(vectorToIdle);
			// todo shoot a projectile or something
		}
		public void TargetedGroundedMovement(Vector2 vectorToTargetPosition)
		{
			IdleGroundedMovement(vectorToTargetPosition);
			// todo shoot a projectile or something
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			IdleMovement(vectorToTargetPosition);
		}

		protected override int ComputeDamage()
		{
			return baseDamage + (baseDamage / 3) * (int)EmpowerCount; // only scale up damage a little bit
		}

		private Vector2? GetTargetVector()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center, searchDistance/2, player.Center) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (ClosestEnemyInRange(searchDistance, player.Center, searchDistance/2, losCenter: player.Center) is Vector2 target2)
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
			return null;
		}

		protected override float ComputeSearchDistance()
		{
			return 600 + 20 * EmpowerCount;
		}

		protected override float ComputeInertia()
		{
			return 5;
		}

		protected override float ComputeTargetedSpeed()
		{
			return ComputeIdleSpeed();
		}

		protected override float ComputeIdleSpeed()
		{
			return isFlying? 12 : 6;
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 0;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if (Math.Abs(projectile.velocity.X) > 2)
			{
				projectile.spriteDirection = projectile.velocity.X > 0 ? -1 : 1;
			}
		}
	}
}
