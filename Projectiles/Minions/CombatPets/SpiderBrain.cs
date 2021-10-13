using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets
{
	public class SpiderBrainMinionBuff : CombatPetVanillaCloneBuff
	{
		public SpiderBrainMinionBuff() : base(ProjectileType<SpiderBrainMinion>()) { }

		public override int VanillaBuffId => BuffID.BrainOfCthulhuPet;

		public override string VanillaBuffName => "BrainOfCthulhuPet";
	}

	public class SpiderBrainMinionItem : CombatPetMinionItem<SpiderBrainMinionBuff, SpiderBrainMinion>
	{
		internal override int VanillaItemID => ItemID.BrainOfCthulhuPetItem;

		internal override string VanillaItemName => "BrainOfCthulhuPetItem";
	}


	/// <summary>
	/// Uses ai[0] for projectile to return to 
	/// </summary>
	public class SpiderBrainEyeProjectile : ModProjectile
	{
		const int TimeToLive = 60;

		private bool returning = false;

		const int timeToReturn = 15;

		private Projectile returnTarget;
		private float maxSpeed;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = TimeToLive;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 15;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			Projectile.rotation += MathHelper.Pi / 15;
			if(Projectile.timeLeft < TimeToLive - timeToReturn)
			{
				returning = true;
			}
			if(returnTarget == null)
			{
				returnTarget = Main.projectile[(int)Projectile.ai[0]];
				maxSpeed = Projectile.velocity.Length();
			}
			if(!returnTarget.active || returning && Vector2.DistanceSquared(returnTarget.Center, Projectile.Center) < 32 * 32)
			{
				Projectile.Kill();
				return;
			}
			if(returning)
			{
				int inertia = 8;
				Vector2 target = returnTarget.Center - Projectile.Center;
				target.Normalize();
				target *= maxSpeed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + target) / inertia;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// don't collide if no LOS to brain
			if(projHitbox.Intersects(targetHitbox) && !Collision.CanHitLine(Projectile.Center, 1, 1, returnTarget.Center, 1,1))
			{
				return false;
			}
			return base.Colliding(projHitbox, targetHitbox);
		}
	}

	public class SpiderBrainMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BrainOfCthulhuPet;
		internal override int BuffId => BuffType<SpiderBrainMinionBuff>();

		internal override int GetAttackFrames(CombatPetLevelInfo info) => base.GetAttackFrames(info) / 4;

		// only fire every third projectile towards the actual enemy
		int fireCount;
		// don't get too close
		private Dictionary<GroundAnimationState, (int, int?)> frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
		{
			[GroundAnimationState.FLYING] = (9, 14),
			[GroundAnimationState.JUMPING] = (1, 1),
			[GroundAnimationState.STANDING] = (0, 0),
			[GroundAnimationState.WALKING] = (1, 8),
		};

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.SpiderBrain"));
			Main.projFrames[Projectile.type] = 14;
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 32;
			// DrawOffsetX = -2;
			DrawOriginOffsetY = -4;
			attackFrames = 12;
		}

		public override void LaunchProjectile()
		{
			int eyeVelocity = 10;
			lastFiredFrame = animationFrame;
			SoundEngine.PlaySound(new LegacySoundStyle(2, 17), Projectile.position);
			if (player.whoAmI == Main.myPlayer)
			{
				Vector2 angleToTarget = (Vector2)vectorToTarget;
				angleToTarget.SafeNormalize();
				angleToTarget *= eyeVelocity;
				if(targetNPCIndex is int idx)
				{
					Vector2 targetVelocity = Main.npc[idx].velocity;
					if(targetVelocity.Length() > 32)
					{
						targetVelocity.Normalize();
						targetVelocity *= 32;
					}
					angleToTarget += targetVelocity / 4;
				}
				Vector2 fireDirection = angleToTarget.RotatedBy(2 * (MathHelper.Pi * fireCount++) / 5);
				Projectile.NewProjectile(
					Projectile.GetProjectileSource_FromThis(),
					Projectile.Center,
					VaryLaunchVelocity(fireDirection),
					ProjectileType<SpiderBrainEyeProjectile>(),
					Projectile.damage,
					Projectile.knockBack,
					player.whoAmI,
					ai0: Projectile.whoAmI);
			}
		}


		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			if (vectorToTarget is Vector2 target && Math.Abs(target.X) < 1.5 * preferredDistanceFromTarget)
			{
				Projectile.spriteDirection = Math.Sign(target.X);
			} else if (Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = 1;
			} else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = -1;
			}
		}
	}
}
