using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BabyFaceMonsterMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BabyFaceMonsterMinion>() };
		public override int VanillaBuffId => BuffID.BabyFaceMonster;
		public override string VanillaBuffName => "BabyFaceMonster";
	}

	public class BabyFaceMonsterMinionItem : CombatPetMinionItem<BabyFaceMonsterMinionBuff, BabyFaceMonsterMinion>
	{
		internal override int VanillaItemID => ItemID.BoneRattle;
		internal override string VanillaItemName => "BoneRattle";
	}

	public class BabyFaceMonsterMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BabyFaceMonster;
		internal override int BuffId => BuffType<BabyFaceMonsterMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 32, -16, -16, -1);
			ConfigureFrames(12, (0, 0), (2, 7), (1, 1), (8, 12));
		}
	}
}
