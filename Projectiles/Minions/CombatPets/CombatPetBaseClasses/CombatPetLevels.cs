using AmuletOfManyMinions.Core.Netcode.Packets;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetEmblems;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets
{
	internal enum CombatPetTier: int
	{
		Base = 0,
		Golden = 1,
		Demonite = 2,
		Skeletal = 3,
		Soulful = 4,
		Hallowed = 5,
		Spectre = 6,
		Stardust = 7,
		Celestial = 8
	}

	internal struct CombatPetLevelInfo
	{
		internal int Level;
		internal int BaseDamage; // The base damage done by combat pets at this level of progression
		internal int BaseSearchRange; // The base distance combat pets will seek from the player 
		internal float BaseSpeed; // How the AI actually uses speed varies quite a bit from type to type ...
		internal int MaxPets; // Maximum # of unique combat pets available
		internal string Description; // Used in combat pet item tooltips referring to level up points

		public CombatPetLevelInfo(int level, int damage, int searchRange, int baseSpeed, int maxPets, string description)
		{
			Level = level;
			BaseDamage = damage;
			BaseSearchRange = searchRange;
			BaseSpeed = baseSpeed;
			MaxPets = maxPets;
			Description = description;
		}
	}
	class CombatPetLevelTable : ModSystem
	{
		internal static CombatPetLevelInfo[] PetLevelTable;

		public override void Load()
		{
			PetLevelTable = new CombatPetLevelInfo[]{
				new(0, 7, 500, 8, 1, "Base"), // Base level
				new(1, 11, 550, 8, 1, "Golden"), // ore tier
				new(2, 14, 650, 9, 1, "Demonite"), // EoC - tier
				new(3, 17, 750, 10, 2, "Skeletal"), // Dungeon Tier
				new(4, 30, 900, 12, 2, "Soulful"), // Post WoF
				new(5, 36, 950, 14, 2, "Hallowed"), // Post Mech
				new(6, 42, 1000, 15, 3, "Spectre"), // Post Plantera
				new(7, 52, 1050, 16, 4, "Stardust"), // Post Pillars
				new(8, 80, 1100, 18, 6, "Celestial") // Post Moon Lord
			};
		}

		public override void Unload()
		{
			base.Unload();
		}
	}

	class LeveledCombatPetModPlayer : ModPlayer
	{
		internal int PetLevel { get; set; }
		internal int PetDamage { get; set; }

		internal CombatPetLevelInfo PetLevelInfo => CombatPetLevelTable.PetLevelTable[PetLevel];

		public int PetSlotsUsed { get; internal set; }

		public int ExtraPetSlots { get; internal set; } = 0;

		internal int MaxPetSlots => PetLevelInfo.MaxPets + ExtraPetSlots;

		// whether the player is using a buff that summons multiple pets
		// this is basically just a boolean flag that changes which buffID a pet checks
		// for its tactics group mapping
		public bool UsingMultiPets { get; internal set; }

		private List<int> BuffFlagsToReset = new List<int>();
		private int buffResetCountdown;

		public void UpdatePetLevel(int newLevel, int newDamage, bool fromSync = false)
		{
			bool didUpdate = newLevel != PetLevel || PetDamage != newDamage;
			PetLevel = newLevel;
			PetDamage = newDamage;
			if(didUpdate && !fromSync)
			{
				// TODO MP packet
				new CombatPetLevelPacket(Player, (byte)PetLevel, (short)PetDamage).Send();
			}
		}

		public override void PreUpdate()
		{
			ExtraPetSlots = 0;
		}

		public override void PostUpdate()
		{
			CheckForCombatPetEmblem();
			UpdateCombatPetCount();
			ReflagPetBuffs();
		}

		private void UpdateCombatPetCount()
		{
			PetSlotsUsed = 0;
			int buffCount = Player.CountBuffs();
			for(int i = 0; i < buffCount; i++)
			{
				if(CombatPetBuff.CombatPetBuffTypes.Contains(Player.buffType[i])) 
				{
					PetSlotsUsed += 1;
				}
			}
			if(PetSlotsUsed > 0 && !ServerConfig.Instance.CombatPetsMinionSlots)
			{
				PetSlotsUsed -= 1;
			}
			Player.maxMinions = Math.Max(0, Player.maxMinions - PetSlotsUsed);
		}

		// look for the best Combat Pet Emblem in the player's inventory, use that
		// to set the player's combat pet's damage
		private void CheckForCombatPetEmblem()
		{
			// don't run every frame
			if(Main.GameUpdateCount % 10 != 0)
			{
				return;
			}
			int maxLevel = 0;
			int maxDamage = CombatPetLevelTable.PetLevelTable[0].BaseDamage;
			for(int i = 0; i < Player.inventory.Length; i++)
			{
				Item item = Player.inventory[i];
				if(!item.IsAir && item.ModItem != null && item.ModItem is CombatPetEmblem petEmblem)
				{
					// choose max tier rather than max damage
					if(petEmblem.PetLevel > maxLevel)
					{
						maxLevel = petEmblem.PetLevel;
						maxDamage = item.damage;
					}
				}
			}
			for(int i = 0; i < Player.bank.item.Length; i++)
			{
				Item item = Player.bank.item[i];
				if(!item.IsAir && item.ModItem != null && item.ModItem is CombatPetEmblem petEmblem)
				{
					// choose max tier rather than max damage
					if(petEmblem.PetLevel > maxLevel)
					{
						maxLevel = petEmblem.PetLevel;
						maxDamage = item.damage;
					}
				}
			}
			UpdatePetLevel(maxLevel, maxDamage);
		}

		private void ReflagPetBuffs()
		{
			if(buffResetCountdown -- == 0)
			{
				for(int i = 0; i < BuffFlagsToReset.Count; i++)
				{
					int buffId = BuffFlagsToReset[i];
					Main.vanityPet[buffId] = true;
				}
				BuffFlagsToReset.Clear();
			}
		}

		public void TemporarilyUnflagPetBuff(int buffId)
		{
			if(!ServerConfig.Instance.AllowMultipleCombatPets)
			{
				return;
			}

			if(PetSlotsUsed < MaxPetSlots)
			{
				Main.vanityPet[buffId] = false;
				BuffFlagsToReset.Add(buffId);
				buffResetCountdown = 4;
			} 
			else if (PetSlotsUsed == MaxPetSlots && MaxPetSlots > 1)
			{
				// unmark all but the most recent buff, so that only one pet gets deleted
				int unmarkCount = 0;
				int buffCount = Player.CountBuffs();
				for(int i = 0; i < buffCount; i++)
				{
					buffId = Player.buffType[i];
					if(CombatPetBuff.CombatPetBuffTypes.Contains(buffId))
					{
						Main.vanityPet[buffId] = false;
						BuffFlagsToReset.Add(buffId);
						buffResetCountdown = 4;
						unmarkCount++;
						if(unmarkCount >= PetLevelInfo.MaxPets - 1)
						{
							break;
						}
					}
				}
			}
		}
	}

	public abstract class CombatPetBuff : MinionBuff
	{

		internal static HashSet<int> CombatPetBuffTypes;

		public override void Load()
		{
			CombatPetBuffTypes = new HashSet<int>();
		}

		public override void Unload()
		{
			CombatPetBuffTypes = null;
		}

		public CombatPetBuff(params int[] projIds) : base(projIds) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.vanityPet[Type] = true;
			Main.buffNoSave[Type] = false;
			CombatPetBuffTypes.Add(Type);
		}

		public override void Update(Player player, ref int buffIndex)
		{
			for(int i = 0; i < projectileTypes.Length; i++)
			{
				int projType = projectileTypes[i];
				if(player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0)
				{
					var p = Projectile.NewProjectileDirect(player.GetProjectileSource_Buff(buffIndex), player.Center, Vector2.Zero, projType, 0, 0, player.whoAmI);
					// p.originalDamage is updated in each frame by the minion itself
					p.originalDamage = 0;
				}
			}
			if (projectileTypes.Select(p => player.ownedProjectileCounts[p]).Sum() > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.buffTime[buffIndex] = Math.Min(player.buffTime[buffIndex], 2);
			}
		}
	}

	public abstract class CombatPetVanillaCloneBuff : CombatPetBuff
	{
		public abstract int VanillaBuffId { get; }
		public abstract string VanillaBuffName { get; }

		public CombatPetVanillaCloneBuff(params int[] projIds) : base(projIds) { }

		public override string Texture => "Terraria/Images/Buff_" + VanillaBuffId;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName." + VanillaBuffName) + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription." + VanillaBuffName));
		}

	}

	internal static class CombatPetItemUtils
	{
		public static void AddCombatPetTooltip(Mod mod,int attackPatternUpdateTier, List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(mod, "CombatPetDescription", 
				"This pet's fighting spirit has been awakened!\n" +
				"It can be powered up by holding a Combat Pet Emblem.")
			{
				overrideColor = Color.LimeGreen
			});


			LeveledCombatPetModPlayer player = Main.player[Main.myPlayer].GetModPlayer<LeveledCombatPetModPlayer>();
			if(attackPatternUpdateTier == 0)
			{
				return;
			} else if (attackPatternUpdateTier > player.PetLevel)
			{
				tooltips.Add(new TooltipLine(mod, "CombatPetNotLeveledUp", 
					"This pet will gain a stronger attack pattern if you hold a\n" +
					CombatPetLevelTable.PetLevelTable[attackPatternUpdateTier].Description + 
					" Combat Pet Emblem or stronger!")
				{
					overrideColor = Color.Gray
				});
			} else
			{
				tooltips.Add(new TooltipLine(mod, "CombatPetLeveledUp", 
					"Your emblem enables this pet's stronger attack pattern!")
				{
					overrideColor = Color.LimeGreen
				});
			}
		}

		// hack to temporarily un-flag buffs as pet type to prevent vanilla removal code from running
		// depending on how many open combat pet slots the player has
		public static bool CanUseItem(Player player, Item item)
		{
			LeveledCombatPetModPlayer petPlayer = player.GetModPlayer<LeveledCombatPetModPlayer>();
			petPlayer.TemporarilyUnflagPetBuff(item.buffType);
			return true;
		}
	}

	/**
	 * Bit of a roundabout naming scheme, non-vanilla-clone combat pets came second
	 */
	public abstract class CombatPetCustomMinionItem<TBuff, TProj> : MinionItem<TBuff, TProj> where TBuff: ModBuff where TProj: Minion
	{
		internal virtual int AttackPatternUpdateTier => 0;

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.ZephyrFish); // via ExampleMod
			base.SetDefaults();
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) =>
			CombatPetItemUtils.AddCombatPetTooltip(Mod, AttackPatternUpdateTier, tooltips);

		public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ApplyBuff(player);
			return false;
		}

		public override bool CanUseItem(Player player) => CombatPetItemUtils.CanUseItem(player, Item);

	}

	public abstract class CombatPetMinionItem<TBuff, TProj> : VanillaCloneMinionItem<TBuff, TProj> where TBuff: ModBuff where TProj: Minion
	{
		internal virtual int AttackPatternUpdateTier => 0;

		public override void ModifyTooltips(List<TooltipLine> tooltips) =>
			CombatPetItemUtils.AddCombatPetTooltip(Mod, AttackPatternUpdateTier, tooltips);

		public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ApplyBuff(player);
			return false;
		}

		public override bool CanUseItem(Player player) => CombatPetItemUtils.CanUseItem(player, Item);
	}
}
