using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Items.Accessories.SquireSkull;
using AmuletOfManyMinions.Items.Armor.RoyalArmor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires
{
	public class SquireModPlayer : ModPlayer
	{
		public bool squireSkullAccessory;
		public int squireDebuffOnHit = -1;
		public float squireTravelSpeedMultiplier;
		public float squireRangeMultiplier;
		public float squireAttackSpeedMultiplier;
		public float squireDamageMultiplierBonus;
		internal int squireDebuffTime;
		internal bool royalArmorSetEquipped;

		public override void ResetEffects()
		{
			squireSkullAccessory = false;
			royalArmorSetEquipped = false;
			squireAttackSpeedMultiplier = 1;
			squireTravelSpeedMultiplier = 1;
			squireRangeMultiplier = 1;
			squireDamageMultiplierBonus = 0;
		}

		public bool HasSquire()
		{
			foreach (int squireType in SquireMinionTypes.squireTypes)
			{
				if(player.ownedProjectileCounts[squireType] > 0)
				{
					return true;
				}
			}
			return false;
		}

		public Projectile GetSquire()
		{
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == player.whoAmI && SquireMinionTypes.Contains(p.type)) {
					return p;
				}
			}
			return null;
		}

		public override void ModifyWeaponDamage(Item item, ref float add, ref float mult, ref float flat)
		{
			base.ModifyWeaponDamage(item, ref add, ref mult, ref flat);
			if(!SquireMinionTypes.Contains(item.shoot))
			{
				return;
			}
			mult += squireDamageMultiplierBonus;
		}
		public override void PostUpdate()
		{
			Projectile mySquire = GetSquire();
			int skullType = ProjectileType<SquireSkullProjectile>();
			int crownType = ProjectileType<RoyalCrownProjectile>();
			bool canSummonAccessory = player.whoAmI == Main.myPlayer && mySquire != null;
			if(canSummonAccessory && squireSkullAccessory && player.ownedProjectileCounts[skullType] == 0)
			{
				Projectile.NewProjectile(mySquire.Center, mySquire.velocity, skullType, 0, 0, player.whoAmI);
			}
			if(canSummonAccessory && royalArmorSetEquipped && player.ownedProjectileCounts[crownType] == 0)
			{
				Projectile.NewProjectile(mySquire.Center, mySquire.velocity, crownType, 0, 0, player.whoAmI);
			}
			if(player.ownedProjectileCounts[skullType] == 0)
			{
				squireDebuffOnHit = -1;
			}
		}
	}

	class SquireGlobalProjectile: GlobalProjectile
	{
		public static HashSet<int> isSquireShot;

		public static void Load()
		{
			isSquireShot = new HashSet<int>();
		}

		public static void Unload()
		{
			isSquireShot = null;
		}

		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
		{
			if(!SquireMinionTypes.Contains(projectile.type) && !isSquireShot.Contains(projectile.type))
			{
				return;
			}
			SquireModPlayer player = Main.player[projectile.owner].GetModPlayer<SquireModPlayer>();
			int debuffType = player.squireDebuffOnHit;
			int duration = player.squireDebuffTime;
			if(debuffType == -1 || Main.rand.NextFloat() > 0.25f)
			{
				return;
			}
			target.AddBuff(debuffType, duration);
		}
	}
}
