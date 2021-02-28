using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	public class MouseResetTimeoutPacket : PlayerPacket
	{
		//For reflection
		public MouseResetTimeoutPacket() { }

		public MouseResetTimeoutPacket(Player player) : base(player)
		{

		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			player.GetModPlayer<MousePlayer>().ResetTimeout();

			if (Main.netMode == NetmodeID.Server)
			{
				new MouseResetTimeoutPacket(player).Send(from: sender, bcCondition: delegate (Player otherPlayer)
				{
					//Only send to other player if he's in visible range
					Rectangle bounds = Utils.CenteredRectangle(player.Center, new Vector2(1920, 1080) * 1.5f);
					Point otherPlayerCenter = otherPlayer.Center.ToPoint();
					return bounds.Contains(otherPlayerCenter);
				});
			}
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			//Nothing
		}
	}
}
