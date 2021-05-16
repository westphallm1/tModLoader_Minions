using AmuletOfManyMinions.Core.Minions;
using System.IO;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	public class TacticPacket : PlayerPacket
	{
		readonly byte[] ids;

		public TacticPacket() { }

		public TacticPacket(Player player, byte[] ids) : base(player)
		{
			this.ids = ids;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write(ids);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			byte[] ids = reader.ReadBytes(MinionTacticsPlayer.TACTICS_GROUPS_COUNT);

			player.GetModPlayer<MinionTacticsPlayer>().SetAllTactics(ids);
			if (Main.netMode == NetmodeID.Server)
			{
				new TacticPacket(player, ids).Send(from: sender);
			}
		}
	}
}
