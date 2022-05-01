using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BabyHornetMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BabyHornetMinion>() };
		public override string VanillaBuffName => "BabyHornet";
		public override int VanillaBuffId => BuffID.BabyHornet;
	}

	public class BabyHornetMinionItem : CombatPetMinionItem<BabyHornetMinionBuff, BabyHornetMinion>
	{
		internal override string VanillaItemName => "Nectar";
		internal override int VanillaItemID => ItemID.Nectar;
	}

	public class BabyHornetMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<BabyHornetMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BabyHornet;
		internal override int? FiredProjectileId => ProjectileType<HornetStinger>();
		internal override LegacySoundStyle ShootSound => SoundID.Item17;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 4;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			forwardDir = -1;
		}

	}
}
