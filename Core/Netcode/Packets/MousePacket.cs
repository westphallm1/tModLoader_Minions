using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	public class MousePacket : MPPacket
	{
		readonly byte whoAmI;

		readonly ushort x;
		readonly ushort y;

		//To reduce sizes (ushort instead of float)
		private const int precision = 16;

		//Picking ushort because it's sufficient for Main.maxTilesX * 16 / precision (maximum maxTilesX is 8400 or so, ushort max is 65k)

		//For reflection
		public MousePacket() { }

		public MousePacket(int whoAmI, Vector2 position)
		{
			this.whoAmI = (byte)whoAmI;
			x = (ushort)(position.X / precision);
			y = (ushort)(position.Y / precision);
		}

		public override void Send(BinaryWriter writer, int to = -1, int from = -1)
		{
			writer.Write((byte)whoAmI);
			writer.Write((ushort)x);
			writer.Write((ushort)y);
		}

		public override void Receive(BinaryReader reader, int sender)
		{
			byte whoAmI = reader.ReadByte();
			ushort x = reader.ReadUInt16();
			ushort y = reader.ReadUInt16();

			Vector2 position = new Vector2(x, y) * precision;

			Player player = Main.player[whoAmI];
			player.GetModPlayer<MousePlayer>().SetNextMousePosition(position);

			if (Main.netMode == NetmodeID.Server)
			{
				new MousePacket(whoAmI, position).Send(from: sender);
			}
		}
	}
}
