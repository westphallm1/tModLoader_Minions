using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Items.Accessories.SquireBat;
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
		public float squireRangeFlatBonus;
		public float squireAttackSpeedMultiplier;
		public float squireDamageMultiplierBonus;
		internal int squireDebuffTime;
		internal bool royalArmorSetEquipped;
		internal bool squireBatAccessory;

		// shouldn't be hand-rolling key press detection but here we are
		private bool didReleaseTap;
		private bool didDoubleTap;

		public override void ResetEffects()
		{
			squireSkullAccessory = false;
			squireBatAccessory = false;
			royalArmorSetEquipped = false;
			squireAttackSpeedMultiplier = 1;
			squireTravelSpeedMultiplier = 1;
			squireRangeFlatBonus = 0;
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

		private void MyDidDoubleTap()
		{
			int tapDirection = Main.ReversedUpDownArmorSetBonuses ? 1 : 0;
			bool tappedRecently = player.doubleTapCardinalTimer[tapDirection] > 0;
			bool didReleaseTapThisFrame = tapDirection == 0 ?
				player.releaseDown:
				player.releaseUp;
			bool didTapThisFrame = tapDirection == 0 ?
				player.controlDown:
				player.controlUp;
			didDoubleTap = false;
			if(!tappedRecently)
			{
				didReleaseTap = false;
			} else if (didReleaseTapThisFrame && tappedRecently)
			{
				didReleaseTap = true;
			} else if(tappedRecently && didReleaseTap && didTapThisFrame)
			{
				didDoubleTap = true;
				didReleaseTap = false;
			}
		}

		public override void PreUpdate()
		{
			MyDidDoubleTap();
		}

		private void SummonSquireSubMinions()
		{
			Projectile mySquire = GetSquire();
			int skullType = ProjectileType<SquireSkullProjectile>();
			int crownType = ProjectileType<RoyalCrownProjectile>();
			int batType = ProjectileType<SquireBatProjectile>();
			bool canSummonAccessory = player.whoAmI == Main.myPlayer && mySquire != null;
			// summon the appropriate squire orbiter(s)
			if(canSummonAccessory && squireSkullAccessory && player.ownedProjectileCounts[skullType] == 0)
			{
				Projectile.NewProjectile(mySquire.Center, mySquire.velocity, skullType, 0, 0, player.whoAmI);
			}
			if(canSummonAccessory && royalArmorSetEquipped && player.ownedProjectileCounts[crownType] == 0)
			{
				Projectile.NewProjectile(mySquire.Center, mySquire.velocity, crownType, 0, 0, player.whoAmI);
			}
			if(canSummonAccessory && squireBatAccessory && player.ownedProjectileCounts[batType] == 0)
			{
				Projectile.NewProjectile(mySquire.Center, mySquire.velocity, batType, 0, 0, player.whoAmI);
			}

		}

		public override void PostUpdate()
		{

			SummonSquireSubMinions();
			// apply bat buff if set bonus active
			int buffType = BuffType<SquireBatBuff>();
			int debuffType = BuffType<SquireBatDebuff>();
			if(squireBatAccessory && didDoubleTap && !player.HasBuff(buffType) && !player.HasBuff(debuffType))
			{
				player.AddBuff(buffType, SquireBatAccessory.BuffTime);
			}
			// undo buff from skill orbiter
			if(player.ownedProjectileCounts[ProjectileType<SquireSkullProjectile>()] == 0)
			{
				squireDebuffOnHit = -1;
			}
		}

		public override void PostUpdateBuffs()
		{
			int buffType = BuffType<SquireBatBuff>();
			int debuffType = BuffType<SquireBatDebuff>();
			if(player.HasBuff(buffType))
			{
				squireAttackSpeedMultiplier *= 0.75f; // 25% attack speed bonus
				squireTravelSpeedMultiplier += 0.25f; // 25% move speed bonus
				if(player.buffTime[player.FindBuffIndex(buffType)] == 1)
				{
					// switch from buff to debuff
					player.AddBuff(debuffType, SquireBatAccessory.DebuffTime);
				}
			} else if (player.HasBuff(debuffType))
			{
				squireTravelSpeedMultiplier -= 0.2f; // reduce move speed 20%
				squireAttackSpeedMultiplier *= 1.2f; // 20% attack speed decrease
				player.minionDamageMult -= 0.15f; // reduce damage 15%
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
