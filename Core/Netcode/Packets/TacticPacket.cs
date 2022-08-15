using AmuletOfManyMinions.Core.Minions;
using System.IO;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	public class TacticPacket : PlayerPacket
	{
		readonly byte[] idByGroup;

		public TacticPacket() { }

		public TacticPacket(Player player, byte[] idByGroup) : base(player)
		{
			this.idByGroup = idByGroup;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write(idByGroup);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			byte[] idByGroup = reader.ReadBytes(MinionTacticsPlayer.TACTICS_GROUPS_COUNT);

			player.GetModPlayer<MinionTacticsPlayer>().SetAllTactics(idByGroup);
			if (Main.netMode == NetmodeID.Server)
			{
				new TacticPacket(player, idByGroup).Send(from: sender);
			}
		}
	}
}
