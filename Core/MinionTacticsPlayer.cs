using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;
using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using AmuletOfManyMinions.Core.Netcode.Packets;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Squires;
using AmuletOfManyMinions.UI;
using AmuletOfManyMinions.UI.TacticsUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AmuletOfManyMinions.Core.Minions
{
	/// <summary>
	/// Helper ModPlayer storing the currently selected minion tactic. Handles IO/netcode and works in tandem with the <see cref="TacticsUIMain"/>
	/// </summary>
	public class MinionTacticsPlayer : ModPlayer
	{
		//tag version, increment this if you do breaking changes to the way data is saved/loaded so backwards compat can be done. Write proper code in Load/Save to accomodate
		private const byte LatestVersion = 1;
		public static int TACTICS_GROUPS_COUNT = 3;
		public static int MAX_TACTICS_GROUP = TACTICS_GROUPS_COUNT -1;

		// The list of tactics "teams" belonging to the player
		public PlayerTargetSelectionTactic[] PlayerTacticsGroups = new PlayerTargetSelectionTactic[TACTICS_GROUPS_COUNT];
		public byte[] TacticsIDs = new byte[TACTICS_GROUPS_COUNT];

		// The active tactics group
		public int CurrentTacticGroup = 0;

		// map from minion buff to tactics group
		public Dictionary<int, int> MinionTacticsMap = new Dictionary<int, int>();
		private bool setInstancedCollections;

		public byte TacticID { get => TacticsIDs[CurrentTacticGroup]; private set => TacticsIDs[CurrentTacticGroup] = value; }


		public TargetSelectionTactic SelectedTactic => TargetSelectionTacticHandler.GetTactic(TacticID);


		public PlayerTargetSelectionTactic PlayerTactic { get => PlayerTacticsGroups[CurrentTacticGroup]; set => PlayerTacticsGroups[CurrentTacticGroup] = value; }


		private List<byte> TacticIDCycle;

		/// <summary>
		/// The list of buffs that have been altered for this modplayer since the last 
		/// sync.
		/// </summary>
		private List<int> BuffIdsToSync = new List<int>();
		/// <summary>
		/// String array used to hold on to saved tactic names until OnEnterWorld is called
		/// There seems to be a very strange interplay between Initialize and Load that causes
		/// tactics to be overwritten back and forth between them, this is the simplest way 
		/// to resolve.
		/// </summary>
		private string[] savedTacticsNames;

		/// <summary>
		/// byte array used to hold saved minion buff groups, for a similar workaround as 
		/// savedTacticsNames
		/// </summary>
		private byte[] buffsToLoad;

		private int PreviousTacticGroup = 0;
		private byte PreviousTacticID = TargetSelectionTacticHandler.DefaultTacticID;
		private bool isQuickDefending = false;

		internal bool TacticsUnlocked = false;

		/// <summary>
		/// attempt to parse out whether or not the npc index was set by a whip or squire
		/// probably not foolproof
		/// </summary>
		private int lastTargetNPC = -1;
		private int targetNPCGroup = -1;

		internal int TargetNPCGroup { get => Player.MinionAttackTargetNPC > -1 ? targetNPCGroup : -1; set => targetNPCGroup = value; }

		internal bool UsingGlobalTactics => CurrentTacticGroup == MAX_TACTICS_GROUP;

		internal bool UsingGlobalNPCTarget => TargetNPCGroup == MAX_TACTICS_GROUP;

		internal bool CurrentNPCGroupActive => TargetNPCGroup == CurrentTacticGroup || UsingGlobalNPCTarget;

		internal bool IsNPCGroupActive(int groupIdx) => TargetNPCGroup == groupIdx || UsingGlobalNPCTarget;

		// TODO localize
		private static string TacticsUnlockedText = "Minions from the Amulet of Many Minions can now use Advanced Tactics!";

		/// <summary>
		/// Timer used for syncing TacticID when it is changed on the client, only registers the last change done within SyncTimerMax ticks
		/// </summary>
		private int SyncTimer { get; set; } = 0;
		// set to true for 2 frames
		private int _updateTargetCounter;
		public bool DidUpdateAttackTarget 
		{
			get => _updateTargetCounter > 0;
			private set => _updateTargetCounter = value ? 2 : 0;
		}

		private const int SyncTimerMax = 15;

		private bool syncTactic = false;
		private bool syncConfig = false;

		/// <summary>
		/// Whether or not to choose the target selected by the current minion tactic over
		/// the target selected by the player target reticle.
		/// Set by the mod config on each player. Needs to be synced in multiplayer
		/// </summary>
		internal byte IgnoreVanillaMinionTarget = 0;
		private int targetSyncCountdown;

		/// <summary>
		/// Update every tactic for the modplayer, and also their selected tactic.
		/// Only used to update non-Main.myPlayer ModPlayers via netcode.
		/// </summary>
		/// <param name="tacticsIds"></param>
		/// <param name="selectedTactic"></param>
		internal void SetAllTactics(byte[] tacticsIds, byte? selectedTactic = null)
		{
			// don't reallocate, not sure it really matters though
			for(int i = 0; i < TACTICS_GROUPS_COUNT; i++)
			{
				TacticsIDs[i] = tacticsIds[i];
			}
			if(selectedTactic is byte selected)
			{
				CurrentTacticGroup = selected;
			}
		}
		/// <summary>
		/// Use this method to set TacticID during gameplay (through the UI, in netcode)
		/// </summary>
		/// <param name="id">The TacticID to switch to</param>
		/// <param name="fromSync">If this is called within SyncPlayer, so it doesn't send TacticPacket unnecessarily</param>
		public void SetTactic(byte id, bool fromSync = false)
		{
			if (!fromSync && TacticID != id && Main.myPlayer == Player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
			{
				SyncTimer = 1;
				syncTactic = true;
			}
			TacticID = id;
			PlayerTactic = SelectedTactic.CreatePlayerTactic();
		}

		public void SetTacticsGroup(int index)
		{
			CurrentTacticGroup = index;
			// also update the tactics panel, this is a bit of a hacky way to go about
			UserInterfaces.tacticsUI.SetSelected(TacticID, CurrentTacticGroup);
		}

		public override void Initialize()
		{
			CurrentTacticGroup = 0;
			SyncTimer = 0;
			TacticIDCycle = new List<byte>
			{
				TargetSelectionTacticHandler.GetTactic<ClosestEnemyToMinion>().ID,
				TargetSelectionTacticHandler.GetTactic<ClosestEnemyToPlayer>().ID,
				TargetSelectionTacticHandler.GetTactic<StrongestEnemy>().ID,
				TargetSelectionTacticHandler.GetTactic<WeakestEnemy>().ID,
				TargetSelectionTacticHandler.GetTactic<LeastDamagedEnemy>().ID,
				TargetSelectionTacticHandler.GetTactic<MostDamagedEnemy>().ID,
				TargetSelectionTacticHandler.GetTactic<SpreadOut>().ID,
				TargetSelectionTacticHandler.GetTactic<AttackGroups>().ID,
			};
		}


		public override void SaveData(TagCompound tag)
		{
			string tacticsNameList = string.Join(",", TacticsIDs.Select(id => TargetSelectionTacticHandler.GetTactic(id).Name));
			// 256 is probably generous enough to not require any resizing in most cases
			MemoryStream stream = new MemoryStream(256);
			MinionTacticsGroupMapper.WriteBuffMap(new BinaryWriter(stream), MinionTacticsMap);
			byte[] buffsToWrite = stream.ToArray();

			TagCompound tacticsTag = new TagCompound
			{
				{ "v", (byte)LatestVersion },
				{ "names", tacticsNameList },
				{ "unlocked", TacticsUnlocked },
				{ "minionGroups", buffsToWrite }
			};

			tag.Add("tactic", tacticsTag);
		}

		public override void LoadData(TagCompound tag)
		{
			TagCompound tacticsTag = tag.Get<TagCompound>("tactic");

			byte tacticVersion = tacticsTag.GetByte("v");
			//Do special logic here if the system changes drastically (used for backwards compat)
			if(tacticsTag.ContainsKey("unlocked"))
			{
				TacticsUnlocked = tacticsTag.GetBool("unlocked");
			} else
			{
				TacticsUnlocked = false;
			}
			if (tacticVersion == 0)
			{
				string tacticName = tacticsTag.GetString("name");
				TacticID = TargetSelectionTacticHandler.GetTactic(tacticName).ID;
			} else
			{
				// extremely odd class of bug here where load is called multiple times, try to 
				// make things as idempotent as possible
				if(tacticsTag.ContainsKey("names"))
				{
					savedTacticsNames = tacticsTag.GetString("names").Split(',');
				}
				if(tacticsTag.ContainsKey("minionGroups") && MinionTacticsMap.Count == 0)
				{
					buffsToLoad = tacticsTag.GetByteArray("minionGroups");
				}
			}
		}

		// set collections per-mod player
		private void SetInstancedCollections()
		{
			PlayerTacticsGroups = new PlayerTargetSelectionTactic[TACTICS_GROUPS_COUNT];
			TacticsIDs = new byte[TACTICS_GROUPS_COUNT];
			CurrentTacticGroup = 0;
			MinionTacticsMap = new Dictionary<int, int>();

			// this needs to be late-initialized for some reason
			if(savedTacticsNames != null)
			{
				for(int i = 0; i < TACTICS_GROUPS_COUNT; i++)
				{
					TacticsIDs[i] = TargetSelectionTacticHandler.GetTactic(savedTacticsNames[i]).ID;
				}
			}
			if(buffsToLoad != null)
			{
				MemoryStream savedMinionsStream = new MemoryStream(buffsToLoad);
				MinionTacticsGroupMapper.ReadBuffMap(new BinaryReader(savedMinionsStream), MinionTacticsMap);
			}
			for(int i = 0; i < TACTICS_GROUPS_COUNT; i++)
			{
				CurrentTacticGroup = i;
				PlayerTactic = SelectedTactic.CreatePlayerTactic();
			}
			CurrentTacticGroup = 0;
			setInstancedCollections = true;

		}
		public override void OnEnterWorld(Player player)
		{
			if (Main.netMode == NetmodeID.Server) return; //Safety check, this hook shouldn't run serverside anyway
			UserInterfaces.tacticsUI.detached = false;
			if(!TacticsUnlocked)
			{
				UserInterfaces.tacticsUI.SetOpenClosedState(OpenedTriState.HIDDEN);
			} else if (UserInterfaces.tacticsUI.opened == OpenedTriState.HIDDEN) 
			{
				UserInterfaces.tacticsUI.SetOpenClosedState(OpenedTriState.FALSE);
			}
			SetInstancedCollections();
			UserInterfaces.tacticsUI.SetSelected(TacticID, CurrentTacticGroup);
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			//This hook is ran on the player that enters in two cases:
			//1. another player joins, so it has to send this players info to it
			//    * The server simply sends the data this player has to the joined player
			//2. this player joins, having to send its info to everyone
			//    * The client sends his info to the server
			//    * Server sends that info to all other players (note that the method is called again, the packet itself doesn't broadcast)
			new SyncMinionTacticsPlayerPacket(this).Send(toWho, fromWho);
		}

		public override void PreUpdate()
		{
			if(Main.myPlayer == Player.whoAmI)
			{
				byte newIgnoreTargetStatus =(byte)(ClientConfig.Instance.IgnoreVanillaTargetReticle ? 1 : 0);
				if(IgnoreVanillaMinionTarget != newIgnoreTargetStatus)
				{
					syncConfig = true;
					SyncTimer = Math.Max(SyncTimer, 1);
				}
				IgnoreVanillaMinionTarget = newIgnoreTargetStatus;
			} else if (!setInstancedCollections)
			{
				SetInstancedCollections();
			}
			//Client sends his newly selected tactic after a small delay
			if (Main.myPlayer == Player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
			{
				if (SyncTimer > 0)
				{
					SyncTimer++; //If it has been set to 1, increment
					if (SyncTimer > SyncTimerMax)
					{
						SyncTimer = 0; //Stop timer from incrementing
						if(syncTactic)
						{
							new TacticPacket(Player, TacticsIDs).Send();
							syncTactic = false;
						}
						if(syncConfig)
						{
							new ConfigPacket(Player, IgnoreVanillaMinionTarget).Send();
							syncConfig = false;
						}
						if(BuffIdsToSync.Count > 0)
						{
							Dictionary<int, int> buffsToSend = BuffIdsToSync.ToDictionary(k => k, k => MinionTacticsMap[k]);
							new MinionGroupsPacket(Player, buffsToSend).Send();
							BuffIdsToSync.Clear();
						}
					}
				}
			}
			// should do some pruning here
			for(int i = 0; i < TACTICS_GROUPS_COUNT; i++)
			{
				PlayerTacticsGroups[i]?.PreUpdate();
				PlayerTacticsGroups[i]?.UpdatePlayerAdjacentNPCs(Player);
			}
		}

		public override void PostUpdate()
		{
			CheckForAoMMItem();
			CheckPlayerAttackTarget();
			// should do some pruning here
			for(int i = 0; i < TACTICS_GROUPS_COUNT; i++)
			{
				PlayerTacticsGroups[i]?.PostUpdate();
			}
		}

		private bool ProjectileSetsWaypoint(int projType)
		{
			bool usingWhip = ServerConfig.Instance.WhipsSetWaypoint &&
				ProjectileID.Sets.IsAWhip[projType];
			bool usingSquire = ServerConfig.Instance.SquiresSetWaypoint &&
				SquireMinionTypes.Contains(projType);
			bool usingSquireProj = ServerConfig.Instance.SquireProjSetWaypoint &&
				SquireGlobalProjectile.isSquireShot.Contains(projType);
			return usingWhip || usingSquire || usingSquireProj;
		}


		internal void CheckPlayerAttackTarget()
		{
			// check if the waypoint was changed this frame, and if it was 
			// set via a whip/Squire
			// need to let other modplayers know, a bit clunky
			_updateTargetCounter--;
			targetSyncCountdown--;
			// MP-safe code
			if(Player.MinionAttackTargetNPC != lastTargetNPC && ProjectileSetsWaypoint(Player.HeldItem.shoot))
			{
				AddPlayerAttackTarget();
				DidUpdateAttackTarget = true;
			}
			lastTargetNPC = Player.MinionAttackTargetNPC;
		}

		internal void RemovePlayerAttackTarget(int groupIdx)
		{
			if(groupIdx == TargetNPCGroup)
			{
				Player.MinionAttackTargetNPC = -1;
				TargetNPCGroup = -1;
			} else if (UsingGlobalNPCTarget)
			{
				TargetNPCGroup = groupIdx == 0 ? 1 : 0;
			}
		}

		internal void AddPlayerAttackTarget()
		{
			if(TargetNPCGroup == -1 || TargetNPCGroup == CurrentTacticGroup)
			{
				TargetNPCGroup = CurrentTacticGroup;
			} else
			{
				TargetNPCGroup = MAX_TACTICS_GROUP;
			}
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
		{
			if(ProjectileSetsWaypoint(proj.type))
			{
				// make sure a waypoint can still be used if it was previously
				// set incorrectly
				if(!CurrentNPCGroupActive || target.whoAmI != Player.MinionAttackTargetNPC)
				{
					AddPlayerAttackTarget();
					DidUpdateAttackTarget = true;
				}
				// redundant for whips
				Player.MinionAttackTargetNPC = target.whoAmI;
				// should only call on main player, make sure not to spam
				if(Player.whoAmI == Main.myPlayer && targetSyncCountdown < 0)
				{
					targetSyncCountdown = 15;
					new TargetNPCWaypointPacket(Player, target.whoAmI, (byte)CurrentTacticGroup).Send();
				}
			}
		}

		/// <summary>
		/// Only called for other clients' players, sets the MinionAttackTargetNPC values in cases
		/// where syncing is not done automatically (I have yet to determine when exactly these are)
		/// </summary>
		/// <param name="npcIdx"></param>
		internal void UpdateTargetNPCFromPacket(int npcIdx, byte groupIdx)
		{
			Player.MinionAttackTargetNPC = npcIdx;
			CurrentTacticGroup = groupIdx;
			AddPlayerAttackTarget();
		}

		internal int GetGroupForBuff(int buffId)
		{
			int groupForMinion;
			if(!MinionTacticsMap.ContainsKey(buffId))
			{
				int defaultTactic = UsingGlobalTactics ? 0 : CurrentTacticGroup;
				MinionTacticsMap[buffId] = defaultTactic;
				SetGroupForMinion(defaultTactic, buffId);
				groupForMinion = defaultTactic;
			} else
			{
				groupForMinion = MinionTacticsMap[buffId];
			}
			return groupForMinion;

		}
		internal int GetGroupForMinion(Minion minion)
		{
			return GetGroupForBuff(minion.BuffId);
		}

		internal void SetGroupForMinion(int groupId, int minionBuffId)
		{
			if (Main.myPlayer == Player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
			{
				SyncTimer = 1;
				BuffIdsToSync.Add(minionBuffId);
			}
			MinionTacticsMap[minionBuffId] = groupId;
		}

		internal bool GroupIsSetForMinion(int minionBuffId)
		{
			return MinionTacticsMap.ContainsKey(minionBuffId);
		}
		public PlayerTargetSelectionTactic GetTacticForMinion(Minion minion)
		{
			if(isQuickDefending || UsingGlobalTactics)
			{
				return PlayerTacticsGroups[MAX_TACTICS_GROUP];
			}
			return PlayerTacticsGroups[GetGroupForMinion(minion)];
		}

		private void CheckForAoMMItem()
		{
			// only check once per second on the client player
			if(Player.whoAmI != Main.myPlayer || TacticsUnlocked || Main.GameUpdateCount % 60 != 0)
			{
				return;
			}
			foreach(Item item in Player.inventory)
			{
				if(item.type == ItemID.None || item.ModItem == null)
				{
					continue;
				}
				if(item.ModItem?.Mod== Mod && item.CountsAsClass<SummonDamageClass>())
				{
					TacticsUnlocked = true;
					break;
				}
			}
			if(TacticsUnlocked)
			{
				UserInterfaces.tacticsUI.SetOpenClosedState(OpenedTriState.TRUE);
			}

		}
		private void CycleTactic()
		{
			int nextTacticIndex = (TacticIDCycle.IndexOf(TacticID) + 1) % TacticIDCycle.Count;
			SetTactic(TacticIDCycle[nextTacticIndex]);
		}

		private void CycleTacticsGroup()
		{
			SetTacticsGroup((CurrentTacticGroup + 1) % TACTICS_GROUPS_COUNT);
		}

		private void StartQuickDefending()
		{
			isQuickDefending = true;
			PreviousTacticGroup = CurrentTacticGroup;
			// switch to a consistent tactics group so that we don't get into any weird states
			// while cycling through tactics groups during quick tactics
			PreviousTacticID = TacticID;
			SetTacticsGroup(MAX_TACTICS_GROUP);
			SetTactic(TargetSelectionTacticHandler.GetTactic<ClosestEnemyToPlayer>().ID);
			UserInterfaces.tacticsUI.SetSelected(TacticID, CurrentTacticGroup);
		}

		private void StopQuickDefending()
		{
			isQuickDefending = false;
			SetTactic(PreviousTacticID);
			SetTacticsGroup(PreviousTacticGroup);
			UserInterfaces.tacticsUI.SetSelected(TacticID, CurrentTacticGroup);
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			// this is probably only run client side, but to be safe
			if(Player.whoAmI != Main.myPlayer || !TacticsUnlocked)
			{
				return;
			}
			if(AmuletOfManyMinions.CycleTacticHotKey.JustPressed)
			{
				CycleTactic();
				UserInterfaces.tacticsUI.SetSelected(TacticID, CurrentTacticGroup);
			}
			
			if(AmuletOfManyMinions.CycleTacticsGroupHotKey.JustPressed)
			{
				CycleTacticsGroup();
				UserInterfaces.tacticsUI.SetSelected(TacticID, CurrentTacticGroup);
			}

			if(AmuletOfManyMinions.QuickDefendHotKey.JustPressed )
			{
				if(ClientConfig.Instance.QuickDefendHotkeyStyle == ClientConfig.QuickDefendToggle)
				{
					if(isQuickDefending)
					{
						StopQuickDefending();
					} else
					{
						StartQuickDefending();
					}
				} else
				{
					StartQuickDefending();
				}
			} else if (AmuletOfManyMinions.QuickDefendHotKey.JustReleased && ClientConfig.Instance.QuickDefendHotkeyStyle == ClientConfig.QuickDefendHold)
			{
				StopQuickDefending();
			}
		}

	}
}
