using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets
{
	internal struct CombatPetLevelInfo
	{
		internal int Level;
		internal int BaseDamage; // The base damage done by combat pets at this level of progression
		internal int BaseSearchRange; // The base distance combat pets will seek from the player 
		internal float BaseSpeed; // How the AI actually uses speed varies quite a bit from type to type ...
		internal int[] BossNPCIds; // The NPC(s) that must die for the level up to take effect
		internal Func<int, bool> ShouldUnlock;

		public CombatPetLevelInfo(int level, int damage, int searchRange, int baseSpeed, params int[] bossIds)
		{
			Level = level;
			BaseDamage = damage;
			BaseSearchRange = searchRange;
			BaseSpeed = baseSpeed;
			BossNPCIds = bossIds;
			ShouldUnlock = npcId => true;
		}

		public CombatPetLevelInfo(int level, int damage, int searchRange, int baseSpeed, Func<int, bool> shouldUnlock, params int[] bossIds)
		{
			Level = level;
			BaseDamage = damage;
			BaseSearchRange = searchRange;
			BaseSpeed = baseSpeed;
			BossNPCIds = bossIds;
			ShouldUnlock = shouldUnlock;
		}
	}
	class CombatPetUtils : ModSystem
	{
		internal static CombatPetLevelInfo[] PetLevelTable;

		internal bool ShouldMechBossesUnlock(int npcId)
		{
			if(npcId == NPCID.Retinazer)
			{
				return NPC.CountNPCS(NPCID.Spazmatism) == 0;
			} else if (npcId == NPCID.Spazmatism)
			{
				return NPC.CountNPCS(NPCID.Retinazer) == 0;
			} else
			{
				return true;
			}
		}

		public override void Load()
		{
			PetLevelTable = new CombatPetLevelInfo[]{
				new(0, 6, 500, 8), // pre-boss
				new(1, 8, 525, 8, NPCID.KingSlime),
				new(2, 10, 550, 8, NPCID.EyeofCthulhu),
				new(3, 12, 600, 8, NPCID.EaterofWorldsHead, NPCID.BrainofCthulhu), // TODO check this only triggers on killing the full EoW
				new(4, 14, 625, 9, NPCID.QueenBee),
				new(5, 18, 650, 9, NPCID.SkeletronHead),
				new(6, 28, 750, 12, NPCID.WallofFlesh),
				// TODO make sure twins are both properly dead
				new(7, 32, 750, 14, ShouldMechBossesUnlock, NPCID.TheDestroyer, NPCID.SkeletronPrime, NPCID.Retinazer, NPCID.Spazmatism),
				new(8, 40, 850, 14, NPCID.Plantera),
				new(9, 42, 900, 15, NPCID.DukeFishron),
				new(10, 48, 950, 15, NPCID.CultistBoss),
				new(11, 75, 1100, 16, NPCID.MoonLordCore)
			};
		}

		public override void Unload()
		{
			base.Unload();
		}

		// used for spawning combat pets from the pet buff
		public static void SpawnIfAbsent(Player player, int buffIdx, int projType, int damage, float knockback = 0.5f)
		{
			if(player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0)
			{
				var p = Projectile.NewProjectileDirect(player.GetProjectileSource_Buff(buffIdx), player.Center, Vector2.Zero, projType, damage, knockback, player.whoAmI);
				p.originalDamage = damage;
			}
		}
	}

	class LeveledCombatPetModPlayer : ModPlayer
	{
		// used for handling changes to TagCompound format across udpates
		private const int LatestVersion = 1;
		internal int PetLevel { get; set; }

		internal CombatPetLevelInfo PetLevelInfo => CombatPetUtils.PetLevelTable[PetLevel];


		public void UpdatePetLevel(int newLevel, bool fromSync = false)
		{
			if(newLevel > PetLevel)
			{
				PetLevel = newLevel;
				if(!fromSync)
				{
					// TODO MP packet
				}
			}
		}

		public override TagCompound Save()
		{
			TagCompound tag = new TagCompound();
			TagCompound levelTag = new TagCompound
			{
				{"v", (byte)LatestVersion },
				{"level", PetLevel }
			};
			tag.Add("petLevel", levelTag);
			return tag;
		}

		public override void Load(TagCompound tag)
		{
			TagCompound petLevelTag = tag.Get<TagCompound>("petLevel");
			byte petLevelVersion = petLevelTag.GetByte("v");
			if(petLevelVersion > 0)
			{
				if(petLevelTag.ContainsKey("level"))
				{
					PetLevel = petLevelTag.GetInt("level");
				}
			}
			// failsafe
			if(PetLevel < 0)
			{
				PetLevel = 0;
			}
		}
	}

	public class CombatPetLevelUpGlobalNPC : GlobalNPC
	{
		/// <summary>
		/// Level up the player's combat pets upon defeating certain bosses
		/// </summary>
		/// <param name="npc"></param>
		public override void OnKill(NPC npc)
		{
			// TODO may need to be synced separately
			base.OnKill(npc);
			if(CombatPetUtils.PetLevelTable.Where(pl=>pl.BossNPCIds.Contains(npc.type) && pl.ShouldUnlock(npc.type)).FirstOrDefault()
				is CombatPetLevelInfo info)
			{
				for(int i = 0; i < Main.maxPlayers; i++)
				{
					Player p = Main.player[i];
					if(p.active && Vector2.DistanceSquared(p.Center, npc.Center) < 2000f * 2000f)
					{
						p.GetModPlayer<LeveledCombatPetModPlayer>().UpdatePetLevel(info.Level);
					}
				}
			}
		}
	}

	public abstract class CombatPetMinionItem<TBuff, TProj> : VanillaCloneMinionItem<TBuff, TProj> where TBuff: ModBuff where TProj: Minion
	{
		public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ApplyBuff(player);
			return false;
		}
	}
}
