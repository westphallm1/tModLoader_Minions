using AmuletOfManyMinions.Core.Minions;
using System.IO;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	public class TacticPacket : PlayerPacket
	{
		readonly byte id;

		public TacticPacket() { }

		public TacticPacket(Player player, byte id) : base(player)
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

			player.GetModPlayer<MinionTacticsPlayer>().SetTactic(id);
			if (Main.netMode == NetmodeID.Server)
			{
				new TacticPacket(player, id).Send(from: sender);
			}
		}
	}
}
