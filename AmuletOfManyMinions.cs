using AmuletOfManyMinions.Core.Netcode;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.NullHatchet;
using AmuletOfManyMinions.Projectiles.Minions.VoidKnife;
using AmuletOfManyMinions.Projectiles.Squires;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

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
			IdleLocationSets.Load();
			if (!Main.dedServ)
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
			IdleLocationSets.Unload();
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			//This should be the only thing in here
			NetHandler.HandlePackets(reader, whoAmI);
		}
	}
}
