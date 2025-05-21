using AmuletOfManyMinions.Core.Minions.CrossModAI;
using AmuletOfManyMinions.Core.Netcode.Packets;
using AmuletOfManyMinions.CrossModClient.SummonersShine;
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
	public enum CombatPetTier: int
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

	public interface ICombatPetLevelInfo
	{
		int Level { get; }
		int BaseDamage { get; }
		int BaseSearchRange { get; }
		float BaseSpeed { get; }
		int MaxPets { get; }
		LocalizedText Description { get; }
	}

	public struct CombatPetLevelInfo : ICombatPetLevelInfo
	{
		public int Level { get; private set;  }
		public int BaseDamage { get; private set; }
		public int BaseSearchRange { get; private set; }
		public float BaseSpeed { get; private set; }
		public int MaxPets { get; private set; }
		public LocalizedText Description { get; private set; }

		public CombatPetLevelInfo(int level, int damage, int searchRange, int baseSpeed, int maxPets, string key)
		{
			Level = level;
			BaseDamage = damage;
			BaseSearchRange = searchRange;
			BaseSpeed = baseSpeed;
			MaxPets = maxPets;
			Description = Language.GetOrRegister(key);
		}
	}

	public class PlayerCombatPetLevelInfo : ICombatPetLevelInfo
	{
		public int Level { get; private set; } = 0;
		public int BaseDamage => RawInfo.BaseDamage + Player.PetDamageBonus;
		public int BaseSearchRange => RawInfo.BaseSearchRange + Player.SearchRangeBonus;
		public float BaseSpeed => RawInfo.BaseSpeed + Player.PetSpeedBonus;
		public int MaxPets => RawInfo.MaxPets;
		public LocalizedText Description => RawInfo.Description;

		private readonly LeveledCombatPetModPlayer Player;
		private ICombatPetLevelInfo RawInfo => CombatPetLevelTable.PetLevelTable[Level];

		public PlayerCombatPetLevelInfo(LeveledCombatPetModPlayer player)
		{
			Player = player;
		}

		public PlayerCombatPetLevelInfo WithLevel(int level)
		{
			Level = level;
			return this;
		}
	}

	public class CombatPetLevelTable : ModSystem
	{
		public static ICombatPetLevelInfo[] PetLevelTable;

		public override void Load()
		{
			string commonKey = Mod.GetLocalizationKey("CombatPetLevels.");
			PetLevelTable = new ICombatPetLevelInfo[]{
				new CombatPetLevelInfo(0, 7, 550, 8, 1, $"{commonKey}Base"),
				new CombatPetLevelInfo(1, 11, 600, 8, 1, $"{commonKey}Golden"),
				new CombatPetLevelInfo(2, 15, 700, 9, 1, $"{commonKey}Demonite"),
				new CombatPetLevelInfo(3, 18, 750, 10, 2, $"{commonKey}Skeletal"),
				new CombatPetLevelInfo(4, 30, 900, 12, 2, $"{commonKey}Soulful"),
				new CombatPetLevelInfo(5, 36, 950, 14, 3, $"{commonKey}Hallowed"),
				new CombatPetLevelInfo(6, 42, 1000, 15, 3, $"{commonKey}Spectre"),
				new CombatPetLevelInfo(7, 52, 1050, 16, 4, $"{commonKey}Stardust"),
				new CombatPetLevelInfo(8, 80, 1100, 18, 6, $"{commonKey}Celestial")
			};
		}

		public override void Unload()
		{
			PetLevelTable = null;
		}
	}
}
