using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class EyeSpringMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<EyeSpringMinion>() };
		public override int VanillaBuffId => BuffID.EyeballSpring;
		public override string VanillaBuffName => "EyeballSpring";
	}

	public class EyeSpringMinionItem : CombatPetMinionItem<EyeSpringMinionBuff, EyeSpringMinion>
	{
		internal override int VanillaItemID => ItemID.EyeSpring;
		internal override string VanillaItemName => "EyeSpring";
	}

	public class EyeSpringMinion : CombatPetSlimeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.EyeSpring;
		public override int BuffId => BuffType<EyeSpringMinionBuff>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 8;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			CombatPetConvenienceMethods.ConfigureDrawBox(this, 16, 30, 0, -26);
			forwardDir = -1;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(gHelper.isFlying) { base.Animate(6, 8); }
			else if(ShouldBounce) { base.Animate(0, 5); }
			else { Projectile.frame = 3; }

			if(gHelper.isFlying && Projectile.velocity.LengthSquared() > 2)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			} else if (!gHelper.isFlying)
			{
				Projectile.rotation = 0;
			}
		}
	}
}
