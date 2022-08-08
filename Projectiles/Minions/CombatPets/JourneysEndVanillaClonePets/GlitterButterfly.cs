using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class GlitteryButterflyMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<GlitteryButterflyMinion>() };
		public override string VanillaBuffName => "GlitterButterfly";
		public override int VanillaBuffId => BuffID.GlitteryButterfly;
	}

	public class GlitteryButterflyMinionItem : CombatPetMinionItem<GlitteryButterflyMinionBuff, GlitteryButterflyMinion>
	{
		internal override string VanillaItemName => "BedazzledNectar";
		internal override int VanillaItemID => ItemID.BedazzledNectar;
	}

	public class GlitteryButterflyMinion : CombatPetHoverShooterMinion
	{
		public override int BuffId => BuffType<GlitteryButterflyMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GlitteryButterfly;
		internal override int? FiredProjectileId => null;
		internal override bool DoBumblingMovement => true;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 18;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			forwardDir = -1;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			// TODO something with the unused phase in/out frames
			base.Animate(0, 3);
		}
	}
}
