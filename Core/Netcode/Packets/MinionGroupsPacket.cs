using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Tactics;
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
	class MinionGroupsPacket : PlayerPacket
	{
		private Dictionary<int, int> tacticsMap;
		public MinionGroupsPacket() { }
		public MinionGroupsPacket(Player player, Dictionary<int, int> groupsMap) : base(player)
		{
			this.tacticsMap = groupsMap;	
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			MinionTacticsGroupMapper.ReadBuffMap(reader, out Dictionary<int, int> destDict);
			MinionTacticsPlayer modPlayer = player.GetModPlayer<MinionTacticsPlayer>();
			foreach(var pair in destDict)
			{
				modPlayer.MinionTacticsMap[pair.Key] = pair.Value;
			}
			// TODO pass the raw byte array, rather than deconstructing/reconstructing the dest dict
			if (Main.netMode == NetmodeID.Server)
			{
				new MinionGroupsPacket(player, destDict).Send(from: sender);
			}
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			MinionTacticsGroupMapper.WriteBuffMap(writer, tacticsMap);
		}
	}
}
