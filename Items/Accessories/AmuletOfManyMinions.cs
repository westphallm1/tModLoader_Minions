using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using DemoMod.Projectiles.Minions.PossessedCopperSword;
using DemoMod.Projectiles.Minions.SpiritGun;
using DemoMod.Projectiles.Minions.FlyingSword;
using DemoMod.Projectiles.Minions.VoidKnife;

namespace DemoMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Neck)]
    class AmuletOfManyMinions : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("+2 Minion Slots\n" +
                "25% increased Minion Damage");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 30;
            item.accessory = true;
            item.value = Item.sellPrice(gold: 5);
            item.rare = ItemRarityID.Expert;
        }

        public override void UpdateEquip(Player player)
        {
            player.minionDamageMult += 0.25f;
            player.maxMinions += 2;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<CopperSwordMinionItem>(), 1);
            recipe.AddIngredient(ItemType<SpiritGunMinionItem>(), 1);
            recipe.AddIngredient(ItemType<FlyingSwordMinionItem>(), 1);
            recipe.AddIngredient(ItemType<VoidKnifeMinionItem>(), 1);
        }

    }
}
