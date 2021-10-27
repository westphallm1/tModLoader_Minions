using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using Microsoft.Xna.Framework;
using System;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class TurtleMinionBuff : CombatPetVanillaCloneBuff
	{
		public TurtleMinionBuff() : base(ProjectileType<TurtleMinion>()) { }
		public override string VanillaBuffName => "PetTurtle";
		public override int VanillaBuffId => BuffID.PetTurtle;
	}

	public class TurtleMinionItem : CombatPetMinionItem<TurtleMinionBuff, TurtleMinion>
	{
		internal override string VanillaItemName => "Seaweed";
		internal override int VanillaItemID => ItemID.Seaweed;
	}

	public class TurtleMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Turtle;
		internal override int BuffId => BuffType<TurtleMinionBuff>();
		internal override int? ProjId => null;

		internal override int GetAttackFrames(CombatPetLevelInfo info) => Math.Max(50, 65 - 3 * info.Level);

		private Vector2 launchPos;
		private int bounceCycleLength => (int)(0.75f * attackFrames);
		private bool IsBouncing => animationFrame - lastFiredFrame < bounceCycleLength;
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 24, -8, -8, -1);
			ConfigureFrames(16, (0, 0), (0, 5), (0, 0), (6, 15));
			preferredDistanceFromTarget = 64;
		}

		// If your name is Kyary...
		private void DoBounce()
		{
			Projectile.tileCollide = true;
			if(animationFrame - lastFiredFrame > 10 && Projectile.velocity.Y < 16)
			{
				Projectile.velocity.Y += 0.5f;
			}
			if(Vector2.DistanceSquared(launchPos, Projectile.position) > 240 * 240)
			{
				// snap out of bounce if we go too far in a straight line
				lastFiredFrame = animationFrame - bounceCycleLength;
			}
		}

		// lifted from Tumblesheep

		// TODO: Refactor into a BounceHelper class
		private void LaunchBounce(Vector2 vectorToTarget)
		{
			lastFiredFrame = animationFrame;
			launchPos = Projectile.position;
			if(targetNPCIndex is int idx && Main.npc[idx].active)
			{
				vectorToTarget += 4 * Main.npc[idx].velocity; // track the target NPC a bit
			}
			if(gHelper.didJustLand && vectorToTarget.Y > -Math.Abs(vectorToTarget.X/4))
			{
				vectorToTarget.Y = -Math.Abs(vectorToTarget.X / 4);
			}
			vectorToTarget.SafeNormalize();
			vectorToTarget *= 8;
			Projectile.velocity = vectorToTarget;
		}

		public override void LaunchProjectile(Vector2 launchVector)
		{
			LaunchBounce((Vector2)vectorToTarget);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if(IsBouncing)
			{
				bool hitFloor = (Projectile.velocity.Y == 0 || Projectile.velocity.Y == -0.5f) && oldVelocity.Y >= 0;
				bool hitCeil = (Projectile.velocity.Y == 0 || Projectile.velocity.Y == 0.5f) && oldVelocity.Y < 0;
				bool hitWall = Projectile.velocity.X == 0;
				if(hitFloor)
				{
					Projectile.velocity.Y = Math.Min(-4, -Projectile.oldVelocity.Y * 0.95f);
				} 				
				if(hitCeil)
				{
					Projectile.velocity.Y = Math.Max(4, -Projectile.oldVelocity.Y);
				}
				if(hitWall)
				{
					Projectile.velocity.X = -oldVelocity.X;
				}
				return false;
			} else
			{
				return base.OnTileCollide(oldVelocity);
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(IsBouncing)
			{
				Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
				Projectile.rotation += Projectile.spriteDirection * MathHelper.Pi / 16;
				return;
			} else
			{
				Projectile.rotation = 0;
				base.Animate(minFrame, maxFrame);
			} 				
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			Projectile.friendly = IsBouncing;
			Projectile.tileCollide |= IsBouncing;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(IsBouncing)
			{
				DoBounce();
			} else
			{
				base.TargetedMovement(vectorToTargetPosition);
			}

		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(IsBouncing)
			{
				DoBounce();
				return;
			} else
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}

	}
}
