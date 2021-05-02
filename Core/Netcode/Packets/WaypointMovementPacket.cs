using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Pathfinding;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	public class WaypointMovementPacket : PlayerPacket
	{
		readonly short xOffset;
		readonly short yOffset;
		readonly byte tacticsGroup;

		public WaypointMovementPacket() { }

		public WaypointMovementPacket(Player player, Vector2 position, byte tacticsGroup) : base(player)
		{
			xOffset = (short)(position.X - player.Center.X);
			yOffset = (short)(position.Y - player.Center.Y);
			this.tacticsGroup = tacticsGroup;
		}

		public WaypointMovementPacket(Player player, short xOffset, short yOffset, byte tacticsGroup) : base(player)
		{
			this.xOffset = xOffset;
			this.yOffset = yOffset;
			this.tacticsGroup = tacticsGroup;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write(xOffset);
			writer.Write(yOffset);
			writer.Write(tacticsGroup);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			short xOffset = reader.ReadInt16();
			short yOffset = reader.ReadInt16();
			byte tacticsGroup = reader.ReadByte();

			player.GetModPlayer<MinionPathfindingPlayer>().UpdateWaypointFromPacket(xOffset, yOffset, tacticsGroup);
			if (Main.netMode == NetmodeID.Server)
			{
				new WaypointMovementPacket(player, xOffset, yOffset, tacticsGroup).Send(from: sender, bcCondition: delegate (Player otherPlayer)
				{
					//Only send to other player if he's in visible range
					Rectangle bounds = Utils.CenteredRectangle(player.Center, new Vector2(1920, 1080) * 1.5f);
					Point otherPlayerCenter = otherPlayer.Center.ToPoint();
					return bounds.Contains(otherPlayerCenter);
				});
			}
		}
	}
}
