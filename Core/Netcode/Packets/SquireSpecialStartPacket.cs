using AmuletOfManyMinions.Projectiles.Squires;
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
	class SquireSpecialStartPacket : PlayerPacket
	{

		public SquireSpecialStartPacket() { }
		public SquireSpecialStartPacket(Player player) : base(player) { }
		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			((SquireMinion)player.GetModPlayer<SquireModPlayer>().GetSquire()?.ModProjectile)?.StartSpecial(true);
			if(Main.netMode == NetmodeID.Server)
			{
				new SquireSpecialStartPacket(player)
					.Send(from: sender, bcCondition: NetUtils.EventProximityDelegate(player.Center));
			}
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			// no-op, simply indicate that it happened
		}
	}
}
