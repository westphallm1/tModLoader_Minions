using AmuletOfManyMinions.Core.Minions;
using System.IO;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Netcode.Packets
{
	/// <summary>
	/// Packet representing mod config options that need to be synced in multiplayer
	/// </summary>
	public class ConfigPacket : PlayerPacket
	{
		readonly byte ignoreTargetReticle;

		public ConfigPacket() { }

		public ConfigPacket(Player player, byte ignoreTargetReticle) : base(player)
		{
			this.ignoreTargetReticle = ignoreTargetReticle;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write((byte)ignoreTargetReticle);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			byte ignoreTargetReticle = reader.ReadByte();

			player.GetModPlayer<MinionTacticsPlayer>().IgnoreVanillaMinionTarget = ignoreTargetReticle;
			if (Main.netMode == NetmodeID.Server)
			{
				new ConfigPacket(player, ignoreTargetReticle).Send(from: sender);
			}
		}
	}
}
