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
		readonly int petEmblemItem;

		public CombatPetLevelPacket() { }

		public CombatPetLevelPacket(Player player, byte petLevel, short petDamage, int petEmblemItem, object[] petModdedData) : base(player)
		{
			this.petLevel = petLevel;
			this.petDamage = petDamage;
			this.petModdedData = petModdedData;
			this.petEmblemItem = petEmblemItem;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write(petLevel);
			writer.Write(petDamage);
			writer.Write7BitEncodedInt(petEmblemItem);
			CrossMod.CombatPetSendCrossModData(writer, petModdedData);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			byte newLevel = reader.ReadByte();
			short petDamage = reader.ReadInt16();
			int petEmblemItem = reader.Read7BitEncodedInt();
			object[] petModdedStats = CrossMod.CombatPetReceiveCrossModData(reader);
			// there may be unintended consequences to setting damage to zero
			LeveledCombatPetModPlayer modPlayer = player.GetModPlayer<LeveledCombatPetModPlayer>();
			modPlayer.UpdatePetLevel(newLevel, petDamage, petEmblemItem, petModdedStats, fromSync: true);
			if (Main.netMode == NetmodeID.Server)
			{
				new CombatPetLevelPacket(player, newLevel, petDamage, petEmblemItem, petModdedStats).Send(from: sender);
			}
		}
	}
}
