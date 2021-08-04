using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Tactics;
using System.Collections.Generic;
using System.IO;
using Terraria;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	public class SyncMinionTacticsPlayerPacket : PlayerPacket
	{
		readonly byte[] tacticsIds;
		readonly byte selectedTactic;
		readonly Dictionary<int, int> tacticsMap;
		readonly byte ignoreTargetReticle;
		//mirror more fields here that have to be synced on join for MinionTacticsPlayer

		public SyncMinionTacticsPlayerPacket() { }

		public SyncMinionTacticsPlayerPacket(MinionTacticsPlayer player) : base(player.Player)
		{
			tacticsIds = player.TacticsIDs;
			selectedTactic = (byte)player.CurrentTacticGroup;
			tacticsMap = player.MinionTacticsMap;
			ignoreTargetReticle = player.IgnoreVanillaMinionTarget;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write(tacticsIds);
			writer.Write(selectedTactic);
			writer.Write(ignoreTargetReticle);
			MinionTacticsGroupMapper.WriteBuffMap(writer, tacticsMap);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			MinionTacticsPlayer minionTacticsPlayer = player.GetModPlayer<MinionTacticsPlayer>();
			byte[] tacticsIds = reader.ReadBytes(MinionTacticsPlayer.TACTICS_GROUPS_COUNT);
			byte selectedTactic = reader.ReadByte();
			byte ignoreTargetReticle = reader.ReadByte();
			// reading rest of packet directly into dict
			MinionTacticsGroupMapper.ReadBuffMap(reader, minionTacticsPlayer.MinionTacticsMap);

			minionTacticsPlayer.SetAllTactics(tacticsIds, selectedTactic);
			minionTacticsPlayer.IgnoreVanillaMinionTarget = ignoreTargetReticle;

		}
	}
}
