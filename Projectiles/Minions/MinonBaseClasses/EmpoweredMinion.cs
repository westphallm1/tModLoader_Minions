using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	// Invisible minion that exists to track the empower count of empowered minions
	// Works around many of the issues involved with changing a projectiles minionSlots
	public abstract class CounterMinion : SimpleMinion
	{
		public override string Texture => "Terraria/Item_0";

		protected virtual int MinionType => default;
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return false;
		}


		public override void Behavior()
		{
			projectile.friendly = false;
			projectile.velocity = Vector2.Zero;
			projectile.position = player.Center;
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[MinionType] == 0)
			{
				// hack to prevent multiple 
				if (GetMinionsOfType(projectile.type)[0].whoAmI == projectile.whoAmI)
				{
					Projectile.NewProjectile(player.Top, Vector2.Zero, MinionType, projectile.damage, projectile.knockBack, Main.myPlayer);
				}
			} else
			{
				// do this to prevent NPC projectile reflections from insta-killing the player
				projectile.hostile = false;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
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
			projectile.minionSlots = 0;
		}

		protected abstract void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame);

		protected virtual int CounterType => default;
		protected virtual int EmpowerCount
		{
			get => player == null ? 0 : player.ownedProjectileCounts[CounterType];
		}

		public virtual void OnEmpower()
		{
			// little visual effect on empower
			for (int i = 0; i < dustCount; i++)
			{
				Dust.NewDust(projectile.Center, 16, 16, dustType);
			}
		}

		public override Vector2 IdleBehavior()
		{
			if (baseDamage == -1)
			{
				baseDamage = projectile.damage;
			}
			if (EmpowerCount > previousEmpowerCount)
			{
				OnEmpower();
				previousEmpowerCount = EmpowerCount;
			}
			projectile.damage = ComputeDamage();
			return Vector2.Zero;
		}

		public override Vector2? FindTarget()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance) is Vector2 target2)
			{
				return target2 - projectile.Center;
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
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
		}


		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// alway clamp to the idle position
			float inertia = ComputeInertia();
			float maxSpeed = ComputeIdleSpeed();
			Vector2 speedChange = vectorToIdlePosition - projectile.velocity;
			if (speedChange.Length() > maxSpeed)
			{
				speedChange.SafeNormalize();
				speedChange *= maxSpeed;
			}
			projectile.velocity = (projectile.velocity * (inertia - 1) + speedChange) / inertia;
		}


		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			int max = 0;
			SetMinAndMaxFrames(ref minFrame, ref max);
			maxFrame = max;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= (maxFrame ?? Main.projFrames[projectile.type]) ||
					projectile.frame < minFrame)
				{
					projectile.frame = minFrame;
				}
			}
		}
	}

	/// <summary>
	/// This class, along with the globalItem below it, are used to conditionally
	/// unset the minion flag from EmpoweredMinions in order to prevent them from
	/// being sacrificed when the player uses a summon item. 
	/// </summary>
	public class EmpoweredMinionSacrificeCircumventionModPlayer : ModPlayer
	{

		internal bool ShouldResetMinionStatus { get; set; }

		public override void PreUpdate()
		{
			// re-minionify all EmpoweredMinions
			if(ShouldResetMinionStatus)
			{
				ShouldResetMinionStatus = true;
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if (p.modProjectile is EmpoweredMinion em)
					{
						p.minion = true;
					}
				}
			}
		}
	}

	public class EmpoweredMinionSacrificeCircumventionGlobalItem : GlobalItem
	{

		public override bool CanUseItem(Item item, Player player)
		{
			if(item.summon && ProjectileID.Sets.MinionSacrificable[item.shoot])
			{
				for(int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if(p.owner == player.whoAmI && p.modProjectile is EmpoweredMinion em) {
						player.GetModPlayer<EmpoweredMinionSacrificeCircumventionModPlayer>().ShouldResetMinionStatus = true;
						p.minion = false; // temporarily de-minion it so that it doesn't get sacrificed
					}
				}
			}
			return base.CanUseItem(item, player);
		}

	}
}
