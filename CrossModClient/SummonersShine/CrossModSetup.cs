using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.Pirate;
using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.CrossModClient.SummonersShine
{
	internal class CrossModSetup
	{
		public static bool SummonersShineLoaded { get; private set; }
		/// <summary>
		/// Blacklist problem projectiles from summoner's shine's globalitems
		/// </summary>
		public static void AddSummonersShineMetadata(Mod mod)
		{
			const int ADD_FILTER = 0;
			const int THOUGHTBUBBLE = 3;
			const int BLACKLIST_PROJECTILE = 1;
			const int DONT_COUNT_AS_MINION = 4;
			const int COUNTS_AS_WHIP_FOR_INSTASTRIKE = 14;
			const int SET_SUMMON_MINION_WEAPON_STAT_SOURCE = 15;
			const int SET_SUMMON_WEAPON_STAT_SOURCE_MINION = 16;
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
					
				IEnumerable<ModProjectile> empoweredMinions = mod.GetContent<ModProjectile>().Where(p => p is EmpoweredMinion);
				
				foreach (var minion in empoweredMinions)
				{
					//set stat source

					EmpoweredMinion empoweredMinion = minion as EmpoweredMinion;

					ModItem[] modItemArray = mod.GetContent<ModItem>().Where(i => i.Item.shoot == empoweredMinion.CounterType).ToArray();
					if (modItemArray.Length > 0)
					{
						int ItemType = modItemArray[0].Type;

						summonersShine.Call(ADD_FILTER, SET_SUMMON_MINION_WEAPON_STAT_SOURCE, minion.Type, ItemType);
						summonersShine.Call(ADD_FILTER, SET_SUMMON_WEAPON_STAT_SOURCE_MINION, ItemType, new int[] { empoweredMinion.CounterType });
					}
				}
				
				IEnumerable<ModItem> squireItems = mod.GetContent<ModItem>().Where(p => p is SquireMinionItemDetector);
				foreach (var squireItem in squireItems)
				{
					summonersShine.Call(ADD_FILTER, COUNTS_AS_WHIP_FOR_INSTASTRIKE, squireItem.Type);
				}

				BakeAoMMVersionSpecialAbilities(summonersShine);

				summonersShine.Call(THOUGHTBUBBLE, CrossModClient.SummonersShine.Bubble.SummonersShine_GetEnergyThoughtTexture);
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
		
		public static void BakeAoMMVersionSpecialAbilities(Mod summonersShine)
		{
            const int SET_CONFIG = 0;
            const int SET_SPECIAL_ITEM_DUPE = 18;
            const int SET_SPECIAL_PROJ_DUPE = 19;

			//summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<AbigailMinionItem>(), ItemID.AbigailsFlower);
			//summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<AbigailCounterMinion>(), ProjectileID.AbigailCounter);
			//summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<AbigailMinion>(), ProjectileID.AbigailMinion);

			summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<BabyFinchMinionItem>(), ItemID.BabyBirdStaff);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<BabyFinchMinion>(), ProjectileID.BabyBird);

			summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<BabySlimeMinionItem>(), ItemID.SlimeStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<BabySlimeMinion>(), ProjectileID.BabySlime);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<FlinxMinionItem>(), ItemID.FlinxStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<FlinxMinion>(), ProjectileID.FlinxMinion);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<VampireFrogMinionItem>(), ItemID.VampireFrogStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<VampireFrogMinion>(), ProjectileID.VampireFrog);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<HornetMinionItem>(), ItemID.HornetStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<HornetMinion>(), ProjectileID.Hornet);

			//summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<ImpMinionItem>(), ItemID.ImpStaff);
			//summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<ImpMinion>(), ProjectileID.FlyingImp);

			summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<SpiderMinionItem>(), ItemID.SpiderStaff);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<DangerousSpiderMinion>(), ProjectileID.DangerousSpider);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<JumperSpiderMinion>(), ProjectileID.JumperSpider);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<VenomSpiderMinion>(), ProjectileID.VenomSpider);

			summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<PirateMinionItem>(), ItemID.PirateStaff);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<PirateMinion>(), ProjectileID.SoulscourgePirate);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<PirateDeadeyeMinion>(), ProjectileID.OneEyedPirate);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<ParrotMinion>(), ProjectileID.OneEyedPirate);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<FlyingDutchmanMinion>(), ProjectileID.OneEyedPirate);

			summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<EnchantedDaggerMinionItem>(), ItemID.Smolstar);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<EnchantedDaggerMinion>(), ProjectileID.Smolstar);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<SanguineBatMinionItem>(), ItemID.SanguineStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<SanguineBatMinion>(), ProjectileID.BatOfLight);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<SharknadoMinionItem>(), ItemID.TempestStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<SharknadoMinion>(), ProjectileID.Tempest);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<TwinsMinionItem>(), ItemID.OpticStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<MiniRetinazerMinion>(), ProjectileID.Retanimini);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<MiniSpazmatismMinion>(), ProjectileID.Spazmamini);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<PygmyMinionItem>(), ItemID.PygmyStaff);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<Pygmy1Minion>(), ProjectileID.Pygmy);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<Pygmy2Minion>(), ProjectileID.Pygmy2);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<Pygmy3Minion>(), ProjectileID.Pygmy3);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<Pygmy4Minion>(), ProjectileID.Pygmy4);

			summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<DeadlySphereMinionItem>(), ItemID.DeadlySphereStaff);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<DeadlySphereMinion>(), ProjectileID.DeadlySphere);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<DeadlySphereFireMinion>(), ProjectileID.DeadlySphere);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<DeadlySphereClingerMinion>(), ProjectileID.DeadlySphere);

			summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<RavenMinionItem>(), ItemID.RavenStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<RavenMinion>(), ProjectileID.Raven);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<UFOMinionItem>(), ItemID.XenoStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<UFOMinion>(), ProjectileID.UFOMinion);

            summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<StardustCellMinionItem>(), ItemID.StardustCellStaff);
            summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<StardustCellMinion>(), ProjectileID.StardustCellMinion);

			summonersShine.Call(SET_CONFIG, SET_SPECIAL_ITEM_DUPE, ModContent.ItemType<StardustDragonMinionItem>(), ItemID.StardustDragonStaff);
			summonersShine.Call(SET_CONFIG, SET_SPECIAL_PROJ_DUPE, ModContent.ProjectileType<StardustDragonMinion>(), ProjectileID.StardustDragon1);
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


		public static void BakeSummonersShineMinionPower_WithHooks(int ItemType, int ProjectileType, int rechargeTime, SummonersShineMinionPowerCollection minionPowers,
			Action<Projectile, Entity, int, bool> MinionOnSpecialAbilityUsed,
			Action<Projectile, Player> MinionTerminateSpecialAbility = null,
			Func<Player, Vector2, Entity> SpecialAbilityFindTarget = null,
			Func<Player, int, List<Projectile>, List<Projectile>> SpecialAbilityFindMinions = null
			)
		{
			const int PROJ_STATICS = 1;
			const int MAXENERGY = 0;
			const int ONSPECIALABILUSED = 4;
			const int TERMINATESPECIAL = 5; //for config changes
			const int ADD_ITEM_STATICS = 2;
			if (!ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				if (SpecialAbilityFindTarget == null)
					SpecialAbilityFindTarget = SummonersShine_SpecialAbilityFindTarget;
				if (SpecialAbilityFindMinions == null)
					SpecialAbilityFindMinions = SummonersShine_SpecialAbilityFindMinions;

				summonersShine.Call(ADD_ITEM_STATICS, ItemType, SpecialAbilityFindTarget, SpecialAbilityFindMinions, minionPowers.BakeToTupleArray(), rechargeTime, true);

				summonersShine.Call(PROJ_STATICS, ProjectileType, ONSPECIALABILUSED, MinionOnSpecialAbilityUsed);
				if (MinionTerminateSpecialAbility != null)
					summonersShine.Call(PROJ_STATICS, ProjectileType, TERMINATESPECIAL, MinionTerminateSpecialAbility);
				summonersShine.Call(PROJ_STATICS, ProjectileType, MAXENERGY, (float)rechargeTime);
			}
		}

		internal static Entity SummonersShine_SpecialAbilityFindTarget(Player player, Vector2 mousePos)
		{
			int num = -1;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				bool flag = Main.npc[i].CanBeChasedBy(player, false) && Main.npc[i].Hitbox.Distance(player.Center) <= 1400 && (num == -1 || Main.npc[i].Hitbox.Distance(mousePos) < Main.npc[num].Hitbox.Distance(mousePos));
				if (flag)
				{
					num = i;
				}
			}
			if (num == -1)
				return null;
			return Main.npc[num];
		}
		internal static List<Projectile> SummonersShine_SpecialAbilityFindMinions(Player player, int itemType, List<Projectile> valid) { return valid; }
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
		public static bool GetSummonersShineIsCastingSpecialAbility(Projectile projectile, int SourceItemID)
		{
			const int USEFULFUNCS = 10;
			const int ISCASTINGSPECIAL = 8;
			if (SummonersShineLoaded && !ServerConfig.Instance.DisableSummonersShineAI && ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				return (bool)summonersShine.Call(USEFULFUNCS, ISCASTINGSPECIAL, projectile, SourceItemID);
			}
			return false;
		}
	}
}
