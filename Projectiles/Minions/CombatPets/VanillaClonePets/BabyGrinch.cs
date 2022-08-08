using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BabyGrinchMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BabyGrinchMinion>() };
		public override string VanillaBuffName => "BabyGrinch";
		public override int VanillaBuffId => BuffID.BabyGrinch;
	}

	public class BabyGrinchMinionItem : CombatPetMinionItem<BabyGrinchMinionBuff, BabyGrinchMinion>
	{
		internal override string VanillaItemName => "BabyGrinchMischiefWhistle";
		internal override int VanillaItemID => ItemID.BabyGrinchMischiefWhistle;
	}

	public class BabyGrinchMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BabyGrinch;
		public override int BuffId => BuffType<BabyGrinchMinionBuff>();
		internal override int? ProjId => ProjectileType<EverscreamSaplingOrnament>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(16, 32, -16, -32, -1);
			ConfigureFrames(14, (0, 0), (2, 9), (1, 1), (10, 14));
		}
	}
}
