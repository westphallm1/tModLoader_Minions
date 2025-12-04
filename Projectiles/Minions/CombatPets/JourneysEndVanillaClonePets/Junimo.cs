using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Items.Armor;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class JunimoMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => [ProjectileType<JunimoMinion>()];
		public override string VanillaBuffName => "JunimoPet";
		public override int VanillaBuffId => BuffID.JunimoPet;
	}

	public class JunimoMinionItem : CombatPetMinionItem<JunimoMinionBuff, JunimoMinion>
	{
		internal override string VanillaItemName => "JunimoPetItem";
		internal override int VanillaItemID => ItemID.JunimoPetItem;
	}

	public class JunimoMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.JunimoPet;
		public override int BuffId => BuffType<JunimoMinionBuff>();
		internal override int? ProjId => ProjectileType<SaplingMinionLeafProjectile>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 24, -8, -16);
			ConfigureFrames(16, (0, 3), (4, 12), (4, 4), (13, 15));
		}
	}
}
