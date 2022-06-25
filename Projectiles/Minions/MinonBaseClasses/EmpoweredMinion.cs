using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	// Invisible minion that exists to track the empower count of empowered minions
	// Works around many of the issues involved with changing a projectiles minionSlots
	public abstract class CounterMinion : SimpleMinion
	{
		public override string Texture => "Terraria/Images/Item_0";

		protected virtual int MinionType => default;
		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}


		public override void Behavior()
		{
			Projectile.friendly = false;
			Projectile.velocity = Vector2.Zero;
			Projectile.position = player.Center;
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[MinionType] == 0)
			{
				// hack to prevent multiple 
				if (GetMinionsOfType(Projectile.type)[0].whoAmI == Projectile.whoAmI)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Top, Vector2.Zero, MinionType, Projectile.damage, Projectile.knockBack, Main.myPlayer);
				}
			} else
			{
				// do this to prevent NPC projectile reflections from insta-killing the player
				Projectile.hostile = false;
			}
		}
		public override Vector2? FindTarget()
		{
			// no op
			return null;
		}

		public override Vector2 IdleBehavior()
		{
			// no op
			return Vector2.Zero;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// no op
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// no op
		}

	}

	public abstract class EmpoweredMinion : SimpleMinion
	{
		protected abstract int ComputeDamage();
		protected abstract float ComputeSearchDistance();
		protected abstract float ComputeInertia();
		protected abstract float ComputeTargetedSpeed();
		protected abstract float ComputeIdleSpeed();

		// drop off damage 
		internal readonly static int MAX_VANILLA_MINIONS = 11;

		protected int baseDamage = -1;
		protected int previousEmpowerCount = 0;
		protected virtual int dustType => DustID.Confetti;
		protected virtual int dustCount => 3;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
		}

		protected abstract void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame);

		public virtual int CounterType => default;
		protected virtual int EmpowerCount => player == null ? 0 : player.ownedProjectileCounts[CounterType];

		public virtual void OnEmpower()
		{
			// little visual effect on empower
			for (int i = 0; i < dustCount; i++)
			{
				Dust.NewDust(Projectile.Center, 16, 16, dustType);
			}
		}

		public override Vector2 IdleBehavior()
		{
			// need to manually fetch the base damage from the counter 
			// minion each frame to keep up with player stat updates
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == player.whoAmI && p.type == CounterType)
				{
					baseDamage = p.originalDamage;
					break;
				}
			}
			if (EmpowerCount > previousEmpowerCount)
			{
				OnEmpower();
				previousEmpowerCount = EmpowerCount;
			}
			Projectile.originalDamage = ComputeDamage();
			return Vector2.Zero;
		}

		// If the player has more than the max vanilla minion slots (eg. using post-ML mods),
		// reduce the amount of extra damage that minions get
		internal float EmpowerCountWithFalloff()
		{
			if(EmpowerCount <= MAX_VANILLA_MINIONS)
			{
				return EmpowerCount;
			} else
			{
				return MAX_VANILLA_MINIONS + MathF.Sqrt(EmpowerCount - MAX_VANILLA_MINIONS);
			}
		}

		public override Vector2? FindTarget()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center) is Vector2 target)
			{
				return target - Projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance) is Vector2 target2)
			{
				return target2 - Projectile.Center;
			}
			else
			{
				return null;
			}
		}


		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			float inertia = ComputeInertia();
			float speed = ComputeTargetedSpeed();
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= speed;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
		}


		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// alway clamp to the idle position
			float inertia = ComputeInertia();
			float maxSpeed = ComputeIdleSpeed();
			Vector2 speedChange = vectorToIdlePosition - Projectile.velocity;
			if (speedChange.Length() > maxSpeed)
			{
				speedChange.SafeNormalize();
				speedChange *= maxSpeed;
			}
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + speedChange) / inertia;
		}


		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			int max = 0;
			SetMinAndMaxFrames(ref minFrame, ref max);
			maxFrame = max;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= (maxFrame ?? Main.projFrames[Projectile.type]) ||
					Projectile.frame < minFrame)
				{
					Projectile.frame = minFrame;
				}
			}
		}
	}

	/// <summary>
	/// This class is used to conditionally reset and then set the minion flag on EmpoweredMinion projectiles
	/// in order to prevent them from being sacrificed when the player uses a summon item. 
	/// </summary>
	public class EmpoweredMinionSacrificeCircumventionSystem : ModSystem
	{
		public override void Load()
		{
			On.Terraria.Player.FreeUpPetsAndMinions += Player_FreeUpPetsAndMinions;
		}

		private void Player_FreeUpPetsAndMinions(On.Terraria.Player.orig_FreeUpPetsAndMinions orig, Player self, Item sItem)
		{
			bool atleastOne = false;
			if (ProjectileID.Sets.MinionSacrificable[sItem.shoot])
			{
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if (p.active && p.owner == self.whoAmI && (p.ModProjectile is EmpoweredMinion || p.ModProjectile is DesertTigerMinion))
					{
						atleastOne = true;
						p.minion = false; // temporarily de-minion it so that it doesn't get sacrificed
					}
				}
			}

			orig(self, sItem);

			// re-minionify all necessary minions
			if (atleastOne)
			{
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if (p.active && p.owner == self.whoAmI && !p.minion && (p.ModProjectile is EmpoweredMinion || p.ModProjectile is DesertTigerMinion))
					{
						p.minion = true;
					}
				}
			}
		}
	}
}
