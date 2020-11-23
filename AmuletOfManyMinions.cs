using AmuletOfManyMinions.Core.Netcode;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Squires;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using static AmuletOfManyMinions.Items.Armor.RoyalArmor.RoyalGown;

namespace AmuletOfManyMinions
{
	public class AmuletOfManyMinions : Mod
	{
		public override void Load()
		{
			NetHandler.Load();
			NPCSets.Load();
			SquireMinionTypes.Load();
			NecromancerAccessory.Load();
			SquireGlobalProjectile.Load();
			if(!Main.dedServ)
			{
				AddEquipTexture(null, EquipType.Legs, "RoyalGown_Legs", "AmuletOfManyMinions/Items/Armor/RoyalArmor/RoyalGown_Legs");
			}
		}

		public override void PostSetupContent()
		{
			NPCSets.Populate();
		}

		public override void Unload()
		{
			NetHandler.Unload();
			NPCSets.Unload();
			SquireMinionTypes.Unload();
			NecromancerAccessory.Unload();
			SquireGlobalProjectile.Unload();
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			//This should be the only thing in here
			NetHandler.HandlePackets(reader, whoAmI);
		}
	}
}
