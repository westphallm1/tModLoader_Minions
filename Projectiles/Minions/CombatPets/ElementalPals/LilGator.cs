using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class LilGatorMinionBuff : CombatPetBuff
	{
		public LilGatorMinionBuff() : base(ProjectileType<LilGatorMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Lil' Gator");
			Description.SetDefault("A surfing gator has joined your adventure!");
		}
	}

	public class LilGatorMinionItem : CombatPetCustomMinionItem<LilGatorMinionBuff, LilGatorMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Jolly Bow of Friendship");
			Tooltip.SetDefault("Summons a pet Lil' Gator!");
		}
	}

	public class LilGatorMinion : WaterBeamLaserCombatPet
	{
		internal override int BuffId => BuffType<LilGatorMinionBuff>();

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(30, 30, -6, -8, -1);
			ConfigureFrames(11, (0, 1), (2, 6), (2, 2), (7, 10));
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
