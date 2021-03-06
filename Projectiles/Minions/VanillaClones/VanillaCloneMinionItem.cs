﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public abstract class VanillaCloneMinionItem<TBuff, TProj> : MinionItem<TBuff, TProj> where TBuff: ModBuff where TProj: Minion 
	{
		internal abstract int VanillaItemID { get;  }
		internal abstract string VanillaItemName { get;  }
		static string BadgeTexture = "AmuletOfManyMinions/Projectiles/Minions/VanillaClones/AoMMBadge";
		public override string Texture => "Terraria/Item_" + VanillaItemID;
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			//Texture2D itemTexture = Main.itemTexture[item.type];
			//Texture2D texture = GetTexture(BadgeTexture);
			//Vector2 positionOffset = new Vector2(itemTexture.Width - texture.Width, itemTexture.Height - texture.Height) * scale;
			//spriteBatch.Draw(texture, position + positionOffset, texture.Bounds, drawColor, 0, origin, scale, SpriteEffects.None, 0f);
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ItemName." + VanillaItemName) + " (AoMM Version)");
			Tooltip.SetDefault(Language.GetTextValue("ItemTooltip." + VanillaItemName));
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(VanillaItemID);
			base.SetDefaults();
		}
	}
}
