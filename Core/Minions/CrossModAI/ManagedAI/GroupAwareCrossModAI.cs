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

		internal bool IsPet { get; set; } = true;

		internal int? FiredProjectileId { get; set; }

		public GroupAwareCrossModAI(Projectile proj, int buffId, int? projId) : base(proj, buffId)
		{
			CircleHelper = new HeadCirclingHelper(this);
			FiredProjectileId = projId;
		}

		internal virtual void ApplyPetDefaults()
		{
			Projectile.minion = true;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			// go slower and smaller circle than minions since it's a cute little pet
			CircleHelper.idleBumbleFrames = 90;
			CircleHelper.idleBumbleRadius = 96;
		}

		internal virtual void UpdatePetState()
		{
			var leveledPetPlayer = Player.GetModPlayer<LeveledCombatPetModPlayer>();
			var info = CombatPetLevelTable.PetLevelTable[leveledPetPlayer.PetLevel];
			SearchRange = info.BaseSearchRange;
			Inertia = info.Level < 6 ? 10 : 15 - info.Level;
			MaxSpeed = (int)info.BaseSpeed;
			Projectile.originalDamage = leveledPetPlayer.PetDamage;
		}

		public override Vector2 IdleBehavior()
		{
			if(IsPet) { UpdatePetState(); }

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
			if (vectorToIdlePosition.LengthSquared() > MaxSpeed * MaxSpeed)
			{
				vectorToIdlePosition.SafeNormalize();
				vectorToIdlePosition *= MaxSpeed;
				Projectile.velocity = (Projectile.velocity * (Inertia - 1) + vectorToIdlePosition) / Inertia;
			} else
			{
				Projectile.velocity = vectorToIdlePosition;
			}
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			// Always cache the projectile position, since we're always overriding it
			ProjCache.Cache(Projectile);
		}
	}
}
