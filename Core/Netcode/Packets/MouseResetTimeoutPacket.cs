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
				new MouseResetTimeoutPacket(player).Send(from: sender, bcCondition: NetUtils.EventProximityDelegate(player.Center));
			}
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			//Nothing
		}
	}
}
