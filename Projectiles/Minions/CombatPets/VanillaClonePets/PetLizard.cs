using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class PetLizardMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<PetLizardMinion>() };
		public override int VanillaBuffId => BuffID.PetLizard;
		public override string VanillaBuffName => "PetLizard";
	}

	public class PetLizardMinionItem : CombatPetMinionItem<PetLizardMinionBuff, PetLizardMinion>
	{
		internal override int VanillaItemID => ItemID.LizardEgg;
		internal override string VanillaItemName => "LizardEgg";
	}

	public class PetLizardMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PetLizard;
		public override int BuffId => BuffType<PetLizardMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 16, -8, -24, -1);
			ConfigureFrames(10, (0, 0), (0, 6), (3, 3), (6, 9));
		}
	}
}
