using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses
{
	class CombatPetConvenienceMethods
	{
		public static void ConfigureDrawBox(ModProjectile proj, int width, int height, int xOffset, int yOffset)
		{
			proj.Projectile.width = width;
			proj.Projectile.height = height;
			proj.DrawOffsetX = xOffset; 
			proj.DrawOriginOffsetY = yOffset;
		}
	}
}
