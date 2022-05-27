using static AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses.CombatPetConvenienceMethods;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Terraria.Localization;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using AmuletOfManyMinions.Items.Accessories;
using System;
using Terraria.Audio;
using AmuletOfManyMinions.Core.Minions.Effects;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd
{
	public class VampireFrogMinionBuff : MinionBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.VampireFrog;

		internal override int[] ProjectileTypes => new int[] { ProjectileType<VampireFrogMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.VampireFrogMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.VampireFrogMinion"));
		}

	}

	public class VampireFrogMinionItem : VanillaCloneMinionItem<VampireFrogMinionBuff, VampireFrogMinion>
	{
		internal override int VanillaItemID => ItemID.VampireFrogStaff;

		internal override string VanillaItemName => "VampireFrogStaff";
	}

	public class VampireFrogMinion : SimpleGroundBasedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.VampireFrog;
		internal override int BuffId => BuffType<VampireFrogMinionBuff>();

		// TODO make the grounded ranged minion state generically available somehow
		internal int preferredDistanceFromTarget = 96;
		internal int lastFiredFrame = 0;
		internal int tongueWhipDuration = 16;
		internal Vector2 tongueFiringVector;
		internal int minTongueLength = 96;
		internal int maxTongueLength = 248;

		internal bool IsFiring => animationFrame - lastFiredFrame < tongueWhipDuration && tongueFiringVector != default;
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(this, 24, 30, -54, -22);
			ConfigureFrames(24, (0, 0), (5, 13), (7, 7), (14, 17));
			xMaxSpeed = 9;
			attackFrames = 45;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			var animState = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			frameSpeed = animState == GroundAnimationState.WALKING ? 2 : 5;
			int croakCycle = 200;
			int croakStartFrame = croakCycle - 8 * 5;
			int idleFrame = animationFrame % croakCycle;
			if(animState == GroundAnimationState.STANDING && idleFrame > croakStartFrame)
			{
				int croakAnimFrame = (idleFrame - croakStartFrame) / 5;
				Projectile.frame = croakAnimFrame < 4 ? 1 + croakAnimFrame : 8 - croakAnimFrame;
			}
			if(IsFiring)
			{
				Projectile.spriteDirection = Math.Sign(tongueFiringVector.X);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(!IsFiring || 
				Vector2.DistanceSquared(projHitbox.Center.ToVector2(), targetHitbox.Center.ToVector2()) > 25 * maxTongueLength * maxTongueLength ||
				!Collision.CanHitLine(projHitbox.Center.ToVector2(), 1, 1, targetHitbox.Center.ToVector2(), 1, 1))
			{
				return false;
			}
			targetHitbox.Inflate(16, 16);
			bool anyHits = false;
			new WhipDrawer(GetTongueFrame, tongueWhipDuration).ApplyWhipSegments(
				Projectile.Center, Projectile.Center + tongueFiringVector, animationFrame - lastFiredFrame,
				// TODO short circuit somehow
				(midPoint, rotation, bounds) => { anyHits |= targetHitbox.Contains(midPoint.ToPoint()); });
			return anyHits;
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{
			DoDefaultGroundedMovement(vector);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if(!IsFiring)
			{
				return true;
			}
			new WhipDrawer(GetTongueFrame, tongueWhipDuration).DrawWhip(
				Terraria.GameContent.TextureAssets.Projectile[Type].Value, 
				Projectile.Center, Projectile.Center + tongueFiringVector, animationFrame - lastFiredFrame);
			return true;
		}

		private Rectangle GetTongueFrame(int frameIdx, bool isLast)
		{
			if(frameIdx == 0)
			{
				return new(92, 1112, 14, 14);
			} else if(isLast)
			{
				return new(120, 1112, 14, 14);
			} else
			{
				return new(106, 1112, 14, 14);
			}
		}

		private void SetupTongue(Vector2 target)
		{
			lastFiredFrame = animationFrame;
			tongueFiringVector = target;
			if(tongueFiringVector.LengthSquared() < minTongueLength * minTongueLength)
			{
				tongueFiringVector.Normalize();
				tongueFiringVector *= minTongueLength;
			} else if (tongueFiringVector.LengthSquared() > maxTongueLength * maxTongueLength)
			{
				tongueFiringVector.Normalize();
				tongueFiringVector *= maxTongueLength;
			}
			SoundEngine.PlaySound(SoundID.Item153 with { Volume = 0.5f }, Projectile.position);

		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{

			if (Math.Abs(vectorToTargetPosition.X) < 2 * preferredDistanceFromTarget &&
				Math.Abs(vectorToTargetPosition.Y) < 2 * preferredDistanceFromTarget &&
				animationFrame - lastFiredFrame >= attackFrames)
			{
				SetupTongue(vectorToTargetPosition);
			}

			// don't move if we're in range-ish of the target
			if (Math.Abs(vectorToTargetPosition.X) < 1.25f * preferredDistanceFromTarget && 
				Math.Abs(vectorToTargetPosition.X) > 0.5f * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X = 0;
			} else if (Math.Abs(vectorToTargetPosition.X) < 0.5f * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X -= Math.Sign(vectorToTargetPosition.X) * 0.75f * preferredDistanceFromTarget;
			}

			if(Math.Abs(vectorToTargetPosition.Y) < 1.25f * preferredDistanceFromTarget && 
				Math.Abs(vectorToTargetPosition.Y) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.Y = 0;
			}
			base.TargetedMovement(vectorToTargetPosition);
		}
	}
}
