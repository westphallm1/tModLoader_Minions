using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class FennecFoxMinionBuff : CombatPetVanillaCloneBuff
	{
		public FennecFoxMinionBuff() : base(ProjectileType<FennecFoxMinion>()) { }
		public override string VanillaBuffName => "FennecFox";
		public override int VanillaBuffId => BuffID.FennecFox;
	}

	public class FennecFoxMinionItem : CombatPetMinionItem<FennecFoxMinionBuff, FennecFoxMinion>
	{
		internal override string VanillaItemName => "ExoticEasternChewToy";
		internal override int VanillaItemID => ItemID.ExoticEasternChewToy;
	}

	public class FennecFoxMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FennecFox;
		internal override int BuffId => BuffType<FennecFoxMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -16, -8, -1);
			ConfigureFrames(17, (0, 3), (4, 10), (4, 4), (11, 16));
			frameSpeed = 8;
		}
	}
}
