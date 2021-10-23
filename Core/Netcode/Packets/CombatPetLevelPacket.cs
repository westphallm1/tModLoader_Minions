using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
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
	class CombatPetLevelPacket : PlayerPacket
	{
		// pet level is bounded to (0, 255)
		readonly byte petLevel;

		public CombatPetLevelPacket() { }

		public CombatPetLevelPacket(Player player, byte petLevel) : base(player)
		{
			this.petLevel = petLevel;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write(petLevel);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			byte newLevel = reader.ReadByte();
			// there may be unintended consequences to setting damage to zero
			player.GetModPlayer<LeveledCombatPetModPlayer>().UpdatePetLevel(newLevel, 0, fromSync: true);
			if (Main.netMode == NetmodeID.Server)
			{
				new CombatPetLevelPacket(player, newLevel).Send(from: sender);
			}
		}
	}
}
