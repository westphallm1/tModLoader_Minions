using AmuletOfManyMinions.Core.Minions;
using System.IO;
using Terraria;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	public class SyncMinionTacticsPlayerPacket : PlayerPacket
	{
		readonly byte id;
		//mirror more fields here that have to be synced on join for MinionTacticsPlayer

		public SyncMinionTacticsPlayerPacket() { }

		public SyncMinionTacticsPlayerPacket(Player player, byte id) : base(player)
		{
			this.id = id;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write((byte)id);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			byte id = reader.ReadByte();

			player.GetModPlayer<MinionTacticsPlayer>().SetTactic(id, fromSync: true);
		}
	}
}
