using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using AmuletOfManyMinions.Core.Netcode.Packets;
using AmuletOfManyMinions.UI;
using AmuletOfManyMinions.UI.TacticsUI;
using Terraria;
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
		private const byte LatestVersion = 0;

		public byte TacticID { get; private set; } = TargetSelectionTacticHandler.DefaultTacticID;

		public TargetSelectionTactic SelectedTactic => TargetSelectionTacticHandler.GetTactic(TacticID);

		/// <summary>
		/// Timer used for syncing TacticID when it is changed on the client, only registers the last change done within SyncTimerMax ticks
		/// </summary>
		private int SyncTimer { get; set; } = 0;

		private const int SyncTimerMax = 15;

		/// <summary>
		/// Use this method to set TacticID during gameplay (through the UI, in netcode)
		/// </summary>
		/// <param name="id">The TacticID to switch to</param>
		/// <param name="fromSync">If this is called within SyncPlayer, so it doesn't send TacticPacket unnecessarily</param>
		public void SetTactic(byte id, bool fromSync = false)
		{
			if (!fromSync && TacticID != id && Main.myPlayer == player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
			{
				SyncTimer = 1;
			}
			TacticID = id;
		}

		public override void Initialize()
		{
			TacticID = TargetSelectionTacticHandler.GetTactic(TacticID).ID; //Safe conversion of default tactic
			SyncTimer = 0;
		}

		public override TagCompound Save()
		{
			TagCompound tag = new TagCompound();

			TagCompound tacticsTag = new TagCompound
			{
				{ "v", (byte)LatestVersion },
				{ "name", SelectedTactic.Name } //Important to save the name and not the ID as its dynamic. Names are the actual "identifiers"
			};

			tag.Add("tactic", tacticsTag);

			return tag;
		}

		public override void Load(TagCompound tag)
		{
			TagCompound tacticsTag = tag.Get<TagCompound>("tactic");

			byte tacticVersion = tacticsTag.GetByte("v");
			//Do special logic here if the system changes drastically (used for backwards compat)

			if (tacticVersion == 0)
			{
				string tacticName = tacticsTag.GetString("name");

				TacticID = TargetSelectionTacticHandler.GetTactic(tacticName).ID;
			}
		}

		public override void OnEnterWorld(Player player)
		{
			if (Main.netMode == NetmodeID.Server) return; //Safety check, this hook shouldn't run serverside anyway
			UserInterfaces.tacticsUI.SetSelected(TacticID);
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			//This hook is ran on the player that enters in two cases:
			//1. another player joins, so it has to send this players info to it
			//    * The server simply sends the data this player has to the joined player
			//2. this player joins, having to send its info to everyone
			//    * The client sends his info to the server
			//    * Server sends that info to all other players (note that the method is called again, the packet itself doesn't broadcast)
			new SyncMinionTacticsPlayerPacket(player, TacticID).Send(toWho, fromWho);
		}

		public override void PreUpdate()
		{
			//Client sends his newly selected tactic after a small delay
			if (Main.myPlayer == player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
			{
				if (SyncTimer > 0)
				{
					SyncTimer++; //If it has been set to 1, increment
					if (SyncTimer > SyncTimerMax)
					{
						SyncTimer = 0; //Stop timer from incrementing
						new TacticPacket(player, TacticID).Send();
					}
				}
			}
		}
	}
}
