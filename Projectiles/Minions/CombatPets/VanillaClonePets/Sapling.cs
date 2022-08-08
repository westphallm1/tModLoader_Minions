using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Items.Armor;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class SaplingMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SaplingMinion>() };
		public override string VanillaBuffName => "PetSapling";
		public override int VanillaBuffId => BuffID.PetSapling;
	}

	public class SaplingMinionItem : CombatPetMinionItem<SaplingMinionBuff, SaplingMinion>
	{
		internal override string VanillaItemName => "Seedling";
		internal override int VanillaItemID => ItemID.Seedling;
	}

	public class SaplingMinionLeafProjectile : BaseTrackingMushroom
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Leaf;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 5;
		}
		public override void Kill(int timeLeft)
		{
			// TODO dust
			for(int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.position, 16, 16, DustID.Grass);
			}
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			Projectile.rotation = Projectile.velocity.ToRotation();
		}
	}

	public class SaplingMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Sapling;
		public override int BuffId => BuffType<SaplingMinionBuff>();
		internal override int? ProjId => ProjectileType<SaplingMinionLeafProjectile>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 24, -16, -8, -1);
			ConfigureFrames(12, (0, 0), (1, 6), (4, 4), (7, 11));
		}
	}
}
