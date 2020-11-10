using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions
{
	public abstract class SimpleMinion<T> : Minion<T> where T : ModBuff
	{
		protected Vector2 vectorToIdle;
		protected Vector2? vectorToTarget;
		protected Vector2 oldVectorToIdle;
		protected Vector2? oldVectorToTarget = null;
		protected int? oldTargetNpcIndex = null;
		protected int framesSinceHadTarget = 0;
		protected bool attackThroughWalls = false;
		protected int frameSpeed = 5;
		protected int proximityForOnHitTarget = 24;
		protected int targetFrameCounter = 0;
		public AttackState attackState = AttackState.IDLE;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
			// Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
			ProjectileID.Sets.Homing[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			// These below are needed for a minion weapon
			// Only controls if it deals damage to enemies on contact (more on that later)
			projectile.friendly = true;
			// Only determines the damage type
			projectile.minion = true;
			// Amount of slots this minion occupies from the total minion slots available to the player (more on that later)
			projectile.minionSlots = 1f;
			// Needed so the minion doesn't despawn on collision with enemies or tiles
			projectile.penetrate = -1;
			// Makes the minion go through tiles
			projectile.tileCollide = false;
			// use local projectile i-frames
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 10;
			// Makes sure this projectile is synced to other newly joined players 
			projectile.netImportant = true;
		}


		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles()
		{
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage()
		{
			return true;
		}

		public abstract Vector2 IdleBehavior();
		public abstract Vector2? FindTarget();
		public abstract void IdleMovement(Vector2 vectorToIdlePosition);
		public abstract void TargetedMovement(Vector2 vectorToTargetPosition);

		public virtual void AfterMoving() { }
		public virtual void Animate(int minFrame = 0, int? maxFrame = null)
		{

			// This is a simple "loop through all frames from top to bottom" animation
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= (maxFrame ?? Main.projFrames[projectile.type]))
				{
					projectile.frame = minFrame;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = true;
			return true;
		}

		public override void Behavior()
		{
			targetNPCIndex = null;
			vectorToIdle = IdleBehavior();
			vectorToTarget = FindTarget();
			framesSinceHadTarget++;
			if (vectorToTarget is Vector2 targetPosition)
			{
				if(player.whoAmI == Main.myPlayer && oldVectorToTarget == null)
				{
					projectile.netUpdate = true;
				}
				projectile.tileCollide = !attackThroughWalls;
				framesSinceHadTarget = 0;
				TargetedMovement(targetPosition);
				oldVectorToTarget = vectorToTarget;
				oldTargetNpcIndex = targetNPCIndex;
			}
			else if (attackState != AttackState.RETURNING && oldTargetNpcIndex is int previousIndex && framesSinceHadTarget < 15)
			{
				projectile.tileCollide = !attackThroughWalls;
				if (previousIndex < Main.maxNPCs)
				{
					TargetedMovement(Main.npc[previousIndex].Center); // don't immediately give up if losing LOS
				}
			}
			else
			{
				if (framesSinceHadTarget > 30)
				{
					projectile.tileCollide = false;
				}
				if(player.whoAmI == Main.myPlayer && oldVectorToTarget != null)
				{
					projectile.netUpdate = true;
				}
				oldVectorToTarget = null;
				IdleMovement(vectorToIdle);
			}
			if(targetNPCIndex is int idx &&
				targetFrameCounter ++ > projectile.localNPCHitCooldown && 
				vectorToTarget is Vector2 target && target.LengthSquared() < proximityForOnHitTarget * proximityForOnHitTarget)
			{
				targetFrameCounter = 0;
				OnHitTarget(Main.npc[idx]);	
			}
			AfterMoving();
			Animate();
			oldVectorToIdle = vectorToIdle;
		}

		/**
		 * Multiplayer safe approximation of OnHitNPC
		 */
		public virtual void OnHitTarget(NPC target)
		{

		}


		// utility methods
		public void TeleportToPlayer(ref Vector2 vectorToIdlePosition, float maxDistance)
		{
			if (Main.myPlayer == player.whoAmI && vectorToIdlePosition.Length() > maxDistance)
			{
				projectile.position += vectorToIdlePosition;
				projectile.velocity = Vector2.Zero;
				projectile.netUpdate = true;
				vectorToIdlePosition = Vector2.Zero;
			}
		}


		public List<Projectile> GetMinionsOfType(int projectileType)
		{
			var otherMinions = new List<Projectile>();
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (other.active && other.owner == projectile.owner && other.type == projectileType)
				{
					otherMinions.Add(other);
				}
			}
			otherMinions.Sort((x, y) => x.minionPos - y.minionPos);
			return otherMinions;
		}
	}
}
