using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class PuppyMinionBuff : CombatPetVanillaCloneBuff
	{
		public PuppyMinionBuff() : base(ProjectileType<PuppyMinion>()) { }
		public override int VanillaBuffId => BuffID.Puppy;
		public override string VanillaBuffName => "Puppy";
	}

	public class PuppyMinionItem : CombatPetMinionItem<PuppyMinionBuff, PuppyMinion>
	{
		internal override int VanillaItemID => ItemID.DogWhistle;
		internal override string VanillaItemName => "DogWhistle";
	}

	public class PuppyMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Puppy;
		internal override int BuffId => BuffType<PuppyMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 32, -20, -4, -1);
			ConfigureFrames(11, (0, 0), (1, 6), (2, 2), (7, 10));
		}
	}
}
