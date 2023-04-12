using AmuletOfManyMinions.CrossModClient.SummonersShine;
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
	class CombatPetLevelModdedPacket : PlayerPacket
	{
		readonly object[] petModdedData;
		readonly int petEmblemItem;

		public CombatPetLevelModdedPacket() { }

		public CombatPetLevelModdedPacket(Player player, int petEmblemItem, object[] petModdedData) : base(player)
		{
			this.petModdedData = petModdedData;
			this.petEmblemItem = petEmblemItem;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write7BitEncodedInt(petEmblemItem);
			CrossModSetup.CombatPetSendCrossModData(writer, petModdedData);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			int petEmblemItem = reader.Read7BitEncodedInt();
			object[] petModdedStats = CrossModSetup.CombatPetReceiveCrossModData(reader);
      
			LeveledCombatPetModPlayer modPlayer = player.GetModPlayer<LeveledCombatPetModPlayer>();
			modPlayer.UpdatePetLevelModded(petEmblemItem, petModdedStats, fromSync: true);
			if (Main.netMode == NetmodeID.Server)
			{
				new CombatPetLevelModdedPacket(player, petEmblemItem, petModdedStats).Send(from: sender);
			}
		}
	}
}
