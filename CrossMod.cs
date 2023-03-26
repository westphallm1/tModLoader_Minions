using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
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
using System.IO;
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
			const int COUNTS_AS_WHIP_FOR_INSTASTRIKE = 14;
			if (ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				SummonersShineLoaded = true;
				int projType = MinionWaypoint.Type;
				summonersShine.Call(ADD_FILTER, BLACKLIST_PROJECTILE, projType);
				summonersShine.Call(ADD_FILTER, DONT_COUNT_AS_MINION, projType);
				// there are some weird speed/behavior inconsistencies
				// with most minions, especially while following the waypoint,
				// so give the option of disabling them all
				if (ServerConfig.Instance.DisableSummonersShineAI)
				{
					IEnumerable<ModProjectile> minions = mod.GetContent<ModProjectile>().Where(p => p is Minion);
					foreach (var minion in minions)
					{
						summonersShine.Call(ADD_FILTER, BLACKLIST_PROJECTILE, minion.Type);
						summonersShine.Call(ADD_FILTER, DONT_COUNT_AS_MINION, minion.Type);
					}
				}

				//IEnumerable<ModProjectile> combatPets = mod.GetContent<ModProjectile>().Where(p =>
				//	p is CombatPetGroundedMeleeMinion || p is CombatPetHoverShooterMinion ||
				//	p is CombatPetSlimeMinion || p is CombatPetWormMinion || p is CombatPetGroundedWormMinion);
				//foreach (var combatPet in combatPets)
				//{
				//	summonersShine.Call(ADD_FILTER, BLACKLIST_PROJECTILE, combatPet.Type);
				//}
				IEnumerable<ModProjectile> counterMinions = mod.GetContent<ModProjectile>().Where(p => p is CounterMinion);
				foreach (var minion in counterMinions)
				{
					summonersShine.Call(ADD_FILTER, BLACKLIST_PROJECTILE, minion.Type);
				}
				
				IEnumerable<ModItem> squireItems = mod.GetContent<ModItem>().Where(p => p is SquireMinionItemDetector);
				foreach (var squireItem in squireItems)
				{
					summonersShine.Call(ADD_FILTER, COUNTS_AS_WHIP_FOR_INSTASTRIKE, squireItem.Type);
				}
			}
		}

		public enum SummonersShineDefaultSpecialWhitelistType { 
			RANGED,
			MELEE,
			RANGEDNOINSTASTRIKE,
			MELEENOINSTASTRIKE,
			RANGEDNOMULTISHOT,
		}

		static readonly BitsByte[] SUMMONERS_SHINE_RANGED_WHITELISTDEFAULTS = new BitsByte[] {
			new BitsByte(
				true, //multishot
				true, //enrage (safe)
				true) //instastrike (ranged)
		};
		
		static readonly BitsByte[] SUMMONERS_SHINE_RANGED_WHITELISTDEFAULTS_NOMULTISHOT = new BitsByte[] {
			new BitsByte(
				false, //multishot
				true, //enrage (safe)
				true) //instastrike (ranged)
		};

		static readonly BitsByte[] SUMMONERS_SHINE_MELEE_WHITELISTDEFAULTS = new BitsByte[] {
			new BitsByte(
				false, //multishot
				true, //enrage (safe)
				false, //instastrike (ranged)
				true) //instastrike (melee)
		};
		
		static readonly BitsByte[] SUMMONERS_SHINE_RANGED_WHITELISTDEFAULTS_NOINSTASTRIKE = new BitsByte[] {
			new BitsByte(
				true, //multishot
				true) //enrage (safe)
		};
		//this is default minion behavior
		static readonly BitsByte[] SUMMONERS_SHINE_MELEE_WHITELISTDEFAULTS_NOINSTASTRIKE = new BitsByte[] {
			new BitsByte(
				false, //multishot
				true) //enrage (safe)
		};
		
		public static void BakeAoMMVersionSpecialAbilities()
		{
            const int SET_CONFIG = 0;
            const int SET_SPECIAL_ITEM_DUPE = 18;
            const int SET_SPECIAL_PROJ_DUPE = 19;

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.BabyBirdStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.BabyBird);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.SlimeStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.BabySlime);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.FlinxStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.FlinxMinion);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.VampireFrogStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.VampireFrog);

            //summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.HornetStaff);
            //summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.Hornet);

            //summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.ImpStaff);
            //summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.FlyingImp);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.SpiderStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.DangerousSpider);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.PirateStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.OneEyedPirate);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.Smolstar);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.Smolstar);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.SanguineStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.BatOfLight);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.TempestStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.Sharknado);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.OpticStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.Retanimini);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.Spazmamini);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.PygmyStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.Pygmy);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.DeadlySphereStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.DeadlySphere);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.RavenStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.Raven);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.XenoStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.UFOMinion);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.StardustCellStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.StardustCellMinion);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.StardustDragonStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ItemType<FlinxMinion>(), ProjectileID.StardustDragon1);
			
			
		}
		
		public static void WhitelistSummonersShineMinionDefaultSpecialAbility(int ItemType, SummonersShineDefaultSpecialWhitelistType specialWhitelistType)
		{
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				BitsByte[] enabledData = null;
				switch (specialWhitelistType)
				{
					case SummonersShineDefaultSpecialWhitelistType.RANGED:
						enabledData = SUMMONERS_SHINE_RANGED_WHITELISTDEFAULTS;
						break;
					case SummonersShineDefaultSpecialWhitelistType.MELEE:
						enabledData = SUMMONERS_SHINE_MELEE_WHITELISTDEFAULTS;
						break;
					case SummonersShineDefaultSpecialWhitelistType.RANGEDNOINSTASTRIKE:
						enabledData = SUMMONERS_SHINE_RANGED_WHITELISTDEFAULTS_NOINSTASTRIKE;
						break;
					case SummonersShineDefaultSpecialWhitelistType.MELEENOINSTASTRIKE:
						enabledData = SUMMONERS_SHINE_MELEE_WHITELISTDEFAULTS_NOINSTASTRIKE;
						break;
					case SummonersShineDefaultSpecialWhitelistType.RANGEDNOMULTISHOT:
						enabledData = SUMMONERS_SHINE_RANGED_WHITELISTDEFAULTS_NOMULTISHOT;
							break;
				}
				const int ADD_FILTER = 0;
				const int DEFAULTSPECIALABILITYWHITELIST = 13;
				summonersShine.Call(ADD_FILTER, DEFAULTSPECIALABILITYWHITELIST, ItemType, enabledData);
			}
		}
		
		public static void BakeSummonersShineMinionPower_NoHooks(int ItemType, SummonersShineMinionPowerCollection minionPowers)
		{
			const int ADD_ITEM_STATICS = 2;
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
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
			const int IS_PROJECTILE_MINION_POWER_ENABLED = 13;

			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				if(!(bool)summonersShine.Call(USEFUL_FUNCS, IS_PROJECTILE_MINION_POWER_ENABLED, projectile))
					return value;
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
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				return (float)summonersShine.Call(GET_MINIONPROJECTILEDATA_VAR, projectile, GET_CURRENTTICK) == 1;
			}
			return true;
		}
		
		public static void SetSummonersShineProjMaxEnergy(int ProjectileType, float maxEnergy)
		{
			const int HOOKPROJECTILE = 1;
			const int HOOKPROJECTILEMAXENERGY = 0;
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
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
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				int minionSpeedModType = (int)summonersShine.Call(GET_MINIONPROJECTILEDATA_VAR, projectile, GET_MINIONSPEEDMODTYPE);
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
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				int minionSpeedModType = (int)summonersShine.Call(GET_MINIONPROJECTILEDATA_VAR, projectile, GET_MINIONSPEEDMODTYPE);
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
		
		public static bool GetCrossModEmblemSuperiority(Item replacer, Item old)
		{
			if (old == null)
				return true;

			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				const int GET_REWORKMINION_ITEM_VALUE = 16;
				const int PREFIXMINIONPOWER = 0;

				int useTimeDiff = replacer.useTime - old.useTime;
				if (useTimeDiff < 0)
					return true;
				if (useTimeDiff > 0)
					return false;
				int critDiff = replacer.crit - old.crit;
				if (critDiff > 0)
					return true;
				if (critDiff < 0)
					return false;
				float apDiff = (float)summonersShine.Call(GET_REWORKMINION_ITEM_VALUE, replacer, PREFIXMINIONPOWER) - (float)summonersShine.Call(GET_REWORKMINION_ITEM_VALUE, old, PREFIXMINIONPOWER);
				if (apDiff > 0)
					return true;
				if (apDiff < 0)
					return false;
			}
			return false;
		}
		public static object[] GetCrossModEmblemStats(Item item)
		{
			Mod summonersShine = null;
			int maxArray = 0;
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out summonersShine))
			{
				maxArray += 3;
			}
			object[] rv = new object[maxArray];
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
						rv[currentArrayPos] = item.useTime / (float)(compItem.useTime);
					}
					else
						rv[currentArrayPos] = 1f;
					currentArrayPos++;
					rv[currentArrayPos] = item.crit;
					currentArrayPos++;

					rv[currentArrayPos] = summonersShine.Call(GET_REWORKMINION_ITEM_VALUE, item, PREFIXMINIONPOWER);
					currentArrayPos++;
				}
				else
				{
					rv[currentArrayPos] = 1f;
					currentArrayPos++;
					rv[currentArrayPos] = 0;
					currentArrayPos++;
					rv[currentArrayPos] = 0f;
					currentArrayPos++;
				}
			}
			return rv;
		}
		
		public static void CombatPetComputeMinionStats(Projectile projectile, LeveledCombatPetModPlayer modPlayer)
		{
			int currentArrayPos = 0;
			if(modPlayer.PetModdedStats.Length == 0)
			{
				return;
			}
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				const int SET_PROJFUNCS = 4;
				const int USEFULFUNCS = 10;
				const int OVERRIDESOURCEITEM = 11;
				const int MINIONASMOD = 1;
				const int PROJECTILECRIT = 0;
				const int PREFIXMINIONPOWER = 10;
				summonersShine.Call(SET_PROJFUNCS, projectile, MINIONASMOD, modPlayer.PetModdedStats[currentArrayPos++]);
				summonersShine.Call(SET_PROJFUNCS, projectile, PROJECTILECRIT, modPlayer.PetModdedStats[currentArrayPos++]);
				summonersShine.Call(SET_PROJFUNCS, projectile, PREFIXMINIONPOWER, modPlayer.PetModdedStats[currentArrayPos]);
				summonersShine.Call(USEFULFUNCS, OVERRIDESOURCEITEM, projectile, modPlayer.PetEmblemItem);
			}
		}

		public static void CombatPetSendCrossModData(BinaryWriter writer, object[] PetModdedStats)
		{

			int currentArrayPos = 0;
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				writer.Write((float)PetModdedStats[currentArrayPos]);
				currentArrayPos++;
				writer.Write7BitEncodedInt((int)PetModdedStats[currentArrayPos]);
				currentArrayPos++;
				writer.Write((float)PetModdedStats[currentArrayPos]);
				currentArrayPos++;
			}
		}
		public static object[] CombatPetReceiveCrossModData(BinaryReader reader)
		{
			int maxArray = 0;
			Mod summonersShine = null;
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out summonersShine))
			{
				maxArray += 3;
			}
			object[] rv = new object[maxArray];
			int currentArrayPos = 0;
			if (summonersShine != null)
			{
				rv[currentArrayPos] = reader.ReadSingle();
				currentArrayPos++;
				rv[currentArrayPos] = reader.Read7BitEncodedInt();
				currentArrayPos++;
				rv[currentArrayPos] = reader.ReadSingle();
				currentArrayPos++;
			}
			return rv;
		}
		
		public static void HookBuffToItemCrossMod(int BuffType, params int[] ItemTypes)
		{
			const int MODIFYCONFIGS = 0;
			const int HOOKBUFFTOITEM = 9;
			const int HOOKBUFFCONSTS = 17;
			const int DISPLAYOVERRIDE = 1;
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				if(ItemTypes.Length == 1)
				{
					int ItemType = ItemTypes[0];
					summonersShine.Call(MODIFYCONFIGS, HOOKBUFFTOITEM, BuffType, ItemType);
				}
				else
					summonersShine.Call(HOOKBUFFCONSTS, BuffType, DISPLAYOVERRIDE, (int i) => ItemTypes);
			}
		}

		static int[] SummonersShineEmblemDisplayOverride(int BuffType)
		{
			Player player = Main.player[Main.myPlayer];
			LeveledCombatPetModPlayer playerFuncs = player.GetModPlayer<LeveledCombatPetModPlayer>();
			int rv = playerFuncs.PetEmblemItem;
			if (rv == -1)
				return null;
			return new int[] { rv };
		}

		public static void HookCombatPetBuffToEmblemSourceItem(int BuffType)
		{
			const int HOOKBUFFCONSTS = 17;
			const int DISPLAYOVERRIDE = 1;
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				summonersShine.Call(HOOKBUFFCONSTS, BuffType, DISPLAYOVERRIDE, SummonersShineEmblemDisplayOverride);
			}
		}
	}
}
