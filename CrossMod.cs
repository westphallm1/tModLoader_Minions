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
	class CrossMod
	{
		public static bool SummonersAssociationLoaded { get; private set; }

		public static HashSet<int> MinionBuffTypes;
		public static void Unload()
		{
			MinionBuffTypes = null;
		}

		public static bool SharknadoFunc(Projectile p)
		{
			return !((SharknadoMinion)p.modProjectile).isBeingUsedAsToken;
		}

		public static void AddSummonersAssociationMetadata()
		{
			MinionBuffTypes = new HashSet<int>();
			if (ModLoader.GetMod("SummonersAssociation") is Mod summonersAssociation && summonersAssociation.Version >= new Version(0, 4, 7))
			{
				SummonersAssociationLoaded = true;
				//1. Collect all "EmpoweredMinion" that have a valid CounterType: WORKS FOR ALL EMPOWERED MINIONS (but also includes some "regular" minions which is unintended)
				//var empoweredMinionsWithCounterType = this.GetContent<ModProjectile>().OfType<EmpoweredMinion>().Where(e => e.CounterType > ProjectileID.None).ToList();
				//var counterTypes = empoweredMinionsWithCounterType.Select((e) => e.CounterType).ToHashSet();

				//2. Collect all "CounterMinion": DOES NOT WORK FOR ALL EMPOWERED MINIONS (those that use "regular" minions for counts), but covers all 100% safe ones
				// not sure what the 1.3 equivalent of GetContent is, use reflection instead
				// Also blacklist squires just for good measure
				Type counterMinionType = typeof(CounterMinion);
				Type squireType = typeof(SquireMinion);
				IEnumerable<Type> blacklistedTypes = Assembly.GetExecutingAssembly().GetTypes()
					.Where(t => !t.IsAbstract && (t.IsSubclassOf(counterMinionType) || t.IsSubclassOf(squireType)));

				//Empowered minion "counter" projectiles should not teleport
				var methodRef = typeof(ModContent).GetMethod("ProjectileType", BindingFlags.Public | BindingFlags.Static);
				foreach (var blacklistedType in blacklistedTypes)
				{
					var genericRef = methodRef.MakeGenericMethod(blacklistedType);
					summonersAssociation.Call("AddTeleportConditionMinion", genericRef.Invoke(null, null));
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

		public static void PopulateSummonersAssociationBuffSet(Mod mod)
		{
			Version minSupportedVersion = new Version(0, 4, 7);
			if (ModLoader.GetMod("SummonersAssociation") is Mod summonersAssociation && summonersAssociation.Version >= minSupportedVersion)
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

	}
}
