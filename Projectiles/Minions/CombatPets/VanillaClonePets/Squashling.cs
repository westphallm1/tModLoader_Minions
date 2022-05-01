using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class SquashlingMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SquashlingMinion>() };
		public override string VanillaBuffName => "Squashling";
		public override int VanillaBuffId => BuffID.Squashling;
	}

	public class SquashlingMinionItem : CombatPetMinionItem<SquashlingMinionBuff, SquashlingMinion>
	{
		internal override string VanillaItemName => "MagicalPumpkinSeed";
		internal override int VanillaItemID => ItemID.MagicalPumpkinSeed;
	}

	public class SquashlingPumpkinBomb : WeakPumpkinBomb
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/PumpkinSquire/PumpkinBomb";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
		}
	}

	public class SquashlingMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Squashling;
		internal override int BuffId => BuffType<SquashlingMinionBuff>();
		internal override int? ProjId => ProjectileType<SquashlingPumpkinBomb>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -16, -12, -1);
			ConfigureFrames(13, (0, 0), (1, 6), (7, 7), (7, 12));
		}
	}
}
