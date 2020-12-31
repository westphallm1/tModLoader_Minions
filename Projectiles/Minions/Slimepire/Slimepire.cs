using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.Slimepire
{
	public class SlimepireMinionBuff : MinionBuff
	{
		public SlimepireMinionBuff() : base(ProjectileType<SlimepireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Slimepire");
			Description.SetDefault("A winged acorn will fight for you!");
		}
	}

	public class SlimepireMinionItem : MinionItem<SlimepireMinionBuff, SlimepireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Slimepire Staff");
			Tooltip.SetDefault("Summons a winged slime to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 10;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 2, 0);
			item.rare = ItemRarityID.White;
		}
	}

	public class SlimepireMinion : HeadCirclingGroupAwareMinion<SlimepireMinionBuff>, IGroundAwareMinion
	{
		private GroundAwarenessHelper gHelper;
		private float intendedX = 0;
		int searchDistance = 600;
		private int lastHitFrame;

		public int animationFrame { get; set; }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Slimepire");
			Main.projFrames[projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 20;
			projectile.height = 20;
			drawOffsetX = (projectile.width - 44) / 2;
			drawOriginOffsetY = (projectile.height - 32) / 2;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			gHelper = new GroundAwarenessHelper(this)
			{
				ScaleLedge = ScaleLedge,
				IdleFlyingMovement = IdleFlyingMovement,
				IdleGroundedMovement = IdleGroundedMovement,
				GetUnstuck = GetUnstuck,
				transformRateLimit = 60
			};
		}

		public override Vector2 IdleBehavior()
		{
			animationFrame++;
			gHelper.SetIsOnGround();
			// the ground-based slime can sometimes bounce its way around 
			// a corner, but the flying version can't
			noLOSPursuitTime = gHelper.isFlying ? 15 : 300;
			return base.IdleBehavior();
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			projectile.spriteDirection = vectorToIdlePosition.X > 0 ? -1 : 1;
			gHelper.DoIdleMovement(vectorToIdlePosition, vectorToTarget, searchDistance, 180f);
		}

		private void GetUnstuck(Vector2 destination, int startFrame, ref bool done)
		{
			if(vectorToTarget is null || gHelper.stuckInfo.overCliff)
			{
				Vector2 vectorToUnstuck = destination - projectile.Center;
				if(vectorToUnstuck.Length() < 16)
				{
					done = true;
				} else
				{
					base.IdleMovement(vectorToUnstuck);
				}
			} else
			{
				base.IdleMovement(vectorToIdle);
				if(vectorToIdle.Length() < 16)
				{
					done = true;
				}
			}
		}

		private void IdleGroundedMovement(Vector2 vector)
		{
			gHelper.ApplyGravity();
			if (vector.Y < -96 && Math.Abs(vector.X) < 64 && vectorToTarget != null)
			{
				gHelper.isFlying = true;
				if(gHelper.isFlying)
				{
					IdleFlyingMovement(vector);
				}
				return;
			}
			if(!gHelper.didJustLand)
			{
				projectile.velocity.X = intendedX;
				// only path after landing
				return;
			}
			if(vector.Y > 0 && gHelper.DropThroughPlatform())
			{
				return;
			}
			StuckInfo info = gHelper.GetStuckInfo(vector);
			if(info.isStuck)
			{
				gHelper.GetUnstuckByTeleporting(info, vector);
			}
			gHelper.DoJump(vector);
			int maxHorizontalSpeed = vector.Y < -64 ? 3 : 6;
			projectile.velocity.X = Math.Min(maxHorizontalSpeed, Math.Abs(vector.X) /16) * Math.Sign(vector.X);
			intendedX = projectile.velocity.X;
		}

		private void IdleFlyingMovement(Vector2 vector)
		{
			if (!gHelper.DropThroughPlatform() && animationFrame - lastHitFrame > 15)
			{
				base.IdleMovement(vector);
			}
		}

		public override void OnHitTarget(NPC target)
		{
			lastHitFrame = animationFrame;
		}

		private bool ScaleLedge(Vector2 vector)
		{
			gHelper.DoJump(vector);
			return true;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			minFrame = gHelper.isFlying ? 0 : 4;
			maxFrame = gHelper.isFlying ? 4 : 6;
			projectile.frameCounter++;
			if(projectile.frame < minFrame)
			{
				projectile.frame = minFrame;
			}
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= maxFrame)
				{
					projectile.frame = minFrame;
				}
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			IdleMovement(vectorToTargetPosition);
			projectile.tileCollide = true;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = false;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			gHelper.DoTileCollide(oldVelocity);
			return false;
		}

		public override Vector2? FindTarget()
		{
			if(Vector2.Distance(player.Center, projectile.Center) > 1.5f * searchDistance)
			{
				return null;
			} 
			else if (PlayerTargetPosition(searchDistance, player.Center) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (ClosestEnemyInRange(searchDistance, player.Center) is Vector2 target2)
			{
				return target2 - projectile.Center;
			}
			else
			{
				return null;
			}
		}
	}
}
