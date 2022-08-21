using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.CrossModSystem.SampleMod.Pets.SampleFlyingPet
{
	// Code largely adapted from tModLoader Sample Mod
	internal class SampleFlyingPetProjectile : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ZephyrFish;
		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = Main.projFrames[ProjectileID.ZephyrFish];
			Main.projPet[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.ZephyrFish);
			AIType = ProjectileID.ZephyrFish;
		}

		public override bool PreAI()
		{
			// unset default buff
			Main.player[Projectile.owner].zephyrfish = false;
			return true;
		}

		public override void AI()
		{
			if(Main.player[Projectile.owner].HasBuff(BuffType<SampleFlyingPetBuff>()))
			{
				Projectile.timeLeft = 2;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// make it red to distinguish from vanilla
			lightColor = Color.Red.MultiplyRGB(lightColor * 1.5f);
			return true;
		}

	}
}
