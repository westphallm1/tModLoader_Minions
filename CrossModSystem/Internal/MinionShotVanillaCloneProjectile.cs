using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.CrossModSystem.Internal
{
	internal abstract class MinionShotVanillaCloneProjectile : ModProjectile
	{

		public override string Texture => "Terraria/Images/Projectile_" + VanillaProjID;

		internal abstract int VanillaProjID { get; }

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Type] = true;
			Main.projFrames[Type] = Main.projFrames[VanillaProjID];
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(VanillaProjID);
			AIType = VanillaProjID;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Summon;
		}

	}
}
