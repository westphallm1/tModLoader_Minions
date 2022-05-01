using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Items.Armor;
using Microsoft.Xna.Framework;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using System.Linq;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets
{
	public class BabyImpMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BabyImpMinion>() };
		public override string VanillaBuffName => "BabyImp";
		public override int VanillaBuffId => BuffID.BabyImp;
	}

	public class BabyImpMinionItem : CombatPetMinionItem<BabyImpMinionBuff, BabyImpMinion>
	{
		internal override string VanillaItemName => "HellCake";
		internal override int VanillaItemID => ItemID.HellCake;
	}

	public class BabyImpFireBall: BaseImpFireball
	{
		internal override int DustType => DustID.Shadowflame;
		internal override float DustScale => 1.5f;

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.penetrate = 1; // don't pierce
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			// no on fire debuff
		}
	}

	public class BabyImpMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BabyImp;
		internal override int BuffId => BuffType<BabyImpMinionBuff>();
		internal override int? ProjId => ProjectileType<BabyImpFireBall>();

		private Projectile flameRing;
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 28, 0, -14, -1);
			ConfigureFrames(23, (0, 5), (12, 18), (12, 12), (19, 22));
			frameSpeed = 8;
		}

		public override Vector2 IdleBehavior()
		{
			int minionType = ProjectileType<ImpPortalMinion>();
			flameRing = Main.projectile.Where(p => p.active && p.owner == Projectile.owner && p.type == minionType).FirstOrDefault();
			Vector2 target = base.IdleBehavior();
			if(flameRing != default && flameRing.localAI[0] > 0)
			{
				return flameRing.Center - Projectile.Center;
			} else
			{
				return target;
			}
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(flameRing != default && flameRing.localAI[0] > 0)
			{
				base.IdleFlyingMovement(vectorToIdlePosition);
				gHelper.isFlying = true;
				Projectile.tileCollide = false;
			} else
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (flameRing != null)
			{
				IdleMovement(vectorToIdle);
			} else
			{
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			int yawnCycle = 292;
			int idleFrame = animationFrame % yawnCycle;
			frameInfo[GroundAnimationState.STANDING] = idleFrame < yawnCycle - 40 ? (0, 5) : (6, 11);
			base.Animate(minFrame, maxFrame);
		}
	}
}
