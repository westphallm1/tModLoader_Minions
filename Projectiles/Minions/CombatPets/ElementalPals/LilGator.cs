using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class LilGatorMinionBuff : CombatPetBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<LilGatorMinion>() };
	}

	public class LilGatorMinionItem : CombatPetCustomMinionItem<LilGatorMinionBuff, LilGatorMinion>
	{
	}

	public class LilGatorMinion : WaterBeamLaserCombatPet
	{
		public override int BuffId => BuffType<LilGatorMinionBuff>();

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
			ConfigureFrames(11, (0, 1), (2, 6), (2, 2), (7, 10));
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
