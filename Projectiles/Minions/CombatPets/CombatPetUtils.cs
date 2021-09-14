using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets
{
	class CombatPetUtils
	{
		// used for spawning combat pets from the pet buff
		public static void SpawnIfAbsent(Player player, int buffIdx, int projType, int damage, float knockback = 0.5f)
		{
			if(player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0)
			{
				var p = Projectile.NewProjectileDirect(player.GetProjectileSource_Buff(buffIdx), player.Center, Vector2.Zero, projType, damage, knockback, player.whoAmI);
				p.originalDamage = damage;
			}
		}
	}

	public abstract class CombatPetMinionItem<TBuff, TProj> : VanillaCloneMinionItem<TBuff, TProj> where TBuff: ModBuff where TProj: Minion
	{
		public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ApplyBuff(player);
			return false;
		}
	}
}
