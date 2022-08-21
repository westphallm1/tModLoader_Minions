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
using Terraria.GameContent;

namespace AmuletOfManyMinions.CrossModSystem.SampleMod.Pets.SampleGroundedRangedPet
{
	internal class SampleGroundedRangedPetItem : ModItem
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.Seaweed;

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.Seaweed);
			Item.shoot = ProjectileType<SampleGroundedRangedPetProjectile>();
			Item.buffType = BuffType<SampleGroundedRangedPetBuff>();
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			if(player.whoAmI == Main.myPlayer && player.itemTime ==0)
			{
				player.AddBuff(Item.buffType, 3600);
			}
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, Color.SkyBlue, 0, origin, scale, 0, 0);
			return false;
		}
	}
}
