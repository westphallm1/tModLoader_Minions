using DemoMod.Projectiles.Minions.MinonBaseClasses;
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.Acorn
{
    public class AcornMinionBuff: MinionBuff
    {
        public AcornMinionBuff() : base(ProjectileType<AcornMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Acorn");
			Description.SetDefault("A winged acorn will fight for you!");
        }
    }

    public class AcornMinionItem: MinionItem<AcornMinionBuff, AcornMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Acorn Staff");
			Tooltip.SetDefault("Summons a winged acorn to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.damage = 6;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 2, 0);
			item.rare = ItemRarityID.White;
		}
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Acorn, 3);
            recipe.AddIngredient(ItemID.Wood, 10);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
    public class AcornMinion : SurferMinion<AcornMinionBuff>
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Flying Acorn");
			Main.projFrames[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            diveBombHeightRequirement = 40;
            diveBombHeightTarget = 120;
            diveBombHorizontalRange = 80;
            diveBombFrameRateLimit = 60;
            diveBombSpeed = 12;
            diveBombInertia = 15;
            approachSpeed = 7;
            approachInertia = 40;
            idleCircle = 25;
            targetSearchDistance = 500;
            projectile.width = 44;
            projectile.height = 20;
            projectile.type = ProjectileType<AcornMinion>();
        }
		public override void Animate(int minFrame = 0, int? maxFrame = null) {

			int frameSpeed = 5;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed) {
                projectile.frameCounter = 0;
                projectile.frame++;
                if(projectile.frame >= Main.projFrames[projectile.type])
                {
                    projectile.frame = 0;
                }
			}
		}
    }
}
