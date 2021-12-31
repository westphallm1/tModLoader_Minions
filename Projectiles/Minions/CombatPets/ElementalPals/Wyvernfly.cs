using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class WyvernFlyMinionBuff : CombatPetBuff
	{
		public WyvernFlyMinionBuff() : base(ProjectileType<WyvernFlyMinion>()) { }
	}

	public class WyvernFlyMinionItem : CombatPetMinionItem<WyvernFlyMinionBuff, WyvernFlyMinion>
	{
		internal override string VanillaItemName => "FullMoonSqueakyToy";
		internal override int VanillaItemID => ItemID.FullMoonSqueakyToy;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("WyvernFly");
		}
	}

	public class WyvernFlyMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<WyvernFlyMinionBuff>();
		internal override int? FiredProjectileId => null;
		internal override bool DoBumblingMovement => true;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 6;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			forwardDir = -1;
			DrawOriginOffsetX = -16;
		}
	}
}
