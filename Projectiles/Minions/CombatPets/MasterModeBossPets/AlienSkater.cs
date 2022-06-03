using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class AlienSkaterMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<AlienSkaterMinion>() };
		public override int VanillaBuffId => BuffID.MartianPet;
		public override string VanillaBuffName => "MartianPet";
	}

	public class AlienSkaterMinionItem : CombatPetMinionItem<AlienSkaterMinionBuff, AlienSkaterMinion>
	{
		internal override int VanillaItemID => ItemID.MartianPetItem;
		internal override string VanillaItemName => "MartianPetItem";
	}

	public class AlienSkaterMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MartianPet;
		internal override int BuffId => BuffType<AlienSkaterMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(32, 32, 0, -16);
			ConfigureFrames(14, (0, 0), (2, 9), (1, 1), (10, 14));
		}
	}
}
