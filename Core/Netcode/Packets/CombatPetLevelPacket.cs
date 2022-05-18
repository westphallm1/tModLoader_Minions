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
		readonly object[] petModdedData;

		public CombatPetLevelPacket() { }

		public CombatPetLevelPacket(Player player, byte petLevel, short petDamage, object[] petModdedData) : base(player)
		{
			this.petLevel = petLevel;
			this.petDamage = petDamage;
			this.petModdedData = petModdedData;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write(petLevel);
			writer.Write(petDamage);
			CrossMod.CombatPetSendCrossModData(writer, petModdedData);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			byte newLevel = reader.ReadByte();
			short petDamage = reader.ReadInt16();
			object[] petModdedStats = CrossMod.CombatPetReceiveCrossModData(reader);
			// there may be unintended consequences to setting damage to zero
			LeveledCombatPetModPlayer modPlayer = player.GetModPlayer<LeveledCombatPetModPlayer>();
			modPlayer.UpdatePetLevel(newLevel, petDamage, petModdedStats, fromSync: true);
			if (Main.netMode == NetmodeID.Server)
			{
				new CombatPetLevelPacket(player, newLevel, petDamage, petModdedStats).Send(from: sender);
			}
		}
	}
}
