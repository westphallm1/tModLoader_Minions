using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Items.Armor;
using Microsoft.Xna.Framework;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class CursedSaplingMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<CursedSaplingMinion>() };
		public override string VanillaBuffName => "CursedSapling";
		public override int VanillaBuffId => BuffID.CursedSapling;
	}

	public class CursedSaplingMinionItem : CombatPetMinionItem<CursedSaplingMinionBuff, CursedSaplingMinion>
	{
		internal override string VanillaItemName => "CursedSapling";
		internal override int VanillaItemID => ItemID.CursedSapling;
	}

	public class CursedSaplingMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CursedSapling;
		internal override int BuffId => BuffType<CursedSaplingMinionBuff>();
		internal override int? ProjId => ProjectileType<RavenGreekFire>();

		internal override Vector2 LaunchPos => Projectile.Top;
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -8, -32, -1);
			ConfigureFrames(10, (0, 0), (3, 6), (1, 1), (7, 10));
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if(gHelper.isFlying)
			{
				Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.5f);
				for(int i = 0; i < 2; i++)
				{
					int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, 0f, 0f, 100);
					Main.dust[dustId].position.X -= 2f;
					Main.dust[dustId].position.Y += 2f;
					Main.dust[dustId].scale += Main.rand.NextFloat(0.5f);
					Main.dust[dustId].noGravity = true;
					Main.dust[dustId].velocity.Y -= 2f;
				}
			}
		}
	}
}
