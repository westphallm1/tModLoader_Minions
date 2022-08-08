using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class PropellerGatoMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<PropellerGatoMinion>() };
		public override string VanillaBuffName => "PetDD2Gato";
		public override int VanillaBuffId => BuffID.PetDD2Gato;
	}

	public class PropellerGatoMinionItem : CombatPetMinionItem<PropellerGatoMinionBuff, PropellerGatoMinion>
	{
		internal override string VanillaItemName => "DD2PetGato";
		internal override int VanillaItemID => ItemID.DD2PetGato;
	}

	public class PropellerGatoMinion : CombatPetHoverShooterMinion
	{
		public override int BuffId => BuffType<PropellerGatoMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2PetGato;
		internal override int? FiredProjectileId => null;
		internal override bool DoBumblingMovement => true;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
	}
}
