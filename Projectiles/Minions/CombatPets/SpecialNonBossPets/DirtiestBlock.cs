using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets
{
	public class DirtiestBlockMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => [ProjectileType<DirtiestBlockMinion>()];
		public override int VanillaBuffId => BuffID.DirtiestBlock;
		public override string VanillaBuffName => "DirtiestBlock";
	}

	public class DirtiestBlockMinionItem : CombatPetMinionItem<DirtiestBlockMinionBuff, DirtiestBlockMinion>
	{
		internal override int VanillaItemID => ItemID.DirtiestBlock;
		internal override string VanillaItemName => "DirtiestBlock";
	}

	public class DirtiestBlockMinion : CombatPetSlimeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DirtiestBlock;
		public override int BuffId => BuffType<DirtiestBlockMinionBuff>();

		// Potentially extraneous animation state variables
		// Track which side the dirt should "fall" to when it stops spinning
		private float? targetAngle = null;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			CombatPetConvenienceMethods.ConfigureDrawBox(this, 16, 16, 0, 0);
			forwardDir = -1;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (Math.Abs(Projectile.velocity.X) >= 1f)
			{
				Projectile.rotation += 0.05f * Projectile.velocity.X;
				targetAngle = null;
			}
			else if (targetAngle is float ta)
			{
				Projectile.rotation = MathHelper.Lerp(Projectile.rotation, ta, 0.2f);
			}
			else
			{
				var prevFlatAngle = Projectile.rotation - (Projectile.rotation % (MathF.PI / 2));
				var nextFlatAngle = prevFlatAngle + MathF.PI / 2;
				targetAngle = Projectile.velocity.X > 0 ? nextFlatAngle : prevFlatAngle;
			}

			if((GHelper.isFlying && AnimationFrame % 15 == 0))
			{
				for(int _ = 0; _ < 4; _++)
				{
					Dust.NewDust(Projectile.TopLeft, 16, 16, DustID.Dirt);
				}
			}
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{
			if(ShouldBounce)
			{
				for(int _ = 0; _ < 4; _++)
				{
					Dust.NewDust(Projectile.TopLeft, 16, 16, DustID.Dirt);
				}
			}
			base.DoGroundedMovement(vector);
		}
	}
}
