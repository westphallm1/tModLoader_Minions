using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public abstract class VanillaCloneMinionItem<TBuff, TProj> : MinionItem<TBuff, TProj> where TBuff: ModBuff where TProj: Minion 
	{
		internal abstract int VanillaItemID { get;  }
		internal abstract string VanillaItemName { get;  }
		public override string Texture => "Terraria/Images/Item_" + VanillaItemID;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ItemName." + VanillaItemName) + " (AoMM Version)");
			Tooltip.SetDefault(Language.GetTextValue("ItemTooltip." + VanillaItemName));
		}

		public override void SetDefaults()
		{
			Item.CloneDefaults(VanillaItemID);
			base.SetDefaults();
		}

		public override void AddRecipes()
		{
			Recipe recipe = Recipe.Create(VanillaItemID);
			recipe.AddIngredient(Type, 1);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();

			Recipe reciprocal = Recipe.Create(Type);
			reciprocal.AddIngredient(VanillaItemID, 1);
			reciprocal.AddTile(TileID.DemonAltar);
			reciprocal.Register();
		}
	}
}
