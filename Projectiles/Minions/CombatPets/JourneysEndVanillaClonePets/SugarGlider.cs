using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class SugarGliderMinionBuff : CombatPetVanillaCloneBuff
	{
		public SugarGliderMinionBuff() : base(ProjectileType<SugarGliderMinion>()) { }
		public override string VanillaBuffName => "SugarGlider";
		public override int VanillaBuffId => BuffID.SugarGlider;
	}

	public class SugarGliderMinionItem : CombatPetMinionItem<SugarGliderMinionBuff, SugarGliderMinion>
	{
		internal override string VanillaItemName => "EucaluptusSap";
		internal override int VanillaItemID => ItemID.EucaluptusSap;
	}

	public class SugarGliderMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SugarGlider;
		internal override int BuffId => BuffType<SugarGliderMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 20, -4, -20, -1);
			ConfigureFrames(10, (0, 0), (0, 5), (6, 6), (7, 9));
		}
	}
}
