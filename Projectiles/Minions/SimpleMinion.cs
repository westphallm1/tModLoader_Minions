using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.AI;
using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Items.Accessories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions
{

	public enum WaypointMovementStyle
	{
		IDLE,
		TARGET,
		CUSTOM
	}

	public abstract class SimpleMinion : Minion, ISimpleMinion
	{
		public Vector2 VectorToIdle { get; set; }
		public Vector2? VectorToTarget { get; set; }
		public Vector2 OldVectorToIdle { get; set; }
		public Vector2? OldVectorToTarget { get; set; }
		public int? OldTargetNpcIndex { get; set; }
		public int FramesSinceHadTarget { get; set; } = 0;
		public bool AttackThroughWalls { get; set; } = false;
		public bool DealsContactDamage { get; set; } = true;
		public int FrameSpeed { get; set; } = 5;
		public int ProximityForOnHitTarget { get; set; } = 24;
		public int TargetFrameCounter { get; set; } = 0;
		public int NoLOSPursuitTime { get; set; } = 15; // time to chase the NPC after losing sight
		public MinionPathfindingHelper Pathfinder { get; set; }
		public bool UsesTactics { get; set; } = true;
		public int AnimationFrame { get; set; }
		public virtual WaypointMovementStyle WaypointMovementStyle => WaypointMovementStyle.IDLE;


		public int GroupAnimationFrames = 180;
		public int GroupAnimationFrame
		{
			get => Player.GetModPlayer<MinionSpawningItemPlayer>().idleMinionSyncronizationFrame % GroupAnimationFrames;
		}
		public AttackState AttackState { get; set; } = AttackState.IDLE;

		internal bool IsPrimaryFrame => Projectile.extraUpdates == 0 || AnimationFrame % (Projectile.extraUpdates + 1) == 0;

		internal new SimpleMinionBehavior MinionBehavior;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			// Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			// These below are needed for a minion weapon
			// Only controls if it deals damage to enemies on contact (more on that later)
			Projectile.friendly = true;
			// Determines multiple things
			Projectile.minion = true;
			// Determines damage type
			Projectile.DamageType = DamageClass.Summon; //TODO 1.4 check shot projectiles, the spawn for originalDamage
														// Amount of slots this minion occupies from the total minion slots available to the player (more on that later)
			Projectile.minionSlots = 1f;
			// Needed so the minion doesn't despawn on collision with enemies or tiles
			Projectile.penetrate = -1;
			// Makes the minion go through tiles
			Projectile.tileCollide = false;
			// use local projectile i-frames
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			// Makes sure this projectile is synced to other newly joined players 
			Projectile.netImportant = true;

			Pathfinder = new MinionPathfindingHelper(this);
			MinionBehavior = new SimpleMinionBehavior(this);
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
			Projectile.frameCounter++;
			if (Projectile.frame < minFrame)
			{
				Projectile.frame = minFrame;
			}
			if (Projectile.frameCounter >= FrameSpeed)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= (maxFrame ?? Main.projFrames[Projectile.type]))
				{
					Projectile.frame = minFrame;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = true;
			return true;
		}

		public override void Behavior()
			=> MinionBehavior.MainBehavior();

		/**
		 * Multiplayer safe approximation of OnHitNPC
		 */
		public virtual void OnHitTarget(NPC target)
		{
			// no-op
		}


		// utility methods
		public void TeleportToPlayer(ref Vector2 vectorToIdlePosition, float maxDistance)
			=> MinionBehavior.TeleportToPlayer(ref vectorToIdlePosition, maxDistance);


		public List<Projectile> GetMinionsOfType(int projectileType) => MinionBehavior.GetMinionsOfType(projectileType);

		/**
		 * Optionally introduce a shot spread to minions for a gameplay experience closer to standard
		 * vanilla ai
		 */
		internal Vector2 VaryLaunchVelocity(Vector2 initial) => MinionBehavior.VaryLaunchVelocity(initial);
	}
}
