using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Netcode.Packets;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Items.Armor.IllusionistArmor;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using AmuletOfManyMinions.Projectiles.Minions.LilEnt;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd;
using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static AmuletOfManyMinions.CrossMod;
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
		internal bool lilEntAccessoryEquipped;
		internal bool illusionistArmorSetEquipped;
		internal int idleMinionSyncronizationFrame = 0;
		internal int minionVarietyBonusCount = 0;
		internal float minionVarietyDamageBonus = 0;
		internal int minionVarietyDamageFlatBonus = 0;
		internal bool didDrawDustThisFrame = false;
		private HashSet<int> uniqueMinionTypes = new HashSet<int>();

		internal static Color[] MinionColors = new Color[]
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
			lilEntAccessoryEquipped = false;
			illusionistArmorSetEquipped = false;
			minionVarietyDamageBonus = 0.03f;
		    didDrawDustThisFrame = false;
		}

		public override void PreUpdate()
		{
			// Get unique minion count for player
			if(SummonersAssociationMinionBuffTypesLoaded)
			{
				minionVarietyBonusCount = GetSummonersAssociationVarietyCount();
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
		public override void PostUpdateEquips()
		{
			// 1.4 allows us to place the minion variety bonus directly on the player, since summon
			// damage updates dynamically
			if (minionVarietyBonusCount > 1)
			{
				Player.GetDamage<SummonDamageClass>() += minionVarietyBonusCount * minionVarietyDamageBonus;
			}
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier modifier)
		{
			if (!item.CountsAsClass<SummonDamageClass>())
			{
				return;
			}
			foreach (NecromancerAccessory accessory in NecromancerAccessory.accessories)
			{
				if (accessory.IsEquipped(this))
				{
					accessory.ModifyPlayerWeaponDamage(this, item, ref modifier);
				}
			}
			if(item.ModItem?.Mod == Mod)
			{
				modifier += (ServerConfig.Instance.GlobalDamageMultiplier - 100) / 100f;
			}
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
					Projectile.NewProjectile(Player.GetSource_Misc("illusionistArmorSet"), Player.Center, Vector2.Zero, projectileType, 
						(int)(Player.GetDamage<SummonDamageClass>().ApplyTo(20)), 0.1f, Player.whoAmI, ai0: isCorrupt ? 0 : 1);
				}
			}
			int flinxType = ProjectileType<BonusFlinxMinion>();
			if(flinxArmorSetEquipped && Player.ownedProjectileCounts[flinxType] == 0)
			{
				Player.AddBuff(BuffType<FlinxMinionBuff>(), 3);
				int projId = Projectile.NewProjectile(Player.GetSource_Misc("flinxArmorSet"), Player.Center, Vector2.Zero, flinxType, 8, 2, Player.whoAmI);
				Main.projectile[projId].originalDamage = 8;
			}

			int lilEntType = ProjectileType<LilEntMinion>();
			if(lilEntAccessoryEquipped && Player.ownedProjectileCounts[lilEntType] == 0)
			{
				Player.AddBuff(BuffType<LilEntMinionBuff>(), 3);
				int projId = Projectile.NewProjectile(Player.GetSource_Misc("lilEntAccessory"), Player.Center, Vector2.Zero, lilEntType, 8, 2, Player.whoAmI);
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

			if(Player.ownedProjectileCounts[ProjectileType<BabyFinchMinion>()] > 0)
			{
				Player.babyBird = true;
			}
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			new SyncIdleAnimationFramePacket(Player, idleMinionSyncronizationFrame).Send(toWho, fromWho);
		}
	}

	public class MinionVarietyBuff : ModBuff
	{
		public LocalizedText VarietyDamageBonusText { get; private set; }

		public override LocalizedText Description => LocalizedText.Empty;

		public override void SetStaticDefaults()
		{
			VarietyDamageBonusText = this.GetLocalization("VarietyDamageBonus");

			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true; // don't allow cancellation
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
		{
			Player player = Main.LocalPlayer;
			MinionSpawningItemPlayer modPlayer = player.GetModPlayer<MinionSpawningItemPlayer>();
			float varietyBonus = modPlayer.minionVarietyBonusCount * modPlayer.minionVarietyDamageBonus * 100;
			tip = VarietyDamageBonusText.Format(varietyBonus.ToString("0"));
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if(player.GetModPlayer<MinionSpawningItemPlayer>().minionVarietyBonusCount > 1 
				&& ClientConfig.Instance.ShowMinionVarietyBonus)
			{
				player.buffTime[buffIndex] = 2;
			}
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
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
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
					accessory.SpawnProjectileOnChance(projectile, target, hit.SourceDamage);
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

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
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
			if(squirePlayer.GetSquire() != default)
			{
				damageMult -= ServerConfig.Instance.MinionDamageSquireNerf / 100f;
			}
			if(target.HasBuff<SquireTagDamageBuff>())
			{
				damageMult += 0.1f;
			}
			modifiers.SourceDamage *= damageMult; //Rather use multiply since it's mostly reduction
		}
	}
}
