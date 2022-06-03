using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class BabyOgreMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BabyOgreMinion>() };
		public override int VanillaBuffId => BuffID.DD2OgrePet;
		public override string VanillaBuffName => "DD2OgrePet";
	}

	public class BabyOgreMinionItem : CombatPetMinionItem<BabyOgreMinionBuff, BabyOgreMinion>
	{
		internal override int VanillaItemID => ItemID.DD2OgrePetItem;
		internal override string VanillaItemName => "DD2OgrePetItem";
	}

	public class BabyOgreMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2OgrePet;
		internal override int BuffId => BuffType<BabyOgreMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(32, 32, -32, -42);
			ConfigureFrames(14, (0, 0), (2, 9), (1, 1), (10, 14));
		}
	}
}
