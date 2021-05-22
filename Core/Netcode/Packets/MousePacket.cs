using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	public class MousePacket : PlayerPacket
	{
		readonly ushort x;
		readonly ushort y;

		//To reduce sizes (ushort instead of float)
		private const int precision = 16;

		//Picking ushort because it's sufficient for Main.maxTilesX * 16 / precision (maximum maxTilesX is 8400 or so, ushort max is 65k)

		//For reflection
		public MousePacket() { }

		public MousePacket(Player player, Vector2 position) : base(player)
		{
			x = (ushort)(position.X / precision);
			y = (ushort)(position.Y / precision);
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write((ushort)x);
			writer.Write((ushort)y);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			ushort x = reader.ReadUInt16();
			ushort y = reader.ReadUInt16();

			Vector2 position = new Vector2(x, y) * precision;

			player.GetModPlayer<MousePlayer>().SetNextMousePosition(position);

			if (Main.netMode == NetmodeID.Server)
			{
				new MousePacket(player, position).Send(from: sender, bcCondition: NetUtils.EventProximityDelegate(position));
			}
		}
	}
}
