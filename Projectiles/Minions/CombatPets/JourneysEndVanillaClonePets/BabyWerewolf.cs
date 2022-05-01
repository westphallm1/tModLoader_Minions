using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class BabyWerewolfMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BabyWerewolfMinion>() };
		public override string VanillaBuffName => "BabyWerewolf";
		public override int VanillaBuffId => BuffID.BabyWerewolf;
	}

	public class BabyWerewolfMinionItem : CombatPetMinionItem<BabyWerewolfMinionBuff, BabyWerewolfMinion>
	{
		internal override string VanillaItemName => "FullMoonSqueakyToy";
		internal override int VanillaItemID => ItemID.FullMoonSqueakyToy;
	}

	public class BabyWerewolfMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BabyWerewolf;
		internal override int BuffId => BuffType<BabyWerewolfMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -4, -12, -1);
			ConfigureFrames(24, (0, 3), (5, 17), (4, 4), (18, 23));
			frameSpeed = 8;
		}
	}
}
