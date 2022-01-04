using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Netcode.Packets;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Items.Armor.IllusionistArmor;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd;
using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static AmuletOfManyMinions.AmuletOfManyMinions;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories
{
	internal class MinionSpawningItemPlayer : ModPlayer
	{
		public bool wormOnAStringEquipped = false;
		public bool spiritCallerCharmEquipped = false;
		public bool techromancerAccessoryEquipped = false;
		internal bool foragerArmorSetEquipped;
		internal bool flinxArmorSetEquipped;
		internal int summonFlatDamage;
		internal bool illusionistArmorSetEquipped;
		internal int idleMinionSyncronizationFrame = 0;
		internal int minionVarietyBonusCount = 0;
		internal float minionVarietyDamageBonus = 0;
		internal int minionVarietyDamageFlatBonus = 0;
		internal bool didDrawDustThisFrame = false;
		private HashSet<int> uniqueMinionTypes = new HashSet<int>();

		static Color[] MinionColors = new Color[]
		{
			Color.Red,
			Color.LimeGreen,
			Color.Blue,
			Color.Orange,
			Color.Indigo,
			Color.Yellow,
			Color.Violet,
			Color.Crimson,
			Color.Green,
			Color.RoyalBlue,
			Color.Aquamarine,
			Color.Gold,
			Color.Purple,
		};

		internal int colorIdx = 0;

		public Color GetNextColor()
		{
			return MinionColors[colorIdx++ % MinionColors.Length];
		}

		public int GetNextColorIndex()
		{
			return colorIdx++;
		}

		public override void ResetEffects()
		{
			wormOnAStringEquipped = false;
			spiritCallerCharmEquipped = false;
			techromancerAccessoryEquipped = false;
			foragerArmorSetEquipped = false;
			flinxArmorSetEquipped = false;
			illusionistArmorSetEquipped = false;
			summonFlatDamage = 0;
			minionVarietyDamageBonus = 0.03f;
		    didDrawDustThisFrame = false;
		}

		public override void PreUpdate()
		{
			// Get unique minion count for player
			if(SummonersAssociationLoaded)
			{
				minionVarietyBonusCount = CrossMod.GetSummonersAssociationVarietyCount();
			} else
			{
				uniqueMinionTypes.Clear();
				for(int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile proj = Main.projectile[i];
					// only count minions that take up slots (no squires, temporary projectiles that were lazily coded
					// as minions, etc.)
					if (proj.active && proj.owner == Player.whoAmI && proj.minionSlots > 0)
					{
						uniqueMinionTypes.Add(proj.type);
					}
				}
				minionVarietyBonusCount = uniqueMinionTypes.Count;
			}

			// add bonus for unique combat pets
			for(int i = 0; i < Player.CountBuffs(); i++)
			{
				if(CombatPetBuff.CombatPetBuffTypes.Contains(Player.buffType[i]))
				{
					minionVarietyBonusCount += 1;
				}
			}
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier modifier, ref float flat)
		{
			if (!item.CountsAsClass<SummonDamageClass>())
			{
				return;
			}
			foreach (NecromancerAccessory accessory in NecromancerAccessory.accessories)
			{
				if (accessory.IsEquipped(this))
				{
					accessory.ModifyPlayerWeaponDamage(this, item, ref modifier, ref flat);
				}
			}
			if(item.ModItem?.Mod == Mod)
			{
				modifier += (ServerConfig.Instance.GlobalDamageMultiplier - 100) / 100f;
			}
			// a bit hacky, will wanna make this nicer eventually
			flat += summonFlatDamage;
		}

		public List<Projectile> GetMinionsOfType(int projectileType)
		{
			var otherMinions = new List<Projectile>();
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (other.active && other.owner == Player.whoAmI && other.type == projectileType)
				{
					otherMinions.Add(other);
				}
			}
			otherMinions.Sort((x, y) => x.minionPos - y.minionPos);
			return otherMinions;
		}

		public override void PostUpdate()
		{
			idleMinionSyncronizationFrame++;
			if (Player.whoAmI != Main.myPlayer)
			{
				return;
			}
			int projectileType = ProjectileType<IllusionistWisp>();
			if (illusionistArmorSetEquipped && GetMinionsOfType(projectileType).Count < 3)
			{
				int buffType = BuffType<IllusionistWispBuff>();
				// this is a hacky check
				bool isCorrupt = Player.armor.Any(i => i.type == ItemType<IllusionistCorruptHood>());
				if (!Player.HasBuff(buffType))
				{
					Player.AddBuff(buffType, IllusionistWisp.SpawnFrequency);
				}
				else if (Player.buffTime[Player.FindBuffIndex(buffType)] == 1)
				{
					Projectile.NewProjectile(Player.GetProjectileSource_SetBonus(-1), Player.Center, Vector2.Zero, projectileType, 
						(int)(20 * Player.GetDamage<SummonDamageClass>()), 0.1f, Player.whoAmI, ai0: isCorrupt ? 0 : 1);
				}
			}
			int flinxType = ProjectileType<BonusFlinxMinion>();
			if(flinxArmorSetEquipped && Player.ownedProjectileCounts[flinxType] == 0)
			{
				Player.AddBuff(BuffType<FlinxMinionBuff>(), 3);
				int projId = Projectile.NewProjectile(Player.GetProjectileSource_SetBonus(-1), Player.Center, Vector2.Zero, flinxType, 8, 2, Player.whoAmI);
				Main.projectile[projId].originalDamage = 8;
			}
			if (minionVarietyBonusCount > 1 && ClientConfig.Instance.ShowMinionVarietyBonus)
			{
				int buffType = BuffType<MinionVarietyBuff>();
				if (!Player.HasBuff(buffType))
				{
					Player.AddBuff(buffType, 2);
				}
			}
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			new SyncIdleAnimationFramePacket(Player, idleMinionSyncronizationFrame).Send(toWho, fromWho);
		}
	}

	public class MinionVarietyBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true; // don't allow cancellation
			Main.buffNoTimeDisplay[Type] = true;
			DisplayName.SetDefault("Minion Variety!");
		}

		public override void ModifyBuffTip(ref string tip, ref int rare)
		{
			// assuming this is only called client-side and it's always the owner who mouses over the buff tip
			// this may not be the best way to update text
			Player player = Main.player[Main.myPlayer];
			MinionSpawningItemPlayer modPlayer = player.GetModPlayer<MinionSpawningItemPlayer>();
			float varietyBonus = modPlayer.minionVarietyBonusCount * modPlayer.minionVarietyDamageBonus * 100;
			tip = varietyBonus.ToString("0") + "% Minion Variety Damage Bonus";
		}
	}

	class MinionSpawningItemGlobalProjectile : GlobalProjectile
	{
		private bool DoesSummonDamage(Projectile projectile)
		{
			return projectile.minion ||
				ProjectileID.Sets.MinionShot[projectile.type] ||
				SquireGlobalProjectile.isSquireShot.Contains(projectile.type);
		}
		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
		{
			if (!DoesSummonDamage(projectile))
			{
				return;
			}
			Player player = Main.player[projectile.owner];
			MinionSpawningItemPlayer modPlayer = player.GetModPlayer<MinionSpawningItemPlayer>();
			foreach (NecromancerAccessory accessory in NecromancerAccessory.accessories)
			{
				if (accessory.IsEquipped(modPlayer))
				{
					accessory.SpawnProjectileOnChance(projectile, target, damage);
				}
			}
			int wispType = ProjectileType<IllusionistWisp>();
			List<Projectile> wisps = modPlayer.GetMinionsOfType(wispType);
			if (wisps.Count == 3 && wisps.All(p => p.timeLeft <= 10))
			{
				foreach (Projectile wisp in wisps)
				{
					wisp.ai[1] = target.whoAmI;
				}
			}
		}

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (!DoesSummonDamage(projectile))
			{
				return;
			}
			Player player = Main.player[projectile.owner];
			MinionSpawningItemPlayer modPlayer = player.GetModPlayer<MinionSpawningItemPlayer>();
			SquireModPlayer squirePlayer = player.GetModPlayer<SquireModPlayer>();
			// require multiple minion types for any bonus
			float damageMult = 1;
			if (modPlayer.minionVarietyBonusCount > 1)
			{
				damageMult += modPlayer.minionVarietyBonusCount * modPlayer.minionVarietyDamageBonus;
			}
			if(squirePlayer.GetSquire() != default)
			{
				damageMult -= ServerConfig.Instance.MinionDamageSquireNerf / 100f;
			}
			damage = (int)(damage * damageMult);
		}
	}
}
