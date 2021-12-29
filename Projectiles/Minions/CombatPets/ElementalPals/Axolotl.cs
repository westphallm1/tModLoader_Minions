using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class AxolotlMinionBuff : CombatPetBuff
	{
		public AxolotlMinionBuff() : base(ProjectileType<AxolotlMinion>()) { }
	}

	public class AxolotlMinionItem : CombatPetMinionItem<AxolotlMinionBuff, AxolotlMinion>
	{
		internal override string VanillaItemName => "FullMoonSqueakyToy";
		internal override int VanillaItemID => ItemID.FullMoonSqueakyToy;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Axolotl");
		}
	}

	public class AxolotlMinion : CombatPetGroundedRangedMinion
	{
		internal override int BuffId => BuffType<AxolotlMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(30, 24, -6, -2, -1);
			ConfigureFrames(12, (0, 1), (2, 6), (2, 2), (7, 11));
		}

		internal override bool ShouldDoShootingMovement => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Skeletal;

		internal override int? ProjId => ProjectileType<SharkPupBubble>();

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.GetAnimationState();
			frameSpeed = (state == GroundAnimationState.WALKING) ? 5 : 10;
			base.Animate(minFrame, maxFrame);
			if(state == GroundAnimationState.JUMPING)
			{
				Projectile.frame = Projectile.velocity.Y > 0 ? 2 : 5;
			} 
		}
	}
}
