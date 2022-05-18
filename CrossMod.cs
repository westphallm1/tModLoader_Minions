using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using AmuletOfManyMinions.Projectiles.Minions.CorruptionAltar;
using AmuletOfManyMinions.Projectiles.Minions.CrimsonAltar;
using AmuletOfManyMinions.Projectiles.Minions.EclipseHerald;
using AmuletOfManyMinions.Projectiles.Minions.GoblinGunner;
using AmuletOfManyMinions.Projectiles.Minions.GoblinTechnomancer;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.Necromancer;
using AmuletOfManyMinions.Projectiles.Minions.SpiritGun;
using AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.Pirate;
using AmuletOfManyMinions.Projectiles.Squires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions
{
	class CrossMod : ModSystem
	{
		public static bool SummonersAssociationLoaded { get; private set; }

		public static bool SummonersShineLoaded { get; private set; }

		public static HashSet<int> MinionBuffTypes;

		public override void Load()
		{
			MinionBuffTypes = new HashSet<int>();
		}
		public override void Unload()
		{
			MinionBuffTypes = null;
		}

		public static bool SharknadoFunc(Projectile p)
		{
			return !((SharknadoMinion)p.ModProjectile).isBeingUsedAsToken;
		}

		public static void AddSummonersAssociationMetadata(Mod mod)
		{
			MinionBuffTypes = new HashSet<int>();
			if (ModLoader.TryGetMod("SummonersAssociation", out Mod summonersAssociation))
			{
				if(summonersAssociation.Version >= new Version(0, 4, 7))
				{
					SummonersAssociationLoaded = true;
					//1. Collect all "EmpoweredMinion" that have a valid CounterType: WORKS FOR ALL EMPOWERED MINIONS (but also includes some "regular" minions which is unintended)
					//var empoweredMinionsWithCounterType = this.GetContent<ModProjectile>().OfType<EmpoweredMinion>().Where(e => e.CounterType > ProjectileID.None).ToList();
					//var counterTypes = empoweredMinionsWithCounterType.Select((e) => e.CounterType).ToHashSet();

					//2. Collect all "CounterMinion": DOES NOT WORK FOR ALL EMPOWERED MINIONS (those that use "regular" minions for counts), but covers all 100% safe ones

					IEnumerable<ModProjectile> minions = mod.GetContent<ModProjectile>().Where(p => p is CounterMinion || p is SquireMinion);

					foreach (var minion in minions)
					{
						summonersAssociation.Call("AddTeleportConditionMinion", minion.Type);
					}

					//Special non-counter projectiles that should not teleport

					//Return false to prevent a teleport
					//Not specifying one will default to "false"
					summonersAssociation.Call("AddTeleportConditionMinion", ModContent.ProjectileType<SharknadoMinion>(), (Func<Projectile, bool>)SharknadoFunc);

					//Static minions should not teleport

					//Don't include the probes, those can move around
					summonersAssociation.Call("AddTeleportConditionMinion", ModContent.ProjectileType<GoblinTechnomancerMinion>());
					summonersAssociation.Call("AddTeleportConditionMinion", ModContent.ProjectileType<CorruptionAltarMinion>());
					summonersAssociation.Call("AddTeleportConditionMinion", ModContent.ProjectileType<CrimsonAltarMinion>());
					summonersAssociation.Call("AddTeleportConditionMinion", ModContent.ProjectileType<GoblinGunnerMinion>());
					summonersAssociation.Call("AddTeleportConditionMinion", ModContent.ProjectileType<NecromancerMinion>());
					summonersAssociation.Call("AddTeleportConditionMinion", ModContent.ProjectileType<SpiritGunMinion>());
					summonersAssociation.Call("AddTeleportConditionMinion", ModContent.ProjectileType<EclipseHeraldMinion>());
					summonersAssociation.Call("AddTeleportConditionMinion", ModContent.ProjectileType<TerrarianEntMinion>());
				}
				if(summonersAssociation.Version >= new Version(0, 4, 1))
				{
					//Minion weapons that summon more than one type
					summonersAssociation.Call(
						"AddMinionInfo",
						ModContent.ItemType<SpiderMinionItem>(),
						ModContent.BuffType<SpiderMinionBuff>(),
						new List<int> {
						ModContent.ProjectileType<JumperSpiderMinion>(),
						ModContent.ProjectileType<VenomSpiderMinion>(),
						ModContent.ProjectileType<DangerousSpiderMinion>()
						}
					);

					summonersAssociation.Call(
						"AddMinionInfo",
						ModContent.ItemType<TwinsMinionItem>(),
						ModContent.BuffType<TwinsMinionBuff>(),
						new List<int> {
						ModContent.ProjectileType<MiniRetinazerMinion>(),
						ModContent.ProjectileType<MiniSpazmatismMinion>()
						}
					);

					summonersAssociation.Call(
						"AddMinionInfo",
						ModContent.ItemType<PirateMinionItem>(),
						ModContent.BuffType<PirateMinionBuff>(),
						new List<int> {
						ModContent.ProjectileType<PirateMinion>(),
						ModContent.ProjectileType<PirateDeadeyeMinion>(),
						ModContent.ProjectileType<ParrotMinion>(),
						ModContent.ProjectileType<FlyingDutchmanMinion>(),
						}
					);

					summonersAssociation.Call(
						"AddMinionInfo",
						ModContent.ItemType<PygmyMinionItem>(),
						ModContent.BuffType<PygmyMinionBuff>(),
						new List<int> {
						ModContent.ProjectileType<Pygmy1Minion>(),
						ModContent.ProjectileType<Pygmy2Minion>(),
						ModContent.ProjectileType<Pygmy3Minion>(),
						ModContent.ProjectileType<Pygmy4Minion>()
						}
					);

					summonersAssociation.Call(
						"AddMinionInfo",
						ModContent.ItemType<DeadlySphereMinionItem>(),
						ModContent.BuffType<DeadlySphereMinionBuff>(),
						new List<int> {
						ModContent.ProjectileType<DeadlySphereMinion>(),
						ModContent.ProjectileType<DeadlySphereClingerMinion>(),
						ModContent.ProjectileType<DeadlySphereFireMinion>()
						}
					);
				}
			}
		}

		public static void PopulateSummonersAssociationBuffSet(Mod mod)
		{
			Version minSupportedVersion = new Version(0, 4, 7);
			if (ModLoader.TryGetMod("SummonersAssociation", out Mod summonersAssociation) && summonersAssociation.Version >= minSupportedVersion)
			{
				object supportedMinionsData = summonersAssociation.Call("GetSupportedMinions", mod, minSupportedVersion.ToString());
				if(supportedMinionsData is List<Dictionary<string, object>> minionsList)
				{
					MinionBuffTypes.UnionWith(minionsList.Select(map =>
						map.ContainsKey("BuffID") ? Convert.ToInt32(map["BuffID"]) : -1).ToList());
				}
			}
		}

		public static int GetSummonersAssociationVarietyCount()
		{
			int varietyCount = 0;
			int[] buffTypes = Main.player[Main.myPlayer].buffType;
			for(int i = 0; i < buffTypes.Length; i++)
			{
				varietyCount += MinionBuffTypes.Contains(buffTypes[i]) ? 1 : 0;
			}
			return varietyCount;
		}


		/// <summary>
		/// Blacklist problem projectiles from summoner's shine's globalitems
		/// </summary>
		public static void AddSummonersShineMetadata(Mod mod)
		{
			const int ADD_FILTER = 0;
			const int BLACKLIST_PROJECTILE = 1;
			const int DONT_COUNT_AS_MINION = 4;
			if (ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				int projType = MinionWaypoint.Type;
				summonersShine.Call(ADD_FILTER, BLACKLIST_PROJECTILE, projType);
				summonersShine.Call(ADD_FILTER, DONT_COUNT_AS_MINION, projType);
				// there are some weird speed/behavior inconsistencies
				// with most minions, especially while following the waypoint,
				// so give the option of disabling them all
				if(ServerConfig.Instance.DisableSummonersShineAI)
				{
					IEnumerable<ModProjectile> minions = mod.GetContent<ModProjectile>().Where(p => p is Minion);
					foreach(var minion in minions)
					{
						summonersShine.Call(ADD_FILTER, BLACKLIST_PROJECTILE, minion.Type);
						summonersShine.Call(ADD_FILTER, DONT_COUNT_AS_MINION, minion.Type);
					}
				}
			}
		}
		
		public static void BakeSummonersShineMinionPower_NoHooks(int ItemType, SummonersShineMinionPowerCollection minionPowers)
		{
			const int ADD_ITEM_STATICS = 2;
			if (ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				summonersShine.Call(ADD_ITEM_STATICS, ItemType, null, null, minionPowers.BakeToTupleArray(), 0, true);
			}
		}
		
		public class SummonersShineMinionPowerCollection
		{
			List<Tuple<float, int, int, bool>> minionPowers = new();

			/// <summary>
			/// Call this to feed data into ModSupport_AddItemStatics. Adds a Minion Power to the Minion Power Collection
			/// </summary>
			/// <param name="power">The base number of the minion power</param>
			/// <param name="scalingType">How the minion power will scale with ability power modifiers</param>
			/// <param name="roundingType">How much to round the ability power value to</param>
			/// <param name="DifficultyScale">If true, halves this in Journey, doubles this in Expert, triples this in Master.</param>
			/// 
			public void AddMinionPower(float power, MinionPowerScalingType scalingType = MinionPowerScalingType.multiply, MinionPowerRoundingType roundingType = MinionPowerRoundingType.dp2, bool DifficultyScale = false)
			{
				minionPowers.Add(new Tuple<float, int, int, bool>
				(
					power,
					(int)scalingType,
					(int)roundingType,
					DifficultyScale
				));
			}

			public Tuple<float, int, int, bool>[] BakeToTupleArray()
			{
				return minionPowers.ToArray();
			}
			public enum MinionPowerScalingType
			{
				add,
				subtract,
				multiply,
				divide,
			}
			public enum MinionPowerRoundingType
			{
				dp2,
				integer,
			}
		}
		
		public static float ApplyCrossModScaling(float original, Projectile projectile, int summonersShineMinionPowerIndex = 0, bool invertSummonersShine = false)
		{
			float rv = ReplaceValueWithSummonersShineMinionPower(original, projectile, summonersShineMinionPowerIndex, invertSummonersShine);
			return rv;
		}
		
		public static float ReplaceValueWithSummonersShineMinionPower(float value, Projectile projectile, int index, bool invert = false)
		{
			const int USEFUL_FUNCS = 10;
			const int GET_ALL_MINION_POWER_DATA = 10;
			
			if (ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				Tuple<float, float, int, int, bool> rv = (Tuple<float, float, int, int, bool>)summonersShine.Call(USEFUL_FUNCS, GET_ALL_MINION_POWER_DATA, projectile, index);
				float outValue = rv.Item1;
				float original = rv.Item2;
				SummonersShineMinionPowerCollection.MinionPowerScalingType mpScalingType = (SummonersShineMinionPowerCollection.MinionPowerScalingType)rv.Item3;
				SummonersShineMinionPowerCollection.MinionPowerRoundingType mpRoudingType = (SummonersShineMinionPowerCollection.MinionPowerRoundingType)rv.Item4;
				bool difficultyScale = rv.Item5;
				switch(mpScalingType){
					case SummonersShineMinionPowerCollection.MinionPowerScalingType.add:
					case SummonersShineMinionPowerCollection.MinionPowerScalingType.subtract:
						if(invert)
							value -= (outValue - original);
						else
							value += (outValue - original);
						break;
					case SummonersShineMinionPowerCollection.MinionPowerScalingType.multiply:
					case SummonersShineMinionPowerCollection.MinionPowerScalingType.divide:
						if(original != 0 && !invert)
						{
							value *= (outValue / original);
						}
						if(outValue != 0 && invert)
						{
							value *= (original / outValue);
						}
						break;
				}
				
				if (mpRoudingType == SummonersShineMinionPowerCollection.MinionPowerRoundingType.integer)
					value = MathF.Round(value);
				else
					value = MathF.Round(value, 2);
			}
			return value;
		}
		
		/// <summary>
		/// Returns true if on default tick, false if on extra Summoner's Shine ticks
		/// </summary>
		public static bool StopSummonersShineFromAcceleratingSpecialAbilityCountdown(Projectile projectile)
		{
			const int GET_MINIONPROJECTILEDATA_VAR = 7;
			const int GET_CURRENTTICK = 17;
			if (ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				return (float)summonersShine.Call(GET_MINIONPROJECTILEDATA_VAR, projectile.whoAmI, GET_CURRENTTICK) == 1;
			}
			return true;
		}
		
		public static void SetSummonersShineProjMaxEnergy(int ProjectileType, float maxEnergy)
		{
			const int HOOKPROJECTILE = 1;
			const int HOOKPROJECTILEMAXENERGY = 0;
			if (ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				summonersShine.Call(HOOKPROJECTILE, ProjectileType, HOOKPROJECTILEMAXENERGY, maxEnergy);
				return;
			}
			return;
		}
		
		public static int GetCrossModNormalizedSpecialFrame(int original, Projectile projectile)
		{
			const int GET_MINIONPROJECTILEDATA_VAR = 7;
			const int GET_MINIONSPEEDMODTYPE = 14;
			const int MINIONSPEEDMODTYPE_NORMAL = 0;
			const int MINIONSPEEDMODTYPE_STEPPED = 1;
			const int USEFULFUNCS = 10;
			const int USEFULFUNCS_GETSIMRATE = 5;
			const int USEFULFUNCS_GETINTERNALSIMRATE = 5;
			if (ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				int minionSpeedModType = (int)summonersShine.Call(GET_MINIONPROJECTILEDATA_VAR, projectile.whoAmI, GET_MINIONSPEEDMODTYPE);
				switch(minionSpeedModType)
				{
					case MINIONSPEEDMODTYPE_NORMAL:
						return (int)(original / (float)summonersShine.Call(USEFULFUNCS, USEFULFUNCS_GETSIMRATE, projectile));
					case MINIONSPEEDMODTYPE_STEPPED:
						return (int)(original / (float)summonersShine.Call(USEFULFUNCS, USEFULFUNCS_GETINTERNALSIMRATE, projectile));
						break;
				}
			}
			return original;
		}

		public static int GetCrossModNormalizedSpecialDuration(int original, Projectile projectile)
		{
			const int GET_MINIONPROJECTILEDATA_VAR = 7;
			const int GET_MINIONSPEEDMODTYPE = 14;
			const int MINIONSPEEDMODTYPE_NORMAL = 0;
			const int MINIONSPEEDMODTYPE_STEPPED = 1;
			const int USEFULFUNCS = 10;
			const int USEFULFUNCS_GETSIMRATE = 5;
			const int USEFULFUNCS_GETINTERNALSIMRATE = 5;
			if (ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				int minionSpeedModType = (int)summonersShine.Call(GET_MINIONPROJECTILEDATA_VAR, projectile.whoAmI, GET_MINIONSPEEDMODTYPE);
				switch (minionSpeedModType)
				{
					case MINIONSPEEDMODTYPE_NORMAL:
						return (int)(original * (float)summonersShine.Call(USEFULFUNCS, USEFULFUNCS_GETSIMRATE, projectile));
					case MINIONSPEEDMODTYPE_STEPPED:
						return (int)(original * (float)summonersShine.Call(USEFULFUNCS, USEFULFUNCS_GETINTERNALSIMRATE, projectile));
						break;
				}
			}
			return original;
		}
		
		public static Item GetPrefixComparisonItem(int netID)
        {
            if (Main.tooltipPrefixComparisonItem == null)
            {
                Main.tooltipPrefixComparisonItem = new Item();
            }
            Item compItem = Main.tooltipPrefixComparisonItem;
            if (compItem.netID != netID)
                compItem.netDefaults(netID);
            return compItem;
        }
		
		public static void GetCrossModEmblemStats(LeveledCombatPetModPlayer modPlayer, Item item)
		{
			Mod summonersShine = null;
			int maxArray = 0;
			if (ModLoader.TryGetMod("SummonersShine", out summonersShine))
			{
				maxArray += 3;
			}
			modPlayer.PetModdedStats = new object[maxArray];
			int currentArrayPos = 0;
			if(summonersShine != null)
			{
				const int GET_REWORKMINION_ITEM_VALUE = 16;
				const int PREFIXMINIONPOWER = 0;
				if (item != null)
				{
					Item compItem = GetPrefixComparisonItem(item.netID);
					if (compItem.useTime > 0)
					{
						modPlayer.PetModdedStats[currentArrayPos] = item.useTime / (float)(compItem.useTime);
					}
					else
						modPlayer.PetModdedStats[currentArrayPos] = 1;
					currentArrayPos++;
					modPlayer.PetModdedStats[currentArrayPos] = item.crit;
					currentArrayPos++;

					modPlayer.PetModdedStats[currentArrayPos] = summonersShine.Call(GET_REWORKMINION_ITEM_VALUE, item, PREFIXMINIONPOWER);
					currentArrayPos++;
				}
				else
				{
					modPlayer.PetModdedStats[currentArrayPos] = 1f;
					currentArrayPos++;
					modPlayer.PetModdedStats[currentArrayPos] = 0;
					currentArrayPos++;
					modPlayer.PetModdedStats[currentArrayPos] = 0f;
					currentArrayPos++;
				}
			}
		}
		
		public static void CombatPetComputeMinionStats(Projectile projectile, LeveledCombatPetModPlayer modPlayer)
		{
			int currentArrayPos = 0;
			if (ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				const int SET_PROJFUNCS = 4;
				const int SET_PROJDATA = 5;
				const int USEFULFUNCS = 10;
				const int OVERRIDESOURCEITEM = 11;
				const int MINIONASMOD = 1;
				const int PROJECTILECRIT = 0;
				const int PREFIXMINIONPOWER = 10;
				summonersShine.Call(SET_PROJFUNCS, projectile.whoAmI, MINIONASMOD, modPlayer.PetModdedStats[currentArrayPos]);
				currentArrayPos++;
				summonersShine.Call(SET_PROJFUNCS, projectile.whoAmI, PROJECTILECRIT, modPlayer.PetModdedStats[currentArrayPos]);
				currentArrayPos++;
				summonersShine.Call(SET_PROJDATA, projectile.whoAmI, PREFIXMINIONPOWER, modPlayer.PetModdedStats[currentArrayPos]);
				currentArrayPos++;
				summonersShine.Call(USEFULFUNCS, OVERRIDESOURCEITEM, projectile, modPlayer.PetEmblemItem);
			}
		}
	}
}
