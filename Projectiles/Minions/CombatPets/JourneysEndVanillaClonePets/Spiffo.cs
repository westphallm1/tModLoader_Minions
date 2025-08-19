using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class SpiffoMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => [ProjectileType<SpiffoMinion>()];
		public override string VanillaBuffName => "Spiffo";
		public override int VanillaBuffId => BuffID.Spiffo;
	}

	public class SpiffoMinionItem : CombatPetMinionItem<SpiffoMinionBuff, SpiffoMinion>
	{
		internal override string VanillaItemName => "SpiffoPlush";
		internal override int VanillaItemID => ItemID.SpiffoPlush;
	}

	public class SpiffoMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Spiffo;
		public override int BuffId => BuffType<SpiffoMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -18, -18, -1);
			ConfigureFrames(16, (0, 0), (2, 11), (1, 1), (12, 15));
		}
	}
}
