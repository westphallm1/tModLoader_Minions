using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class MiniMinotaurMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<MiniMinotaurMinion>() };
		public override string VanillaBuffName => "MiniMinotaur";
		public override int VanillaBuffId => BuffID.MiniMinotaur;
	}

	public class MiniMinotaurMinionItem : CombatPetMinionItem<MiniMinotaurMinionBuff, MiniMinotaurMinion>
	{
		internal override string VanillaItemName => "TartarSauce";
		internal override int VanillaItemID => ItemID.TartarSauce;
	}

	public class MiniMinotaurAxeProjectile : WeakPumpkinBomb
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.LeadAxe;
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

		public override void Kill(int timeLeft)
		{
			// TODO dust
			for(int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.position, 16, 16, DustID.Lead);
			}
		}
	}

	public class MiniMinotaurMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MiniMinotaur;
		public override int BuffId => BuffType<MiniMinotaurMinionBuff>();
		internal override int? ProjId => ProjectileType<MiniMinotaurAxeProjectile>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -16, -16);
			ConfigureFrames(10, (0, 0), (1, 4), (1, 1), (6, 9));
		}
	}
}
