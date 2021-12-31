using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class CloudiphantMinionBuff : CombatPetBuff
	{
		public CloudiphantMinionBuff() : base(ProjectileType<CloudiphantMinion>()) { }
	}

	public class CloudiphantMinionItem : CombatPetMinionItem<CloudiphantMinionBuff, CloudiphantMinion>
	{
		internal override string VanillaItemName => "FullMoonSqueakyToy";
		internal override int VanillaItemID => ItemID.FullMoonSqueakyToy;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Cloudiphant");
		}
	}

	public class CloudiphantMinion : CombatPetGroundedRangedMinion
	{
		internal override int BuffId => BuffType<CloudiphantMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(30, 24, -6, -10, -1);
			ConfigureFrames(9, (0, 0), (1, 5), (1, 1), (6, 8));
		}

		internal override bool ShouldDoShootingMovement => false; //  leveledPetPlayer.PetLevel >= (int)CombatPetTier.Skeletal;

		internal override int? ProjId => null; // ProjectileType<SharkPupBubble>();

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
