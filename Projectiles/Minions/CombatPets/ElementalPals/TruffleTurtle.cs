﻿using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Items.Armor;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class TruffleTurtleMinionBuff : CombatPetBuff
    {
        internal override int[] ProjectileTypes => new int[] { ProjectileType<TruffleTurtleMinion>() };
	}

	public class TruffleTurtleMinionItem : CombatPetCustomMinionItem<TruffleTurtleMinionBuff, TruffleTurtleMinion>
	{
	}

	public class TruffleTurtleMinion : CombatPetGroundedRangedMinion
	{
		public override int BuffId => BuffType<TruffleTurtleMinionBuff>();

		internal override bool ShouldDoShootingMovement => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Skeletal;

		internal override int? ProjId => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Spectre ?
			ProjectileType<LeafBlade>() :
			ProjectileType<SaplingMinionLeafProjectile>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();

			ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, 2, 10)
				.WithSpriteDirection(-1)
				.WithOffset(-2, 0)
				.WhenSelected(2, 5, 5);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(30, 30, -6, -6, -1);
			ConfigureFrames(8, (0, 1), (2, 6), (2, 2), (7, 7));
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = GHelper.GetAnimationState();
			FrameSpeed = (state == GroundAnimationState.WALKING) ? 5 : 10;
			base.Animate(minFrame, maxFrame);
			if(state == GroundAnimationState.JUMPING)
			{
				Projectile.frame = Projectile.velocity.Y > 0 ? 2 : 5;
			} 
		}
	}
}
