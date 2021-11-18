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
	internal struct CombatPetLevelInfo
	{
		internal int Level;
		internal int BaseDamage; // The base damage done by combat pets at this level of progression
		internal int BaseSearchRange; // The base distance combat pets will seek from the player 
		internal float BaseSpeed; // How the AI actually uses speed varies quite a bit from type to type ...
		internal string Description; // Used in combat pet item tooltips referring to level up points

		public CombatPetLevelInfo(int level, int damage, int searchRange, int baseSpeed, string description)
		{
			Level = level;
			BaseDamage = damage;
			BaseSearchRange = searchRange;
			BaseSpeed = baseSpeed;
			Description = description;
		}
	}
	class CombatPetLevelTable : ModSystem
	{
		internal static CombatPetLevelInfo[] PetLevelTable;

		public override void Load()
		{
			PetLevelTable = new CombatPetLevelInfo[]{
				new(0, 7, 500, 8, "Base"), // Base level
				new(1, 9, 550, 8, "Golden"), // ore tier
				new(2, 14, 650, 9, "Demonite"), // EoC - tier
				new(3, 17, 750, 10, "Skeletal"), // Dungeon Tier
				new(4, 30, 900, 12, "Soulful"), // Post WoF
				new(5, 36, 950, 13, "Hallowed"), // Post Mech
				new(6, 42, 1000, 14, "Spectre"), // Post Plantera
				new(7, 48, 1050, 15, "Stardust"), // Post Pillars
				new(8, 60, 1100, 16, "Celestial") // Post Moon Lord
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

		public int PetSlots { get; internal set; }

		// whether the player is using a buff that summons multiple pets
		// this is basically just a boolean flag that changes which buffID a pet checks
		// for its tactics group mapping
		public bool UsingMultiPets { get; internal set; }

		public void UpdatePetLevel(int newLevel, int newDamage, bool fromSync = false)
		{
			bool didUpdate = newLevel != PetLevel || PetDamage != newDamage;
			PetLevel = newLevel;
			PetDamage = newDamage;
			if(didUpdate && !fromSync)
			{
				// TODO MP packet
				new CombatPetLevelPacket(Player, (byte)PetLevel).Send();
			}
		}

		public override void PreUpdate()
		{
			PetSlots = 0;
		}

		public override void PostUpdate()
		{
			// look for the best Combat Pet Emblem in the player's inventory, use that
			// to set the player's combat pet's damage
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
				if(item.ModItem != null && item.ModItem is CombatPetEmblem petEmblem)
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
				if(item.ModItem != null && item.ModItem is CombatPetEmblem petEmblem)
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
			Player.maxMinions = Math.Max(0, Player.maxMinions - PetSlots);
			// subtract minion slots
		}
	}

	public abstract class CombatPetBuff : MinionBuff
	{
		public CombatPetBuff(params int[] projIds) : base(projIds) { }

		internal virtual int MinionSlotsUsed => 1;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.vanityPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetModPlayer<LeveledCombatPetModPlayer>().PetSlots += MinionSlotsUsed;
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

	public abstract class CombatPetMinionItem<TBuff, TProj> : VanillaCloneMinionItem<TBuff, TProj> where TBuff: ModBuff where TProj: Minion
	{
		internal virtual int AttackPatternUpdateTier => 0;
		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "CombatPetDescription", 
				"This pet's fighting spirit has been awakened!\n" +
				"It can be powered up by holding a Combat Pet Emblem.")
			{
				overrideColor = Color.LimeGreen
			});


			LeveledCombatPetModPlayer player = Main.player[Main.myPlayer].GetModPlayer<LeveledCombatPetModPlayer>();
			if(AttackPatternUpdateTier == 0)
			{
				return;
			} else if (AttackPatternUpdateTier > player.PetLevel)
			{
				tooltips.Add(new TooltipLine(Mod, "CombatPetNotLeveledUp", 
					"This pet will gain a stronger attack pattern if you hold a\n" +
					CombatPetLevelTable.PetLevelTable[AttackPatternUpdateTier].Description + 
					" Combat Pet Emblem or stronger!")
				{
					overrideColor = Color.Gray
				});
			} else
			{
				tooltips.Add(new TooltipLine(Mod, "CombatPetLeveledUp", 
					"Your emblem enables this pet's stronger attack pattern!")
				{
					overrideColor = Color.LimeGreen
				});
			}
		}

		public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ApplyBuff(player);
			return false;
		}
	}
}
