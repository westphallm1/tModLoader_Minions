using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.CrossModSystem.SampleMod.Pets.SampleFlyingRangedPet
{
	internal class SampleFlyingRangedPetItem : ModItem
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.ZephyrFish;

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.ZephyrFish);
			Item.shoot = ProjectileType<SampleFlyingRangedPetProjectile>();
			Item.buffType = BuffType<SampleFlyingRangedPetBuff>();
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			if(player.whoAmI == Main.myPlayer && player.itemTime ==0)
			{
				player.AddBuff(Item.buffType, 3600);
			}
		}
	}
}
