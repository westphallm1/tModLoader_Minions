using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI.ManagedAI
{
	/// <summary>
	/// Base class for the 'boilerplate' AIs used by most combat pets and vanilla 
	/// clone minions. Also suitable for applying a fully managed AI to a 
	/// cross-mod minion or pet. Roughly equivalent to HeadCirclingGroupAwareMinion
	/// in the regular class hierarchy.
	/// </summary>
	internal abstract class GroupAwareCrossModAI : BasicCrossModAI
	{
		internal HeadCirclingHelper CircleHelper { get; set; }

		[CrossModParam]
		[CrossModState]
		public int? FiredProjectileId { get; set; }


		[CrossModParam]
		[CrossModState]
		public virtual int AttackFrames { get; set; }

		[CrossModParam]
		[CrossModState]
		public virtual float AttackFramesScaleFactor { get; set; } = 1f;

		[CrossModParam]
		[CrossModState]
		public float LaunchVelocity { get; set; } = 14;

		[CrossModParam]
		[CrossModState]
		public float LaunchVelocityScaleFactor { get; set; } = 1f;



		[CrossModParam]
		public int PreferredTargetDistance { get; set; } = 128;

		[CrossModParam]
		[CrossModState]
		internal bool UseDefaultIdleAnimation { get; set; }

		public abstract bool IsInFiringRange { get; }

		[CrossModState]
		public bool ShouldFireThisFrame { get; set; }

		protected bool IsIdlingNearPlayer =>
			UseDefaultIdleAnimation && IsIdle && Vector2.DistanceSquared(Projectile.Center, Player.Center) < 164 * 164 &&
			Collision.CanHitLine(Projectile.Center, 1, 1, Player.Center, 1, 1);

		public GroupAwareCrossModAI(Projectile proj, int buffId, int? projId, bool isPet, bool defaultIdle) : base(proj, buffId, isPet: isPet)
		{
			CircleHelper = new HeadCirclingHelper(this);
			FiredProjectileId = projId;
			UseDefaultIdleAnimation = defaultIdle;
		}

		internal override void ApplyPetDefaults()
		{
			base.ApplyPetDefaults();
			// go slower and smaller circle than minions since it's a cute little pet
			CircleHelper.idleBumbleFrames = 90;
			CircleHelper.idleBumbleRadius = 96;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			ShouldFireThisFrame = false;

			if(CircleHelper.IdleBumble && Player.velocity.Length() < 4)
			{
				return CircleHelper.BumblingHeadCircle();
			} else
			{
				return CircleHelper.DirectHeadCircle();
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// Always calculate an cache a 
			Projectile.tileCollide = false;
			if (vectorToIdlePosition.LengthSquared() > MaxSpeed * MaxSpeed)
			{
				vectorToIdlePosition.SafeNormalize();
				vectorToIdlePosition *= MaxSpeed;
			} 
			Projectile.velocity = (Projectile.velocity * (Inertia - 1) + vectorToIdlePosition) / Inertia;
		}

		internal override void UpdatePetState()
		{
			base.UpdatePetState();
			var leveledPetPlayer = Player.GetModPlayer<LeveledCombatPetModPlayer>();
			var info = CombatPetLevelTable.PetLevelTable[leveledPetPlayer.PetLevel];
			AttackFrames = (int)( AttackFramesScaleFactor * Math.Max(30, 60 - 6 * info.Level));
			LaunchVelocity = (int)( LaunchVelocityScaleFactor * (info.BaseSpeed + 3));
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			// Cache the projectile's position and velocity before applying vanilla AI
			ProjCache.Cache(Projectile);
			// Unless the minion/pet is idling and close to the player, and is configured to
			// use its default AI while not attacking
			if(IsIdlingNearPlayer)
			{
				ProjCache.Rollback(Projectile);
			}
		}
	}
}
