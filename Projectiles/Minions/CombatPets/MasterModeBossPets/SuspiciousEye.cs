using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class SuspiciousEyeMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SuspiciousEyeMinion>() };

		public override int VanillaBuffId => BuffID.EyeOfCthulhuPet;

		public override string VanillaBuffName => "EyeOfCthulhuPet";
	}

	public class SuspiciousEyeMinionItem : CombatPetMinionItem<SuspiciousEyeMinionBuff, MiniRetinazerMinion>
	{
		internal override int VanillaItemID => ItemID.EyeOfCthulhuPetItem;
		internal override int AttackPatternUpdateTier => (int)CombatPetTier.Skeletal;
		internal override string VanillaItemName => "EyeOfCthulhuPetItem";
	}

	public class SuspiciousEyeMinion : CombatPetHoverDasherMinion
	{
		public override int BuffId => BuffType<SuspiciousEyeMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.EyeOfCthulhuPet;

		internal override bool DoBumblingMovement => leveledPetPlayer.PetLevel < (int)CombatPetTier.Skeletal;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Spazmamini") + " (AoMM Version)");
			Main.projFrames[Projectile.type] = 20;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			attackFrames = 90;
			circleHelper.idleBumbleFrames = 90;
			FrameSpeed = 5;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(!DoBumblingMovement && VectorToTarget != null && 
				AnimationFrame - hsHelper.lastShootFrame <= attackFrames)
			{
				minFrame = 10;
				maxFrame = 16;
			} else
			{
				minFrame = 0;
				maxFrame = 6;
			}
			base.Animate(minFrame, maxFrame);
			Projectile.rotation = 0.05f * Projectile.velocity.X;
		}
	}
}
