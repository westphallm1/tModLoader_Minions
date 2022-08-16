using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Core.Minions.Effects;
using Microsoft.Xna.Framework;
using System;
using AmuletOfManyMinions.Core;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets
{
	public class VoltBunnyMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<VoltBunnyMinion>() };
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
		public override int BuffId => BuffType<VoltBunnyMinionBuff>();

		private MotionBlurDrawer blurDrawer;
		private int dashStartFrame;
		private Vector2 dashVector;
		private readonly int dashVelocity = 14;
		private readonly int dashDuration = 10;
		private bool isDashing => dashVector != default && AnimationFrame - dashStartFrame < dashDuration;

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
			if(GHelper.isFlying)
			{
				float idleAngle = MathHelper.TwoPi * AnimationFrame / 120f;
				target += 36 * idleAngle.ToRotationVector2();
			}
			return target;
		}

		protected override void IdleFlyingMovement(Vector2 vector)
		{
			if(!isDashing)
			{
				dashStartFrame = AnimationFrame;
				dashVector = vector;
				dashVector.SafeNormalize();
				dashVector *= dashVelocity;
			}
			bool isIdling = VectorToTarget is null && vector.LengthSquared() < 128 * 128;
			if(isIdling)
			{
				base.IdleFlyingMovement(vector);
			} else
			{
				GHelper.DropThroughPlatform();
				Projectile.tileCollide = false;
				Projectile.velocity = dashVector;
			}

			// visual effects
			if(Main.rand.NextBool(isIdling? 8 : 4))
			{
				int dustIdx = Dust.NewDust(
					Projectile.position, Projectile.width, Projectile.height, DustID.t_Martian,
					-dashVector.X/10, -dashVector.Y/10);
				Main.dust[dustIdx].noLight = true;
				Main.dust[dustIdx].scale *= 0.75f;
			}
			if(Main.rand.NextBool(isIdling? 6: 2))
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
				GHelper.isFlying = true;
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
