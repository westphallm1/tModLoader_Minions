using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using DemoMod.Projectiles.Minions.VoidKnife;

namespace DemoMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Neck)]
    class CharmOfManyMinions : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Increases max number of minions by 1,\n" +
                "but each minion deals slightly less damage");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 32;
            item.accessory = true;
            item.value = Item.sellPrice(gold: 5);
            item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateEquip(Player player)
        {
            player.minionDamageMult -= 0.1f;
            player.maxMinions += 1;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.LightShard, 1);
            recipe.AddIngredient(ItemID.SoulofNight, 8);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

    }
}
