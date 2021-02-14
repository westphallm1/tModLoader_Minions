using AmuletOfManyMinions.Core.Minions;
using System.IO;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	public class TacticPacket : PlayerPacket
	{
		readonly byte id;
		readonly byte ignoreTargetReticle;

		public TacticPacket() { }

		public TacticPacket(Player player, byte id, byte ignoreTargetReticle) : base(player)
		{
			this.id = id;
			this.ignoreTargetReticle = ignoreTargetReticle;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write((byte)id);
			writer.Write((byte)ignoreTargetReticle);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			byte id = reader.ReadByte();
			byte ignoreTargetReticle = reader.ReadByte();

			player.GetModPlayer<MinionTacticsPlayer>().SetTactic(id);
			player.GetModPlayer<MinionTacticsPlayer>().IgnoreVanillaMinionTarget = ignoreTargetReticle;
			if (Main.netMode == NetmodeID.Server)
			{
				new TacticPacket(player, id, ignoreTargetReticle).Send(from: sender);
			}
		}
	}
}
