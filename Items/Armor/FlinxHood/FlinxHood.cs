using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Armor.FlinxHood
{
	[AutoloadEquip(EquipType.Head)]
	public class FlinxFurHood : ModItem
	{
		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization("SetBonus");
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 3);
			Item.rare = ItemRarityID.Green;
			Item.defense = 3;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemID.FlinxFurCoat;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = SetBonusText.ToString();
			player.GetModPlayer<MinionSpawningItemPlayer>().flinxArmorSetEquipped = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Silk, 8).AddIngredient(ItemID.FlinxFur, 6)
				.AddRecipeGroup("AmuletOfManyMinions:Golds", 8).AddTile(TileID.Loom).Register();
		}
	}
}