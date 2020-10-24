using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.PaperSurfer
{
    public class PaperSurferMinionBuff: MinionBuff
    {
        public PaperSurferMinionBuff() : base(ProjectileType<PaperSurferMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Paper Surfer");
			Description.SetDefault("A paper surfer will fight for you!");
        }
    }

    public class PaperSurferMinionItem: MinionItem<PaperSurferMinionBuff, PaperSurferMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paper Surfer Staff");
			Tooltip.SetDefault("Summons a paper surfer to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.damage = 12;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 38;
			item.height = 40;
			item.value = Item.buyPrice(0, 0, 70, 0);
			item.rare = ItemRarityID.Green;
		}
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ClayBlock, 30);
            recipe.AddIngredient(ItemID.Cloud, 30);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
    public class PaperSurferMinion : SurferMinion<PaperSurferMinionBuff>
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            diveBombHeightRequirement = 40;
            diveBombHeightTarget = 120;
            diveBombHorizontalRange = 80;
            diveBombFrameRateLimit = 60;
            diveBombSpeed = 12;
            diveBombInertia = 15;
            approachSpeed = 8;
            approachInertia = 40;
            targetSearchDistance = 800;
            projectile.width = 28;
            projectile.height = 32;
            projectile.type = ProjectileType<PaperSurferMinion>();
        }
    }
}
