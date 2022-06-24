using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using Microsoft.Xna.Framework;
using System;
using AmuletOfManyMinions.Core.Minions.Effects;
using Terraria.Audio;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets
{
	public class SquashlingMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SquashlingMinion>() };
		public override string VanillaBuffName => "Squashling";
		public override int VanillaBuffId => BuffID.Squashling;
	}

	public class SquashlingMinionItem : CombatPetMinionItem<SquashlingMinionBuff, SquashlingMinion>
	{
		internal override string VanillaItemName => "MagicalPumpkinSeed";
		internal override int VanillaItemID => ItemID.MagicalPumpkinSeed;
	}

	public class SquashlingPumpkinBomb : WeakPumpkinBomb
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/PumpkinSquire/PumpkinBomb";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
		}
	}

	public class SquashlingMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Squashling;
		internal override int BuffId => BuffType<SquashlingMinionBuff>();
		internal override int? ProjId => ProjectileID.None; // needs to be non null for super to invoke on fire

		internal int vineWhipDuration => Math.Min(18, 3 * attackFrames / 4);
		internal Vector2 vineFiringVector;
		internal int minVineLength = 96;
		internal int maxVineLength => 232 + 16 * leveledPetPlayer.PetLevel;

		internal bool IsFiring => animationFrame - lastFiredFrame < vineWhipDuration && vineFiringVector != default;
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -16, -12, -1);
			ConfigureFrames(13, (0, 0), (1, 6), (7, 7), (7, 12));
			preferredDistanceFromTarget = 96;
		}

		public override void LoadAssets()
		{
			base.LoadAssets();
			AddTexture(base.Texture + "Whip");
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(!IsFiring || 
				Vector2.DistanceSquared(projHitbox.Center.ToVector2(), targetHitbox.Center.ToVector2()) > 10 * maxVineLength * maxVineLength ||
				!Collision.CanHitLine(projHitbox.Center.ToVector2(), 1, 1, targetHitbox.Center.ToVector2(), 1, 1))
			{
				return false;
			}
			targetHitbox.Inflate(16, 16);
			bool anyHits = false;
			new WhipDrawer(GetVineFrame, vineWhipDuration).ApplyWhipSegments(
				Projectile.Center, Projectile.Center + vineFiringVector, animationFrame - lastFiredFrame,
				// TODO short circuit somehow
				(midPoint, rotation, bounds) => { anyHits |= targetHitbox.Contains(midPoint.ToPoint()); });
			return anyHits;
		}

		private Rectangle GetVineFrame(int frameIdx, bool isLast)
		{
			if(isLast)
			{
				return new(40, 0, 20, 44);
			} else
			{
				return new(20 * (frameIdx % 2), 0, 20, 44);
			}
		}

		public override void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			if(vectorToTarget is not Vector2 target)
			{
				return;
			}
			lastFiredFrame = animationFrame;
			vineFiringVector = target;
			if(vineFiringVector.LengthSquared() < minVineLength * minVineLength)
			{
				vineFiringVector.Normalize();
				vineFiringVector *= minVineLength;
			} else if (vineFiringVector.LengthSquared() > maxVineLength * maxVineLength)
			{
				vineFiringVector.Normalize();
				vineFiringVector *= maxVineLength;
			}
			SoundEngine.PlaySound(SoundID.Item153 with { Volume = 0.5f }, Projectile.position);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if(!IsFiring)
			{
				return true;
			}
			new WhipDrawer(GetVineFrame, vineWhipDuration).DrawWhip(
				ExtraTextures[0].Value, 
				Projectile.Center, Projectile.Center + vineFiringVector, animationFrame - lastFiredFrame);
			return true;
		}

		public override void AfterMoving()
		{
			// Don't disable collisions
		}
	}
}
