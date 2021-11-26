using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BabyDinosaurMinionBuff : CombatPetVanillaCloneBuff
	{
		public BabyDinosaurMinionBuff() : base(ProjectileType<BabyDinosaurMinion>()) { }
		public override int VanillaBuffId => BuffID.BabyDinosaur;
		public override string VanillaBuffName => "BabyDinosaur";
	}

	public class BabyDinosaurMinionItem : CombatPetMinionItem<BabyDinosaurMinionBuff, BabyDinosaurMinion>
	{
		internal override int VanillaItemID => ItemID.AmberMosquito;
		internal override string VanillaItemName => "AmberMosquito";
	}

	public class BabyDinosaurMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.BabyDino;
		internal override int BuffId => BuffType<BabyDinosaurMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 32, -20, -32, -1);
			ConfigureFrames(13, (0, 0), (2, 8), (1, 1), (9, 13));
		}
	}
}
