using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Items.Armor;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class TruffleTurtleMinionBuff : CombatPetVanillaCloneBuff
	{
		public TruffleTurtleMinionBuff() : base(ProjectileType<TruffleTurtleMinion>()) { }
		public override string VanillaBuffName => "BabyWerewolf";
		public override int VanillaBuffId => BuffID.BabyWerewolf;
	}

	public class TruffleTurtleMinionItem : CombatPetMinionItem<TruffleTurtleMinionBuff, TruffleTurtleMinion>
	{
		internal override string VanillaItemName => "FullMoonSqueakyToy";
		internal override int VanillaItemID => ItemID.FullMoonSqueakyToy;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Truffle Turtle");
		}
	}

	public class TruffleTurtleMinion : CombatPetGroundedRangedMinion
	{
		internal override int BuffId => BuffType<TruffleTurtleMinionBuff>();

		internal override bool ShouldDoShootingMovement => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Skeletal;

		internal override int? ProjId => ProjectileType<ForagerMushroom>();

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(30, 30, -6, -8, -1);
			ConfigureFrames(8, (0, 1), (2, 6), (2, 2), (7, 7));
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.GetAnimationState();
			frameSpeed = (state == GroundAnimationState.WALKING) ? 5 : 10;
			base.Animate(minFrame, maxFrame);
			if(state == GroundAnimationState.JUMPING)
			{
				Projectile.frame = Projectile.velocity.Y > 0 ? 2 : 5;
			} 
		}
	}
}
