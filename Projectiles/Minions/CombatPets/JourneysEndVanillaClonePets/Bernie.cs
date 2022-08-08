using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class BernieMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BernieMinion>() };
		public override string VanillaBuffName => "BerniePet";
		public override int VanillaBuffId => BuffID.BerniePet;
	}

	public class BernieMinionItem : CombatPetMinionItem<BernieMinionBuff, BernieMinion>
	{
		internal override string VanillaItemName => "BerniePetItem";
		internal override int VanillaItemID => ItemID.BerniePetItem;
	}

	public class BernieMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BerniePet;
		public override int BuffId => BuffType<BernieMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -12, -16);
			ConfigureFrames(11, (0, 0), (1, 8), (9, 9), (10, 10));
			FrameSpeed = 8;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			DoSimpleFlyingDust();
		}
	}
}
