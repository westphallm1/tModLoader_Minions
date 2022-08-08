using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class ShadowMimicMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<ShadowMimicMinion>() };
		public override int VanillaBuffId => BuffID.ShadowMimic;
		public override string VanillaBuffName => "ShadowMimic";
	}

	public class ShadowMimicMinionItem : CombatPetMinionItem<ShadowMimicMinionBuff, ShadowMimicMinion>
	{
		internal override int VanillaItemID => ItemID.OrnateShadowKey;
		internal override string VanillaItemName => "OrnateShadowKey";
	}

	public class ShadowMimicMinion : CombatPetSlimeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ShadowMimic;
		public override int BuffId => BuffType<ShadowMimicMinionBuff>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 14;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			CombatPetConvenienceMethods.ConfigureDrawBox(this, 30, 30, 0, -24);
			forwardDir = -1;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			int idleCycle = AnimationFrame % 180;
			if(gHelper.isFlying) { base.Animate(6, 14); }
			else if(ShouldBounce) { base.Animate(4, 6); }
			else if (idleCycle < 150) 
			{
				Projectile.frame = 0; 
			} else if (idleCycle < 165)
			{
				Projectile.frame = 1 + (idleCycle - 150) / 5;
			} else
			{
				if(idleCycle == 165)
				{
					var source = Projectile.GetSource_Death();
					for(int i = 0; i < 3; i++)
					{
						Gore.NewGore(source, Projectile.Center, new Vector2(3 * forwardDir * Projectile.spriteDirection, -1), GoreID.ShadowMimicCoins);
					}
				}
				Projectile.frame = 3 -  (idleCycle - 165) / 5;

			}

			if(gHelper.isFlying && Projectile.velocity.LengthSquared() > 2)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			} else if (!gHelper.isFlying)
			{
				Projectile.rotation = 0;
			}

			if(gHelper.isFlying)
			{
				Vector2 offsetVector = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * 8;
				Vector2 spawnVector = Projectile.Center + offsetVector;
				int idx = Dust.NewDust(spawnVector, 16, 16, DustID.Shadowflame, offsetVector.X, offsetVector.Y);
				Main.dust[idx].scale *= 0.75f;
				Main.dust[idx].alpha = 112;
			}
		}
	}
}
