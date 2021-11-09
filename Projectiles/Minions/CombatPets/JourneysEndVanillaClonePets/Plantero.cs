using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using Microsoft.Xna.Framework;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class PlanteroMinionBuff : CombatPetVanillaCloneBuff
	{
		public PlanteroMinionBuff() : base(ProjectileType<PlanteroMinion>()) { }
		public override string VanillaBuffName => "Plantero";
		public override int VanillaBuffId => BuffID.Plantero;
	}

	public class PlanteroMinionItem : CombatPetMinionItem<PlanteroMinionBuff, PlanteroMinion>
	{
		internal override string VanillaItemName => "MudBud";
		internal override int VanillaItemID => ItemID.MudBud;
	}


	public class PlanteroMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Plantero;
		internal override int BuffId => BuffType<PlanteroMinionBuff>();
		private bool wasFlyingThisFrame =  false;
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 24, -8, -18, -1);
			ConfigureFrames(19, (0, 3), (4, 12), (12, 12), (13, 18));
			frameSpeed = 8;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if(gHelper.isFlying && Projectile.velocity.LengthSquared() > 2)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			} else 
			{
				Projectile.rotation = Utils.AngleLerp(Projectile.rotation, 0, 0.25f);
			}
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			if(!wasFlyingThisFrame && gHelper.isFlying)
			{
				Gore.NewGore(Projectile.Center, Vector2.Zero, GoreID.PlanteroSombrero);
			}
			wasFlyingThisFrame = gHelper.isFlying;
		}
	}
}
