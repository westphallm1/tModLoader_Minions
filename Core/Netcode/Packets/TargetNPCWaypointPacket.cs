using AmuletOfManyMinions.Core.Minions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	class TargetNPCWaypointPacket : PlayerPacket
	{
		private int npcIdx;
		private byte groupIdx;
		public TargetNPCWaypointPacket() { }
		public TargetNPCWaypointPacket(Player player, int npcIdx, byte groupIdx): base(player)
		{
			this.npcIdx = npcIdx;
			this.groupIdx = groupIdx;
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			int npcIdx = reader.ReadInt32();
			byte groupIdx = reader.ReadByte();
			player.GetModPlayer<MinionTacticsPlayer>().UpdateTargetNPCFromPacket(npcIdx, groupIdx);
			if(Main.netMode == NetmodeID.Server)
			{
				new TargetNPCWaypointPacket(player, npcIdx, groupIdx)
					.Send(from: sender, bcCondition: NetUtils.EventProximityDelegate(player.Center));
			}
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write(npcIdx);
			writer.Write(groupIdx);
		}
	}
}
