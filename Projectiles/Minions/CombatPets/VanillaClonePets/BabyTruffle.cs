using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Items.Armor;
using Microsoft.Xna.Framework;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BabyTruffleMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BabyTruffleMinion>() };
		public override string VanillaBuffName => "BabyTruffle";
		public override int VanillaBuffId => BuffID.BabyTruffle;
	}

	public class BabyTruffleMinionItem : CombatPetMinionItem<BabyTruffleMinionBuff, BabyTruffleMinion>
	{
		internal override string VanillaItemName => "StrangeGlowingMushroom";
		internal override int VanillaItemID => ItemID.StrangeGlowingMushroom;
	}

	public class TrufflePetGlowingMushroom : BaseTrackingMushroom
	{
		public override string Texture => "Terraria/Images/NPC_" + NPCID.FungiSpore;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			Lighting.AddLight(Projectile.Center, Color.DeepSkyBlue.ToVector3() * 0.5f);
		}
		public override void Kill(int timeLeft)
		{
			for(int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.position, 16, 16, DustID.GlowingMushroom);
			}
		}
	}

	public class BabyTruffleMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Truffle;
		internal override int BuffId => BuffType<BabyTruffleMinionBuff>();
		internal override int? ProjId => ProjectileType<TrufflePetGlowingMushroom>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, 0, -8, -1);
			ConfigureFrames(12, (0, 0), (1, 11), (1, 1), (1, 1));
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			DoSimpleFlyingDust();
		}

		public override void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			Vector2 launchVel = new Vector2(0.25f * launchVector.X, -Main.rand.Next(8, 12));
			base.LaunchProjectile(launchVel);
		}
	}
}
