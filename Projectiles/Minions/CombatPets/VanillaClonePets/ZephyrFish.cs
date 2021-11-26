using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class ZephyrFishMinionBuff : CombatPetVanillaCloneBuff
	{
		public ZephyrFishMinionBuff() : base(ProjectileType<ZephyrFishMinion>()) { }
		public override string VanillaBuffName => "ZephyrFish";
		public override int VanillaBuffId => BuffID.ZephyrFish;
	}

	public class ZephyrFishMinionItem : CombatPetMinionItem<ZephyrFishMinionBuff, ZephyrFishMinion>
	{
		internal override string VanillaItemName => "ZephyrFish";
		internal override int VanillaItemID => ItemID.ZephyrFish;
	}

	public class ZephyrFishMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<ZephyrFishMinionBuff>();
		public override string Texture => "Terraria/Projectile_" + ProjectileID.ZephyrFish;
		internal override int? FiredProjectileId => null;
		internal override bool DoBumblingMovement => true;
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
