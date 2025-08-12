using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class CavelingGardenerMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => [ProjectileType<CavelingGardenerMinion>()];
		public override string VanillaBuffName => "CavelingGardener";
		public override int VanillaBuffId => BuffID.CavelingGardener;
	}

	public class CavelingGardenerMinionItem : CombatPetMinionItem<CavelingGardenerMinionBuff, CavelingGardenerMinion>
	{
		internal override string VanillaItemName => "GlowTulip";
		internal override int VanillaItemID => ItemID.GlowTulip;
	}

	public class CavelingGardenerMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CavelingGardener;
		public override int BuffId => BuffType<CavelingGardenerMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -18, -22, -1);
			ConfigureFrames(15, (0, 0), (2, 9), (1, 1), (10, 14));
		}
	}
}
