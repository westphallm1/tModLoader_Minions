using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class BlueChickenMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => [ ProjectileType<BlueChickenMinion>() ];
		public override string VanillaBuffName => "BlueChickenPet";
		public override int VanillaBuffId => BuffID.BlueChickenPet;
	}

	public class BlueChickenMinionItem : CombatPetMinionItem<BlueChickenMinionBuff, BlueChickenMinion>
	{
		internal override string VanillaItemName => "BlueEgg";
		internal override int VanillaItemID => ItemID.BlueEgg;
	}

	public class BlueChickenEggProjectile : WeakPumpkinBomb
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RottenEgg;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			bounces = 1;
		}

		public override void OnKill(int timeLeft)
		{
			// TODO dust
			for(int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.position, 16, 16, DustID.Marble);
			}
		}
	}

	public class BlueChickenMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BlueChickenPet;
		public override int BuffId => BuffType<BlueChickenMinionBuff>();
		internal override int? ProjId => ProjectileType<BlueChickenEggProjectile>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(28, 30, -4, -6, forwardDir: -1);
			ConfigureFrames(10, (0, 0), (1, 4), (1, 1), (6, 9));
		}
	}
}
