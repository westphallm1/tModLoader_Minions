using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Core.Minions.Effects;
using Microsoft.Xna.Framework;
using System;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets
{
	public class VoltBunnyMinionBuff : CombatPetVanillaCloneBuff
	{
		public VoltBunnyMinionBuff() : base(ProjectileType<VoltBunnyMinion>()) { }
		public override string VanillaBuffName => "VoltBunny";
		public override int VanillaBuffId => BuffID.VoltBunny;
	}

	public class VoltBunnyMinionItem : CombatPetMinionItem<VoltBunnyMinionBuff, VoltBunnyMinion>
	{
		internal override string VanillaItemName => "LightningCarrot";
		internal override int VanillaItemID => ItemID.LightningCarrot;
	}

	public class VoltBunnyMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.VoltBunny;
		internal override int BuffId => BuffType<VoltBunnyMinionBuff>();

		private MotionBlurDrawer blurDrawer;
		private int dashStartFrame;
		private Vector2 dashVector;
		private readonly int dashVelocity = 14;
		private readonly int dashDuration = 10;
		private bool isDashing => dashVector != default && animationFrame - dashStartFrame < dashDuration;

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(30, 24, -4, -12, -1);
			ConfigureFrames(11, (0, 0), (1, 6), (4, 4), (7, 10));
			blurDrawer = new MotionBlurDrawer(5);
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 target = base.IdleBehavior();
			if(gHelper.isFlying)
			{
				float idleAngle = MathHelper.TwoPi * animationFrame / 120f;
				target += 36 * idleAngle.ToRotationVector2();
			}
			return target;
		}

		protected override void IdleFlyingMovement(Vector2 vector)
		{
			if(!isDashing)
			{
				dashStartFrame = animationFrame;
				dashVector = vector;
				dashVector.SafeNormalize();
				dashVector *= dashVelocity;
			}
			bool isIdling = vectorToTarget is null && vector.LengthSquared() < 128 * 128;
			if(isIdling)
			{
				base.IdleFlyingMovement(vector);
			} else
			{
				gHelper.DropThroughPlatform();
				Projectile.tileCollide = false;
				Projectile.velocity = dashVector;
			}

			// visual effects
			if(Main.rand.Next(isIdling? 8 : 4) == 0)
			{
				int dustIdx = Dust.NewDust(
					Projectile.position, Projectile.width, Projectile.height, DustID.t_Martian,
					-dashVector.X/10, -dashVector.Y/10);
				Main.dust[dustIdx].noLight = true;
				Main.dust[dustIdx].scale *= 0.75f;
			}
			if(Main.rand.Next(isIdling? 6: 2) == 0)
			{
				var source = Projectile.GetSource_FromThis();
				int goreIdx = Gore.NewGore(source, Projectile.Center, Vector2.Zero, GoreID.LightningBunnySparks);
				Main.gore[goreIdx].position = Projectile.position;
				Main.gore[goreIdx].velocity = Vector2.Zero;
				Main.gore[goreIdx].scale = Main.rand.NextFloat(0.8f, 1.2f);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(vectorToTargetPosition.LengthSquared() < 128 * 128)
			{
				gHelper.isFlying = true;
				IdleFlyingMovement(vectorToTargetPosition);
			} else
			{
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			blurDrawer.Update(Projectile.Center, isDashing);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if(isDashing)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == forwardDir ? 0 : MathHelper.Pi);
			}
		}
	}
}
