using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Tactics;
using System.Collections.Generic;
using System.IO;
using Terraria;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	public class SyncMinionTacticsPlayerPacket : PlayerPacket
	{
		readonly byte[] tacticIDByGroup;
		readonly byte currentTacticGroup;
		readonly Dictionary<int, int> tacticsMap;
		readonly byte ignoreTargetReticle;
		//mirror more fields here that have to be synced on join for MinionTacticsPlayer

		public SyncMinionTacticsPlayerPacket() { }

		public SyncMinionTacticsPlayerPacket(MinionTacticsPlayer player) : base(player.Player)
		{
			tacticIDByGroup = player.TacticIDByGroup;
			currentTacticGroup = (byte)player.CurrentTacticGroup;
			tacticsMap = player.MinionTacticsMap;
			ignoreTargetReticle = player.IgnoreVanillaMinionTarget;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write(tacticIDByGroup);
			writer.Write(currentTacticGroup);
			writer.Write(ignoreTargetReticle);
			MinionTacticsGroupMapper.WriteBuffMap(writer, tacticsMap);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			MinionTacticsPlayer minionTacticsPlayer = player.GetModPlayer<MinionTacticsPlayer>();
			byte[] tacticIDByGroup = reader.ReadBytes(MinionTacticsPlayer.TACTICS_GROUPS_COUNT);
			byte currentTacticGroup = reader.ReadByte();
			byte ignoreTargetReticle = reader.ReadByte();
			// reading rest of packet directly into dict
			MinionTacticsGroupMapper.ReadBuffMap(reader, minionTacticsPlayer.MinionTacticsMap);

			minionTacticsPlayer.SetAllTactics(tacticIDByGroup, currentTacticGroup);
			minionTacticsPlayer.IgnoreVanillaMinionTarget = ignoreTargetReticle;

		}
	}
}
