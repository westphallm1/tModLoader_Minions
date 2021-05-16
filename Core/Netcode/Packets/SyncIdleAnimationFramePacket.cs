using AmuletOfManyMinions.Items.Accessories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	class SyncIdleAnimationFramePacket : PlayerPacket
	{
		readonly int idleFrame;


		public SyncIdleAnimationFramePacket() { }
		public SyncIdleAnimationFramePacket(Player player, int idleFrame) : base(player)
		{
			this.idleFrame = idleFrame;
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			int idleFrame = reader.ReadInt32();
			MinionSpawningItemPlayer modPlayer = player.GetModPlayer<MinionSpawningItemPlayer>();
			modPlayer.idleMinionSyncronizationFrame = idleFrame;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write(idleFrame);
		}
	}
}
