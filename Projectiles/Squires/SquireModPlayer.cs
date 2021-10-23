using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Items.Accessories.SquireBat;
using AmuletOfManyMinions.Items.Accessories.SquireSkull;
using AmuletOfManyMinions.Items.Accessories.TechnoCharm;
using AmuletOfManyMinions.Items.Armor.AridArmor;
using AmuletOfManyMinions.Items.Armor.RoyalArmor;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
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
		public float squireDamageOnHitMultiplier;
		internal int squireDebuffTime;
		internal bool royalArmorSetEquipped;
		internal bool squireBatAccessory;
		internal bool aridArmorSetEquipped;
		internal bool hardmodeOreSquireArmorSetEquipped;
		internal bool spookyArmorSetEquipped;
		internal bool squireTechnoSkullAccessory;
		internal bool graniteArmorEquipped;
		internal float usedMinionSlots;

		// shouldn't be hand-rolling key press detection but here we are
		private bool didReleaseTap;
		private bool didDoubleTap;
		

		public override void ResetEffects()
		{
			squireSkullAccessory = false;
			squireTechnoSkullAccessory = false;
			squireBatAccessory = false;
			royalArmorSetEquipped = false;
			aridArmorSetEquipped = false;
			graniteArmorEquipped = false;
			hardmodeOreSquireArmorSetEquipped = false;
			spookyArmorSetEquipped = false;
			squireAttackSpeedMultiplier = 1;
			squireTravelSpeedMultiplier = 1;
			squireDamageOnHitMultiplier = 1;
			squireRangeFlatBonus = 0;
			squireDamageMultiplierBonus = 0;
		}

		public bool HasSquire()
		{
			foreach (int squireType in SquireMinionTypes.squireTypes)
			{
				if (Player.ownedProjectileCounts[squireType] > 0)
				{
					return true;
				}
			}
			return false;
		}

		public Projectile GetSquire()
		{
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == Player.whoAmI && SquireMinionTypes.Contains(p.type))
				{
					return p;
				}
			}
			return null;
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier modifier, ref float flat)
		{
			if(!item.CountsAsClass<SummonDamageClass>() && usedMinionSlots > 0)
			{
				modifier -= ServerConfig.Instance.OtherDamageMinionNerf / 100f;
			}
			if (!SquireMinionTypes.Contains(item.shoot))
			{
				return;
			}
			//TODO maybe +=
			modifier *= (1f + squireDamageMultiplierBonus);
		}

		private void MyDidDoubleTap()
		{
			if (Main.myPlayer != Player.whoAmI && Main.netMode != NetmodeID.Server)
			{
				//Only do control related stuff on the local player
				return;
			}

			int tapDirection = Main.ReversedUpDownArmorSetBonuses ? 1 : 0;
			bool tappedRecently = Player.doubleTapCardinalTimer[tapDirection] > 0;
			bool didReleaseTapThisFrame = tapDirection == 0 ?
				Player.releaseDown :
				Player.releaseUp;
			bool didTapThisFrame = tapDirection == 0 ?
				Player.controlDown :
				Player.controlUp;
			didDoubleTap = false;
			if (!tappedRecently)
			{
				didReleaseTap = false;
			}
			else if (didReleaseTapThisFrame && tappedRecently)
			{
				didReleaseTap = true;
			}
			else if (tappedRecently && didReleaseTap && didTapThisFrame)
			{
				didDoubleTap = true;
				didReleaseTap = false;
			}
		}

		public override void PreUpdate()
		{
			MyDidDoubleTap();
		}

		private int modifiedFixedDamage(int damage)
		{
			return (int)(damage * Player.GetDamage<SummonDamageClass>() * squireDamageMultiplierBonus);
		}

		private void SummonSquireSubMinions()
		{
			Projectile mySquire = GetSquire();
			bool hasSquire = mySquire != null;
			if (!hasSquire)
			{
				return;
			}
			int skullType = ProjectileType<SquireSkullProjectile>();
			int technoSkullType = ProjectileType<TechnoCharmProjectile>();
			int crownType = ProjectileType<RoyalCrownProjectile>();
			int batType = ProjectileType<SquireBatProjectile>();
			int tumblerType = ProjectileType<AridTumblerProjectile>();
			var source = mySquire.GetProjectileSource_FromThis();
			bool canSummonAccessory = Player.whoAmI == Main.myPlayer;
			// summon the appropriate squire orbiter(s)
			if (canSummonAccessory && squireSkullAccessory && Player.ownedProjectileCounts[skullType] == 0)
			{
				Projectile.NewProjectile(source, mySquire.Center, mySquire.velocity, skullType, 0, 0, Player.whoAmI);
			}
			if (canSummonAccessory && squireTechnoSkullAccessory && Player.ownedProjectileCounts[technoSkullType] == 0)
			{
				Projectile.NewProjectile(source, mySquire.Center, mySquire.velocity, technoSkullType, 0, 0, Player.whoAmI);
			}
			if (canSummonAccessory && royalArmorSetEquipped && Player.ownedProjectileCounts[crownType] == 0)
			{
				Projectile.NewProjectile(source, mySquire.Center, mySquire.velocity, crownType, modifiedFixedDamage(12), 0, Player.whoAmI);
			}
			if (canSummonAccessory && squireBatAccessory && Player.ownedProjectileCounts[batType] == 0)
			{
				Projectile.NewProjectile(source, mySquire.Center, mySquire.velocity, batType, 0, 0, Player.whoAmI);
			}
			if (canSummonAccessory && aridArmorSetEquipped && Player.ownedProjectileCounts[tumblerType] == 0)
			{
				Projectile.NewProjectile(source, mySquire.Center, mySquire.velocity, tumblerType, modifiedFixedDamage(18), 0, Player.whoAmI);
			}

		}

		public override void PostUpdate()
		{

			SummonSquireSubMinions();
			// apply bat buff if set bonus active
			int buffType = BuffType<SquireBatBuff>();
			int debuffType = BuffType<SquireBatDebuff>();
			if (squireBatAccessory && didDoubleTap && !Player.HasBuff(buffType) && !Player.HasBuff(debuffType))
			{
				Player.AddBuff(buffType, SquireBatAccessory.BuffTime, false);
			}
			// undo buff from skill orbiter
			if (Player.ownedProjectileCounts[ProjectileType<SquireSkullProjectile>()] == 0)
			{
				squireDebuffOnHit = -1;
			}

			// count used minion slots
			usedMinionSlots = 0;
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == Player.whoAmI)
				{
					usedMinionSlots += p.minionSlots;
				}
			}
			if(usedMinionSlots > 0)
			{
				float damageReduction = ServerConfig.Instance.SquireDamageMinionNerf / 100f;
				squireDamageMultiplierBonus -= damageReduction;
			}
			if (ServerConfig.Instance.SquireMinionSlot && GetSquire() != default)
			{
				if( GetSquire() != default)
				{
					Player.maxMinions = Math.Max(0, Player.maxMinions - 1);
				}
			}
		}

		public override void PostUpdateBuffs()
		{
			int buffType = BuffType<SquireBatBuff>();
			int debuffType = BuffType<SquireBatDebuff>();
			if (Player.HasBuff(buffType))
			{
				squireAttackSpeedMultiplier *= 0.75f; // 25% attack speed bonus
				squireTravelSpeedMultiplier += 0.25f; // 25% move speed bonus
				if (Player.buffTime[Player.FindBuffIndex(buffType)] == 1)
				{
					// switch from buff to debuff
					Player.AddBuff(debuffType, SquireBatAccessory.DebuffTime);
				}
			}
			else if (Player.HasBuff(debuffType))
			{
				squireAttackSpeedMultiplier *= 1.1f; // 10% attack speed decrease
				squireDamageOnHitMultiplier -= 0.1f; // reduce damage 10%
			}
		}
	}

	class SquireCooldownBuff : ModBuff
	{

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Squire Special Cooldown");
			Description.SetDefault("Your squire's special is on cooldown!");
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}
	}

	class SquireGlobalProjectile : GlobalProjectile
	{
		public static HashSet<int> isSquireShot;
		// buffs that affect your squire
		public static HashSet<int> squireBuffTypes;
		public static HashSet<int> squireDebuffTypes;

		public override void Load()
		{
			isSquireShot = new HashSet<int>();
			squireBuffTypes = new HashSet<int>();
			squireDebuffTypes = new HashSet<int>();
		}

		public override void Unload()
		{
			isSquireShot = null;
			squireBuffTypes = null;
			squireDebuffTypes = null;
		}
		private void doBuffDust(Projectile projectile, int dustType)
		{
			Vector2 dustVelocity = new Vector2(0, -Main.rand.NextFloat() * 0.25f - 0.5f);
			for (int i = 0; i < 3; i++)
			{
				Vector2 offset = new Vector2(10 * (i - 1), (i == 1 ? -4 : 4) + Main.rand.Next(-2, 2));
				Dust dust = Dust.NewDustPerfect(projectile.Top + offset, dustType, dustVelocity, Scale: 1f);
				dust.customData = projectile.whoAmI;
			}
		}

		// add buff/debuff dusts if we've got a squire affecting buff or debuff
		public override void PostAI(Projectile projectile)
		{
			if (!SquireMinionTypes.Contains(projectile.type))
			{
				return;
			}
			Player player = Main.player[projectile.owner];
			foreach (int buffType in player.buffType)
			{
				bool debuff = false;

				if (squireDebuffTypes.Contains(buffType))
				{
					debuff = true;
				}
				else if (!squireBuffTypes.Contains(buffType))
				{
					continue;
				}
				int timeLeft = player.buffTime[player.FindBuffIndex(buffType)];
				if (timeLeft % 60 == 0)
				{
					doBuffDust(projectile, debuff ? DustType<MinusDust>() : DustType<PlusDust>());
				}
				break;
			}
		}

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (!SquireMinionTypes.Contains(projectile.type) && !isSquireShot.Contains(projectile.type))
			{
				return;
			}
			float multiplier = Main.player[projectile.owner].GetModPlayer<SquireModPlayer>().squireDamageOnHitMultiplier;
			if (multiplier == 1)
			{
				return;
			}
			// may need to manually apply defense formula
			damage = (int)(damage * multiplier);
		}

		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
		{
			if (!SquireMinionTypes.Contains(projectile.type) && !isSquireShot.Contains(projectile.type))
			{
				return;
			}
			SquireModPlayer player = Main.player[projectile.owner].GetModPlayer<SquireModPlayer>();
			int debuffType = player.squireDebuffOnHit;
			int duration = player.squireDebuffTime;
			if (debuffType == -1 || Main.rand.NextFloat() > 0.25f)
			{
				return;
			}
			target.AddBuff(debuffType, duration);
		}
	}
}
