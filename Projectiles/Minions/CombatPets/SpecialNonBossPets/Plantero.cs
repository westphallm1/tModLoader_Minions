using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using AmuletOfManyMinions.Core.Minions.Effects;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets
{
	public class PlanteroMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<PlanteroMinion>() };
		public override string VanillaBuffName => "Plantero";
		public override int VanillaBuffId => BuffID.Plantero;
	}

	public class PlanteroMinionItem : CombatPetMinionItem<PlanteroMinionBuff, PlanteroMinion>
	{
		internal override string VanillaItemName => "MudBud";
		internal override int VanillaItemID => ItemID.MudBud;
	}


	public class PlanteroMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Plantero;
		public override int BuffId => BuffType<PlanteroMinionBuff>();
		private bool wasFlyingThisFrame =  false;

		private int attackCycle;
		private SkeletronHand[] hands;
		private bool shouldDrawHands;

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 24, -8, -18, -1);
			ConfigureFrames(19, (0, 3), (4, 12), (12, 12), (13, 18));
			hands = new SkeletronHand[2];
			FrameSpeed = 8;
		}

		public override void LoadAssets()
		{
			AddTexture(base.Texture + "Clingers");
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if(GHelper.isFlying && Projectile.velocity.LengthSquared() > 2)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			} else 
			{
				Projectile.rotation = Utils.AngleLerp(Projectile.rotation, 0, 0.25f);
			}
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			if(!wasFlyingThisFrame && GHelper.isFlying)
			{
				var source = Projectile.GetSource_FromThis();
				Gore.NewGore(source, Projectile.Center, Vector2.Zero, GoreID.PlanteroSombrero);
			}
			wasFlyingThisFrame = GHelper.isFlying;

			bool didDrawHands = shouldDrawHands;
			shouldDrawHands = VectorToTarget is Vector2 target && target.LengthSquared() < 
				2 * preferredDistanceFromTarget * preferredDistanceFromTarget;

			// dust to cover up the hands' (dis)appearance
			if(didDrawHands != shouldDrawHands)
			{
				for(int i = 0; i < hands.Length; i++)
				{
					Vector2 handPos = hands[i].Position + Projectile.Center;
					Dust.NewDust(handPos, 8, 8, DustID.Grass);
				}
			}

			DealsContactDamage = shouldDrawHands;
			Projectile.friendly = shouldDrawHands;
			for(int i = 0; i < hands.Length; i++)
			{
				UpdateHand(ref hands[i], i);
				hands[i].UpdatePosition(8);
			}
		}

		public override void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			base.LaunchProjectile(launchVector);
			attackCycle++;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if(shouldDrawHands)
			{
				Texture2D texture = ExtraTextures[0].Value;
				int frameHeight = texture.Height / 3;
				ChainDrawer chainDrawer = new(new Rectangle(0, 2 * frameHeight + 2, texture.Width, frameHeight -2));
				Vector2 center = Projectile.Center;
				for(int i = 0; i < hands.Length; i++)
				{
					chainDrawer.DrawChain(texture, center, center + hands[i].Position, lightColor);
					hands[i].Draw(center, texture, frameHeight, lightColor);
				}
			}
			return true;
		}

		private void UpdateHand(ref SkeletronHand hand, int handIdx)
		{
			Vector2 offset;
			int shootFrame = AnimationFrame - lastFiredFrame;
			if(handIdx != attackCycle % 2 || VectorToTarget is not Vector2 target || shootFrame > attackFrames)
			{
				// very hacky way to get -1 and 1
				Vector2 baseOffset = 32 * Vector2.UnitX * MathF.Sign(handIdx - 0.5f);
				float cycleAngle = MathHelper.TwoPi * AnimationFrame / 120 + handIdx * MathHelper.Pi;
				Vector2 cycleOffset = 8 * cycleAngle.ToRotationVector2();
				offset = baseOffset + cycleOffset;
			} else
			{
				float attackFraction = 1.1f * MathF.Sin(MathHelper.Pi * shootFrame / attackFrames);
				offset = target * attackFraction;
			}
			hand.Rotation = offset.ToRotation() + MathHelper.PiOver2;
			hand.TargetPosition = offset;
			hand.Frame = (AnimationFrame /10) % 2;
			hand.SpriteDirection = handIdx == 0 ? 1 : -1;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(!ShouldDoShootingMovement)
			{
				return base.Colliding(projHitbox, targetHitbox);
			}
			targetHitbox.Inflate(16, 16);
			for(int i = 0; i < hands.Length; i++)
			{
				Vector2 handPos = hands[i].Position + Projectile.Center;
				if(targetHitbox.Contains(handPos.ToPoint()))
				{
					return true;
				}
			}
			return false;
		}
	}
}
