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
		readonly short petDamage;

		public CombatPetLevelPacket() { }

		public CombatPetLevelPacket(Player player, byte petLevel, short petDamage) : base(player)
		{
			this.petLevel = petLevel;
			this.petDamage = petDamage;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write(petLevel);
			writer.Write(petDamage);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			byte newLevel = reader.ReadByte();
			short petDamage = reader.ReadInt16();
			// there may be unintended consequences to setting damage to zero
			player.GetModPlayer<LeveledCombatPetModPlayer>().UpdatePetLevel(newLevel, petDamage, fromSync: true);
			if (Main.netMode == NetmodeID.Server)
			{
				new CombatPetLevelPacket(player, newLevel, petDamage).Send(from: sender);
			}
		}
	}
}
