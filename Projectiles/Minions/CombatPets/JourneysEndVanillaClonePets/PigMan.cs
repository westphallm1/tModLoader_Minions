using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class PigManMinionBuff : CombatPetVanillaCloneBuff
	{
		public PigManMinionBuff() : base(ProjectileType<PigManMinion>()) { }
		public override string VanillaBuffName => "PigPet";
		public override int VanillaBuffId => BuffID.PigPet;
	}

	public class PigManMinionItem : CombatPetMinionItem<PigManMinionBuff, PigManMinion>
	{
		internal override string VanillaItemName => "PigPetItem";
		internal override int VanillaItemID => ItemID.PigPetItem;
	}

	public class PigManMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PigPet;
		internal override int BuffId => BuffType<PigManMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -16, -32);
			ConfigureFrames(12, (0, 0), (1, 9), (10, 10), (11, 11));
			frameSpeed = 8;
		}
	}
}
